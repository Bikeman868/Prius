using System;
using System.Collections.Generic;
using System.Linq;
using Prius.Contracts.Exceptions;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Orm.Config;
using Prius.Orm.Utility;
using Urchin.Client.Interfaces;

namespace Prius.Orm.Connections
{
    public class RepositoryFactory : Disposable, IRepositoryFactory
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConfigurationStore _configurationStore;

        private readonly IDisposable _configChangeNotifier;
        private Dictionary<string, IRepository> _repositories;
        private ITraceWriterFactory _traceWriterFactory;

        public RepositoryFactory(
            IConnectionFactory connectionFactory,
            IConfigurationStore configurationStore)
        {
            _connectionFactory = connectionFactory;
            _configurationStore = configurationStore;

            _configChangeNotifier = configurationStore.Register<PriusConfig>("/prius", ConfigurationChanged);
        }

        protected override void Dispose(bool destructor)
        {
            if (!destructor)
                _configChangeNotifier.Dispose();

            base.Dispose(destructor);
        }

        private void ConfigurationChanged(PriusConfig config)
        {
            var oldRepositories = _repositories;

            if (config == null || config.Repositories == null || config.Repositories.Count == 0)
            {
                _repositories = new Dictionary<string, IRepository>();
            }
            else
            {
                _repositories = config.Repositories.ToDictionary(
                    r => r.Name.ToLower(),
                    r => new Repository(_connectionFactory, _configurationStore).Initialize(r.Name));
            }

            if (oldRepositories != null)
            {
                lock (oldRepositories)
                {
                    foreach (var repo in oldRepositories.Values)
                        repo.Dispose();
                }
            }
        }

        public IRepository Create(string repositoryName)
        {
            if (string.IsNullOrEmpty(repositoryName))
                throw new PriusException("No Prius repository name was specified, please check your configuration");

            var repositories = _repositories;

            if (repositories == null)
                throw new PriusException("No repositories are configured in /prius/repositories");

            lock (repositories)
            {
                IRepository repository;
                if (repositories.TryGetValue(repositoryName.ToLower(), out repository))
                {
                    repository.EnableTracing(_traceWriterFactory);
                    return repository;
                }
                throw new PriusException("No configuration for repository '" + repositoryName + "' in /prius/repositories");
            }
        }

        public void EnableTracing(ITraceWriterFactory traceWriterFactory)
        {
            _traceWriterFactory = traceWriterFactory;
        }
    }
}
