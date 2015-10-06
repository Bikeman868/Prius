using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Prius.Contracts.Interfaces;

namespace Prius.Orm.Enumeration
{
    public class AsyncEnumerable<T>: IAsyncEnumerable<T> where T: class
    {
        private IContext _context;
        private ICommand _command;
        private IAsyncResult _asyncResult; 
        private string _dataSetName;
        private IFactory<T> _dataContractFactory;

        public IAsyncEnumerable<T> Initialize(
            IContext context, 
            ICommand command, 
            IAsyncResult asyncResult, 
            string dataSetName, 
            IFactory<T> dataContractFactory)
        {
            _context = context;
            _command = command;
            _asyncResult = asyncResult;
            _dataSetName = dataSetName;
            _dataContractFactory = dataContractFactory;

            return this;
        }

        public void Dispose()
        {
            _command.Dispose();
            _context.Dispose();
        }

        public IDataEnumerator<T> GetResults()
        {
            return _context.EndExecuteEnumerable(_asyncResult, _dataSetName, _dataContractFactory);
        }

        public object AsyncState
        {
            get { return _asyncResult.AsyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _asyncResult.AsyncWaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return _asyncResult.CompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return _asyncResult.IsCompleted; }
        }
    }
}
