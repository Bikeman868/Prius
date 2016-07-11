using System;
using Moq.Modules;
using Prius.Contracts.Interfaces;
using Moq;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Mocks.Helper;

namespace Prius.Mocks
{
    public class MockContextFactory : MockImplementationProvider<IContextFactory>
    {
        public IMockedRepository MockedRepository { get; set; }

        protected override void SetupMock(IMockProducer mockProducer, Mock<IContextFactory> mock)
        {
            mock.Setup(m => m.Create(It.IsAny<string>()))
                .Returns((string repositoryName) => new Context(MockedRepository));
        }

        private class Context : IContext
        {
            private IMockedRepository _mockedRepository;

            public Context(IMockedRepository mockedRepository)
            {
                _mockedRepository = mockedRepository;
            }

            public void BeginTransaction()
            {
            }

            public void Commit()
            {
            }

            public void Rollback()
            {
            }

            public bool ReportErrors { get; set; }

            public IConnection PrepareCommand(ICommand command)
            {
                return new Connection(_mockedRepository, command);
            }

            public IAsyncResult BeginExecuteReader(ICommand command, AsyncCallback callback)
            {
                return new SyncronousResult(command, callback);
            }

            public IDataReader EndExecuteReader(IAsyncResult asyncResult)
            {
                return PrepareCommand(asyncResult.AsyncState as ICommand).EndExecuteReader(asyncResult);
            }

            public IAsyncResult BeginExecuteEnumerable(ICommand command, AsyncCallback callback)
            {
                return PrepareCommand(command).BeginExecuteEnumerable(callback);
            }

            public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult, string dataSetName, IFactory<T> dataContractFactory) where T : class
            {
                return PrepareCommand(asyncResult.AsyncState as ICommand).EndExecuteEnumerable<T>(asyncResult);
            }

            public IAsyncResult BeginExecuteNonQuery(ICommand command, AsyncCallback callback)
            {
                return new SyncronousResult(command, callback);
            }

            public long EndExecuteNonQuery(IAsyncResult asyncResult)
            {
                return PrepareCommand(asyncResult.AsyncState as ICommand).EndExecuteNonQuery(asyncResult);
            }

            public IAsyncResult BeginExecuteScalar(ICommand command, AsyncCallback callback)
            {
                return new SyncronousResult(command, callback);
            }

            public T EndExecuteScalar<T>(IAsyncResult asyncResult)
            {
                return PrepareCommand(asyncResult.AsyncState as ICommand).EndExecuteScalar<T>(asyncResult);
            }

            public bool IsReusable { get { return false; } }

            public bool IsDisposing { get { return false; } }

            private bool IsDisposed { get; set; }

            public void Dispose()
            {
                IsDisposed = true;
            }

            public IDataReader ExecuteReader(ICommand command)
            {
                return EndExecuteReader(BeginExecuteReader(command, null));
            }

            public IDataEnumerator<T> ExecuteEnumerable<T>(
                ICommand command, 
                string dataSetName = null, 
                IFactory<T> dataContractFactory = null) where T : class
            {
                return EndExecuteEnumerable(BeginExecuteEnumerable(command, null), dataSetName, dataContractFactory);
            }

            public long ExecuteNonQuery(ICommand command)
            {
                return EndExecuteNonQuery(BeginExecuteNonQuery(command, null));
            }

            public T ExecuteScalar<T>(ICommand command)
            {
                return EndExecuteScalar<T>(BeginExecuteScalar(command, null));
            }
        }
    }
}
