using System;

namespace Prius.Contracts.Utility
{
    /// <summary>
    /// Base class for objects that implement IDisposable
    /// </summary>
    public class Disposable : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public bool IsDisposing { get; private set; }

        private object _locker = new object();

#if CHECK_DISPOSABLE

        private string _constructorStackTrace;

        public Disposable()
        {
            _constructorStackTrace = new System.Diagnostics.StackTrace().ToString();
        }

        ~Disposable()
        {
            var msg = GetType().FullName + " objects are disposable, you should not allow the garbage collector to dispose of them, ";
            msg += "you should explicitly dispose of them in your application. The stack trace of where this object was created is:\r\n";
            msg += _constructorStackTrace;
            throw new Exception(msg);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DoDispose(false);
        }

#else

        public void Dispose()
        {
            lock(_locker)
                DoDispose(false);
        }

#endif

        protected virtual void Dispose(bool destructor)
        {
            // If the dispose method is called from the destructor, then
            // do not dispose of objects that this instance owns.
            //
            // If the dispose method is called explicitly by the application
            // then cascade the dispose to all owned objects.
        }

        private void DoDispose(bool destructor)
        {
            if (IsDisposing || IsDisposed)
            {
#if CHECK_DISPOSABLE
                System.Diagnostics.Debugger.Break();
#endif
            }
            else
            {
                IsDisposing = true;
                try
                {
                    Dispose(destructor);
                }
                finally
                {
                    IsDisposed = true;
                }
            }
        }

    }
}
