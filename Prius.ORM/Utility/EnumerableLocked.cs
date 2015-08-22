using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prius.Contracts.Interfaces;

namespace Prius.Orm.Utility
{
    public class EnumerableLocked<T> : Disposable, IDisposableEnumerable<T>
    {
        private IEnumerable<T> _enumerable;
        private Action _lockAction;
        private Action _unlockAction;
        private bool _locked;

        public IDisposableEnumerable<T> Initialize(IEnumerable<T> enumerable, Action lockAction, Action unlockAction)
        {
            _enumerable = enumerable;
            _lockAction = lockAction;
            _unlockAction = unlockAction;

            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!_locked && _lockAction != null)
            {
                _lockAction();
                _lockAction = null;
                _locked = true;
            }
            return _enumerable.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override void Dispose(bool destructor)
        {
            if (_locked && _unlockAction != null)
                _unlockAction();
            _unlockAction = null;

            base.Dispose(destructor);
        }
    }
}
