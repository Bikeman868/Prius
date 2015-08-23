using Prius.Contracts.Interfaces;

namespace Prius.Orm.Connections
{
    public class ContextFactory: IContextFactory
    {
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IErrorReporter _errorReporter;

        public ContextFactory(
            IDataEnumeratorFactory dataEnumeratorFactory,
            IRepositoryFactory repositoryFactory,
            IErrorReporter errorReporter)
        {
            _dataEnumeratorFactory = dataEnumeratorFactory;
            _repositoryFactory = repositoryFactory;
            _errorReporter = errorReporter;
        }

        public IContext Create(string repositoryName)
        {
            return new Context(
                _dataEnumeratorFactory, 
                _errorReporter,
                _repositoryFactory.Create(repositoryName));
        }
    }
}
