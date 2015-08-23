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
        private JArray _data;

        public IDataEnumerator<T> Initialize(string dataShapeName, JArray data)
        {
            _data = data;
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
            if (_data == null)
                return new List<T>().GetEnumerator();

            return _data.Select(e => e.ToObject<T>()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
