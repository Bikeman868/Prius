using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.Factory;
using Prius.Contracts.Interfaces.Utility;
using Prius.Orm.Config;
using Prius.Orm.Utility;
using Urchin.Client.Interfaces;

namespace Prius.Orm.Connections
{
    public class Repository : Disposable, IRepository
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConfigurationStore _configurationStore;

        public string Name { get; private set; }

        private IDisposable _configChangeNotifier;
        private Group[] _groups;

        public Repository(
            IConnectionFactory connectionFactory,
            IConfigurationStore configurationStore)
        {
            _connectionFactory = connectionFactory;
            _configurationStore = configurationStore;
        }

        public IRepository Initialize(string name)
        {
            Name = name;
            _configChangeNotifier = _configurationStore.Register<PriusConfig>("/prius", ConfigurationChanged);
            return this;
        }

        protected override void Dispose(bool destructor)
        {
            if (!destructor)
                _configChangeNotifier.Dispose();

            base.Dispose(destructor);
        }

        private void ConfigurationChanged(PriusConfig config)
        {
            Action nullConfig = () =>
                {
                    _groups = new Group[0];
                };

            if (config == null || config.Repositories == null)
            {
                nullConfig();
                return;
            }

            var repositoryConfiguration = config.Repositories.FirstOrDefault(r => string.Equals(r.Name, Name, StringComparison.InvariantCultureIgnoreCase));
            if (repositoryConfiguration == null)
            {
                nullConfig();
                return;
            }

            var fallbackPolicies = config.FallbackPolicies == null 
                ? new Dictionary<string, FallbackPolicy>() 
                : config.FallbackPolicies.ToDictionary(p => p.Name);

            var databases = config.Databases == null
                ? new Dictionary<string, Database>()
                : config.Databases.ToDictionary(s => s.Name);

            var groups = repositoryConfiguration.Clusters
                .Where(cluster => cluster.Enabled)
                .OrderBy(cluster => cluster.SequenceNumber)
                .Select(cluster =>
                {
                    FallbackPolicy fallbackPolicy;
                    if (!fallbackPolicies.TryGetValue(cluster.FallbackPolicyName, out fallbackPolicy))
                        fallbackPolicy = new FallbackPolicy();

                    var servers = cluster.DatabaseNames
                        .Select(databaseName =>
                        {
                            Database database;
                            return databases.TryGetValue(databaseName, out database)
                                ? database
                                : null;
                        })
                        .Where(database => database != null && database.Enabled)
                        .OrderBy(database => database.SequenceNumber)
                        .Select(database => new Server(
                            database.ServerType, 
                            database.ConnectionString, 
                            database.StoredProcedures == null ? null : database.StoredProcedures.ToDictionary(p => p.Name.ToLower(), p => p.TimeoutSeconds)));

                    return new Group().Initialize
                        (
                            this,
                            fallbackPolicy.FailureWindowSeconds,
                            fallbackPolicy.AllowedFailurePercent / 100f,
                            fallbackPolicy.WarningFailurePercent / 100f,
                            fallbackPolicy.BackOffTime,
                            servers
                        );
                });
            _groups = groups.ToArray();
        }

        public IConnection GetConnection(ICommand command)
        {
            var server = GetOnlineServer();

            if (server == null)
                return null;

            if (!command.TimeoutSeconds.HasValue)
                SetTimeout(command, server);

            IConnection result = null;
            switch (server.ServerType)
            {
                case ServerType.SqlServer:
                    result = _connectionFactory.CreateSqlServer(this, command, server.ConnectionString);
                    break;
                case ServerType.MySql:
                    result = _connectionFactory.CreateMySql(this, command, server.ConnectionString);
                    break;
                case ServerType.Redis:
                    result = _connectionFactory.CreateRedis(this, command, server.HostName, server.InstanceName);
                    break;
                case ServerType.PostgreSQL:
                    result = _connectionFactory.CreatePostgreSql(this, command, server.ConnectionString, server.SchemaName);
                    break;
                default:
                    server.Group.RecordFailure(server);
                    break;
            }

            if (result != null) result.RepositoryContext = server;
            return result;
        }

        public void RecordSuccess(IConnection connection, double elapsedSeconds)
        {
            if (connection != null)
            {
                var server = connection.RepositoryContext as Server;
                if (server != null)
                    server.Group.RecordSuccess(server, elapsedSeconds);
            }
        }

        public void RecordFailure(IConnection connection)
        {
            if (connection != null)
            {
                var server = connection.RepositoryContext as Server;
                if (server != null)
                    server.Group.RecordFailure(server);
            }
        }

        public override string ToString()
        {
            var groups = _groups;
            var sb = new StringBuilder("Repository: ");
            sb.AppendFormat("Name='{0}'; ", Name);
            for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                sb.AppendFormat("Group[{0}]={1}; ", groupIndex, groups[groupIndex].IsOnline() ? "online" : "failed");
            }
            return sb.ToString();
        }

        private void SetTimeout(ICommand command, Server server)
        {
            const int defaultTimeout = 5;

            if (ReferenceEquals(command, null))
                return;

            if (ReferenceEquals(server, null) || string.IsNullOrEmpty(command.CommandText))
            {
                command.TimeoutSeconds = defaultTimeout;
                return;
            }

            var key = command.CommandText.ToLower();
            lock (server.CommandTimeouts)
            {
                int timeout;
                command.TimeoutSeconds = server.CommandTimeouts.TryGetValue(key, out timeout) ? timeout : defaultTimeout;
            }
        }

        private void RecordFailover(Group group)
        {
        }

        private void RecordWarning(Group group, string warning)
        {
        }

        private Server GetOnlineServer()
        {
            var groups = _groups;
            for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                var group = groups[groupIndex];
                lock (group)
                {
                    if (group.IsOnline())
                    {
                        var server = group.ChooseServer();
                        if (server != null) return server;
                    }
                }
            }
            return null;
        }

        private class Group
        {
            private Repository _repository;
            private IHistoryBucketQueue<HistoryBucket, bool> _history;
            private DateTime? _retryAt;
            private DateTime _evaluateAt;
            private float _failureWindowSeconds;
            private float _allowedFailureRate;
            private float _warningFailureRate;
            private TimeSpan _backOffTime;
            private Server[] _servers;
            private int _nextServerIndex;

            public Group Initialize(
                Repository repository, 
                float failureWindowSeconds, 
                float allowedFailureRate, 
                float warningFailureRate, 
                TimeSpan backOffTime, 
                IEnumerable<Server> servers)
            {
                _repository = repository;
                _failureWindowSeconds = failureWindowSeconds;
                _allowedFailureRate = allowedFailureRate;
                _warningFailureRate = warningFailureRate;
                _backOffTime = backOffTime;
                _servers = servers.ToArray();

                foreach (var server in _servers) server.Group = this;

                _history = new HistoryBucketQueue<HistoryBucket, bool>(
                    TimeSpan.FromSeconds(failureWindowSeconds),
                    20,
                    () => new HistoryBucket(),
                    (bucket, item) => bucket.Add(item),
                    (bucket, ticks) => bucket.Clear(ticks));

                _retryAt = null;
                _evaluateAt = DateTime.UtcNow.AddSeconds(_failureWindowSeconds);
                return this;
            }

            public void RecordSuccess(Server server, double elapsedSeconds)
            {
                _history.Add(true);
            }

            public void RecordFailure(Server server)
            {
                _history.Add(false);
            }

            public bool IsOnline()
            {
                var now = DateTime.UtcNow;

                if (_retryAt.HasValue)
                {
                    if (now >= _retryAt.Value)
                    {
                        _evaluateAt = now.AddSeconds(_failureWindowSeconds / 5);
                        _history.Clear();
                        _retryAt = null;
                        return true;
                    }
                    return false;
                }

                if (now >= _evaluateAt)
                {
                    var failureRate = CalculateFailureRate();
                    if (failureRate > _allowedFailureRate)
                    {
                        _repository.RecordFailover(this);
                        _retryAt = now + _backOffTime;
                        return false;
                    }
                    if (failureRate > _warningFailureRate)
                    {
                        _repository.RecordWarning(this, "Failure rate exceeds warning limit of " + _warningFailureRate + " winthin " + _failureWindowSeconds + " seconds");
                    }
                    _evaluateAt = now.AddSeconds(_failureWindowSeconds / 5);
                }

                return true;
            }

            public Server ChooseServer()
            {
                if (_servers.Length == 0) return null;

                // it doesn't matter too much about thread safety here, so long as
                // all of the servers see some action, not a problem if it's not
                // exactly true round robin.
                _nextServerIndex = (_nextServerIndex + 1) % _servers.Length;
                return _servers[_nextServerIndex];
            }

            private float CalculateFailureRate()
            {
                var totalSuccess = 0;
                var totalFail = 0;
                foreach (var bucket in _history)
                {
                    totalSuccess += bucket.SuccessCount;
                    totalFail += bucket.FailCount;
                }
                var total = totalSuccess + totalFail;
                if (total == 0) return 0;
                return (float)totalFail / total;
            }

            private class HistoryBucket
            {
                public int SuccessCount;
                public int FailCount;

                public void Add(bool success)
                {
                    if (success) SuccessCount++;
                    else FailCount++;
                }

                public void Clear(long ticks)
                {
                    SuccessCount = 0;
                    FailCount = 0;
                }
            }
        }

        private class Server
        {
            public Group Group { get; set; }
            public ServerType ServerType { get; private set; }
            public string ConnectionString { get; private set; }
            public string HostName { get; private set; }
            public string InstanceName { get; private set; }
            public string SchemaName { get; private set; }
            public IDictionary<string, int> CommandTimeouts { get; private set; }

            public Server(ServerType serverType, string connectionString, IDictionary<string, int> commandTimeouts)
            {
                ServerType = serverType;
                ConnectionString = connectionString;
                CommandTimeouts = commandTimeouts ?? new Dictionary<string, int>();

                var connectionParameters = connectionString.Split(';')
                    .Where(cs => !string.IsNullOrEmpty(cs) && cs.Contains('='))
                    .Select(cs => new { Key = cs.Substring(0, cs.IndexOf('=')), Value = cs.Substring(cs.IndexOf('=') + 1) })
                    .ToDictionary(t => t.Key);

                if (serverType == ServerType.Redis)
                {
                    HostName = connectionParameters["server"].Value;
                    InstanceName = connectionParameters["database"].Value;
                }
                else if (serverType == ServerType.PostgreSQL)
                {
                    if (connectionParameters.ContainsKey("schema"))
                    {
                        SchemaName = connectionParameters["schema"].Value;
                        ConnectionString = ConnectionString.Replace(string.Format(";schema={0}", SchemaName), "");
                    }
                }
            }
        }
    }
}
