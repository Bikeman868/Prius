using System.Collections.Generic;
using Prius.Contracts.Interfaces;
using Prius.Orm.Utility;

namespace Prius.Orm.Orm
{
    public class EnumerableData<T> : Disposable, IDisposableEnumerable<T> where T : class
    {
        private IContext _context;
        private IDataEnumerator<T> _data;

        public IDisposableEnumerable<T> Initialize(IContext context, IDataEnumerator<T> data)
        {
            _context = context;
            _data = data;
            return this;
        }

        protected override void Dispose(bool destructor)
        {
            _context.Dispose();
            _data.Dispose();

            base.Dispose(destructor);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}
