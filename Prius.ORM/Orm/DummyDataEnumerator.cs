using System;
using System.Collections;
using System.Collections.Generic;
using Prius.Contracts.Interfaces;
using Prius.Orm.Utility;

namespace Prius.Orm.Orm
{
    public class DummyDataEnumerator<T> : Disposable, IDataEnumerator<T> where T : class
    {
        public DummyDataEnumerator()
        {
        }

        public IDataEnumerator<T> Initialize()
        {
            return this;
        }

        public bool IsServerOffline { get { return true; } }

        public Exception ServerOfflineException { get { return new Exception("DummyDataEnumerator is used."); } }

        public IEnumerator<T> GetEnumerator()
        {
            return new List<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
