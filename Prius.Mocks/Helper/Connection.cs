using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.Mocks.Helper
{
    internal class Connection : IConnection
    {
        private readonly IMockedRepository _mockedRepository;
        private ICommand _command;

        public object RepositoryContext { get; set; }

        public Connection(IMockedRepository mockedRepository, ICommand command = null)
        {
            _mockedRepository = mockedRepository;
            _command = command;
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

        public void SetCommand(ICommand command)
        {
            _command = command;
        }

        public IAsyncResult BeginExecuteReader(AsyncCallback callback)
        {
            if (_command == null)
                throw new Exception("You must provide a command before executing reader");
            return new SyncronousResult(_command, callback);
        }

        public IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            var command = asyncResult.AsyncState as ICommand;

            var procedure = _mockedRepository.GetProcedure(command.CommandText);
            var results = procedure.Query(command);

            return new DataReader().Initialize(command.CommandText, results);
        }

        public IAsyncResult BeginExecuteEnumerable(AsyncCallback callback)
        {
            if (_command == null)
                throw new Exception("You must provide a command before executing enumerable");
            return new SyncronousResult(_command, callback);
        }

        public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T : class
        {
            var command = asyncResult.AsyncState as ICommand;

            var procedure = _mockedRepository.GetProcedure(command.CommandText);
            var results = procedure.Query(command);

            return new DataEnumerator<T>().Initialize(command.CommandText, results);
        }

        public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback)
        {
            return new SyncronousResult(_command, callback);
        }

        public long EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            var command = asyncResult.AsyncState as ICommand;

            var procedure = _mockedRepository.GetProcedure(command.CommandText);
            return procedure.NonQuery(command);
        }

        public IAsyncResult BeginExecuteScalar(AsyncCallback callback)
        {
            return new SyncronousResult(_command, callback);
        }

        public T EndExecuteScalar<T>(IAsyncResult asyncResult)
        {
            var command = asyncResult.AsyncState as ICommand;

            var procedure = _mockedRepository.GetProcedure(command.CommandText);
            return procedure.Scalar<T>(command);
        }

        public IDataReader ExecuteReader()
        {
            return EndExecuteReader(BeginExecuteReader(null));
        }

        public long ExecuteNonQuery()
        {
            return EndExecuteNonQuery(BeginExecuteNonQuery(null));
        }

        public T ExecuteScalar<T>()
        {
            return EndExecuteScalar<T>(BeginExecuteScalar(null));
        }

        public IDataEnumerator<T> ExecuteEnumerable<T>() where T : class
        {
            return EndExecuteEnumerable<T>(BeginExecuteEnumerable(null));
        }

        public bool IsReusable { get { return false; } }

        public bool IsDisposing { get { return false; } }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
