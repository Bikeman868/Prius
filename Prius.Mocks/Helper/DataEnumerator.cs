using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;

namespace Prius.Mocks.Helper
{
    internal class DataEnumerator<T> : IDataEnumerator<T> where T : class
    {
        private IMockedResultSet _resultSet;

        public IDataEnumerator<T> Initialize(string dataShapeName, IEnumerable<IMockedResultSet> resultSets)
        {
            _resultSet = resultSets.FirstOrDefault();
            return this;
        }

        public bool IsServerOffline { get { return false; } }

        public Exception ServerOfflineException { get { return null; } }

        public bool IsReusable { get { return false; } }

        public bool IsDisposing { get { return false; } }

        private bool IsDisposed { get; set; }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_resultSet == null)
                return new List<T>().GetEnumerator();

            return _resultSet.Data.Select(jobject => jobject.ToObject<T>()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
