using System;
using System.Collections;
using System.Collections.Generic;
using Prius.Contracts.Interfaces;
using Prius.Orm.Utility;

namespace Prius.Orm.Data
{
    public class DataEnumerator<T> : Disposable, IDataEnumerator<T> where T: class
    {
        private readonly IMapper _mapper;

        private IDataReader _reader;
        private Action _closeAction;
        private string _dataSetName;
        private IFactory<T> _dataContractFactory;

        public DataEnumerator(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IDataEnumerator<T> Initialize(IDataReader reader, Action closeAction, string dataSetName, IFactory<T> dataContractFactory)
        {
            _reader = reader;
            _closeAction = closeAction;
            _dataSetName = dataSetName;
            _dataContractFactory = dataContractFactory;
            return this;
        }

        protected override void Dispose(bool destructor)
        {
            if (_closeAction != null) _closeAction();
            base.Dispose(destructor);
        }

        public bool IsServerOffline { get { return _reader.IsServerOffline;  } }

        public Exception ServerOfflineException { get { return _reader.ServerOfflineException; } }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_mapper.GetMappedDataReader<T>(_reader, _dataSetName, _dataContractFactory));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator : IEnumerator<T>
        {
            private IMappedDataReader<T> _mappedDataReader;
            private T _current;

            public Enumerator(IMappedDataReader<T> mappedDataReader)
            {
                _mappedDataReader = mappedDataReader;
            }

            public T Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_mappedDataReader.Read())
                {
                    _current = _mappedDataReader.Map();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                throw new InvalidOperationException("You can not reset a data reader, you have to run the query again instead");
            }
        }
    }
}
