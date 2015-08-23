using System;
using System.Collections;
using System.Collections.Generic;
using Moq;
using Moq.Modules;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;
using Prius.Mocks.Helper;

namespace Prius.Mocks
{
    public class MockConnectionFactory: MockImplementationProvider<IConnectionFactory>
    {
        private readonly CommandFunctions _functions = new CommandFunctions();

        public void AddQueryFunction(ICommand command, Func<ICommand, JArray> func)
        {
            _functions.QueryFunctions.Add(command, func);
        }

        public void AddNonQueryFunction(ICommand command, Func<ICommand, long> func)
        {
            _functions.NonQueryFunctions.Add(command, func);
        }

        public void AddScalarFunction(ICommand command, Func<ICommand, object> func)
        {
            _functions.ScalarFunctions.Add(command, func);
        }

        protected override void SetupMock(IMockProducer mockProducer, Mock<IConnectionFactory> mock)
        {
            mock.Setup(cf => cf.CreateMySql(It.IsAny<IRepository>(), It.IsAny<ICommand>(), It.IsAny<string>()))
                .Returns((IRepository r, ICommand cmd, string c) => new Connection(r, cmd, _functions));

            mock.Setup(cf => cf.CreatePostgreSql(It.IsAny<IRepository>(), It.IsAny<ICommand>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRepository r, ICommand cmd, string c, string schema) => new Connection(r, cmd, _functions));

            mock.Setup(cf => cf.CreateRedis(It.IsAny<IRepository>(), It.IsAny<ICommand>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRepository r, ICommand cmd, string host, string repo) => new Connection(r, cmd, _functions));

            mock.Setup(cf => cf.CreateSqlServer(It.IsAny<IRepository>(), It.IsAny<ICommand>(), It.IsAny<string>()))
                .Returns((IRepository r, ICommand cmd, string c) => new Connection(r, cmd, _functions));
        }

        private class CommandFunctions
        {
            public readonly Dictionary<ICommand, Func<ICommand, JArray>> QueryFunctions = new Dictionary<ICommand, Func<ICommand, JArray>>();
            public readonly Dictionary<ICommand, Func<ICommand, long>> NonQueryFunctions = new Dictionary<ICommand, Func<ICommand, long>>();
            public readonly Dictionary<ICommand, Func<ICommand, object>> ScalarFunctions = new Dictionary<ICommand, Func<ICommand, object>>();
        }

        private class Connection : IConnection
        {
            private ICommand _command;
            private IRepository _repository;
            private readonly CommandFunctions _functions;

            public Connection(IRepository repository, ICommand command, CommandFunctions functions)
            {
                _command = command;
                _repository = repository;
                _functions = functions;
            }

            public object RepositoryContext { get; set; }

            public void BeginTransaction()
            {
            }

            public void Commit()
            {
            }

            public void Rollback()
            {
            }

            public void SetCommand(ICommand command)
            {
                _command = command;
            }

            public IAsyncResult BeginExecuteReader(AsyncCallback callback = null)
            {
                return new SyncronousResult(null, callback);
            }

            public IDataReader EndExecuteReader(IAsyncResult asyncResult)
            {
                if (asyncResult.AsyncState != null)
                    ((AsyncCallback) asyncResult.AsyncState)(asyncResult);

                Func<ICommand, JArray> func;
                if (_functions.QueryFunctions.TryGetValue(_command, out func))
                    return new DataReader().Initialize(null, func(_command));

                return new DataReader().Initialize(null, new JArray());
            }

            public IAsyncResult BeginExecuteEnumerable(AsyncCallback callback = null)
            {
                return new SyncronousResult(null, callback);
            }

            public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T : class
            {
                if (asyncResult.AsyncState != null)
                    ((AsyncCallback) asyncResult.AsyncState)(asyncResult);


                Func<ICommand, JArray> func;
                if (_functions.QueryFunctions.TryGetValue(_command, out func))
                    return new DataEnumerator<T>().Initialize(null, func(_command));

                return new DataEnumerator<T>().Initialize(null, new JArray());
            }

            public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback = null)
            {
                return new SyncronousResult(null, callback);
            }

            public long EndExecuteNonQuery(IAsyncResult asyncResult)
            {
                if (asyncResult.AsyncState != null)
                    ((AsyncCallback)asyncResult.AsyncState)(asyncResult);

                Func<ICommand, long> func;
                if (_functions.NonQueryFunctions.TryGetValue(_command, out func))
                    return func(_command);

                return 0;
            }

            public IAsyncResult BeginExecuteScalar(AsyncCallback callback = null)
            {
                return new SyncronousResult(null, callback);
            }

            public T EndExecuteScalar<T>(IAsyncResult asyncResult)
            {
                Func<ICommand, object> func;
                if (_functions.ScalarFunctions.TryGetValue(_command, out func))
                    return (T)Convert.ChangeType((T)func(_command), typeof(T));

                return default(T);
            }

            public void Dispose()
            {
            }

        }
    }
}
