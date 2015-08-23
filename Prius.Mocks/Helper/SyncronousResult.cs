using System;
using System.Threading;

namespace Prius.Mocks.Helper
{
    internal class SyncronousResult : IAsyncResult
    {
        public WaitHandle AsyncWaitHandle { get; private set; }
        public object AsyncState { get; private set; }
        public bool CompletedSynchronously { get { return true; } }
        public bool IsCompleted { get { return true; } }

        public SyncronousResult(object asyncContext, AsyncCallback callback)
        {
            AsyncState = asyncContext;
            AsyncWaitHandle = new ManualResetEvent(true);
            if (callback != null) callback(this);
        }
    }
}
