using Prius.Contracts.Interfaces;

namespace Prius.Orm.Connections
{
    public class ContextFactory: IContextFactory
    {
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;
        private readonly IDataReaderFactory _dataReaderFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IErrorReporter _errorReporter;

        public ContextFactory(
            IDataEnumeratorFactory dataEnumeratorFactory,
            IDataReaderFactory dataReaderFactory,
            IRepositoryFactory repositoryFactory,
            IErrorReporter errorReporter)
        {
            _dataEnumeratorFactory = dataEnumeratorFactory;
            _dataReaderFactory = dataReaderFactory;
            _repositoryFactory = repositoryFactory;
            _errorReporter = errorReporter;
        }

        public IContextFactory Initialize()
        {
            return this;
        }

        public IContext Create(string repositoryName)
        {
            return new Context(_dataEnumeratorFactory, _dataReaderFactory, _errorReporter)
                .Initialize(_repositoryFactory.Create(repositoryName));
        }
    }
}
