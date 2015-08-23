using System;
using System.Collections.Generic;
using System.Linq;
using Prius.Contracts.Interfaces;
using Prius.Orm.Config;
using Prius.Orm.Utility;
using Urchin.Client.Interfaces;

namespace Prius.Orm.Connections
{
    public class RepositoryFactory : Disposable, IRepositoryFactory
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConfigurationStore _configurationStore;

        private IDisposable _configChangeNotifier;
        private Dictionary<string, IRepository> _repositories;

        public RepositoryFactory(
            IConnectionFactory connectionFactory,
            IConfigurationStore configurationStore)
        {
            _connectionFactory = connectionFactory;

            _configChangeNotifier = configurationStore.Register<DataAccessLayer>("/prius", ConfigurationChanged);
        }

        protected override void Dispose(bool destructor)
        {
            if (!destructor)
                _configChangeNotifier.Dispose();

            base.Dispose(destructor);
        }

        private void ConfigurationChanged(DataAccessLayer config)
        {
            var oldRepositories = _repositories;

            _repositories = config.Repositories.ToDictionary(
                r => r.Name.ToLower(), 
                r => new Repository(_connectionFactory, _configurationStore).Initialize(r.Name));

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
            var repositories = _repositories;

            lock (repositories)
            {
                IRepository repository;
                if (repositories.TryGetValue(repositoryName.ToLower(), out repository))
                    return repository;
                throw new Exception("No configuration for repository '" + repositoryName + "' in /dataAccessLayer");
            }
        }
    }
}
