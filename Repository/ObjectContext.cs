using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public abstract class ObjectContext<T> : IDisposable where T : class
    {
        //===============================================================
        public abstract T Object { get; }
        //===============================================================
        public abstract void SaveChanges();
        //===============================================================
        public abstract void Update<TValue>(TValue value);
        //===============================================================
        public abstract void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter);
        //===============================================================
        public abstract void Dispose();
        //===============================================================
    }

    public abstract class EnumerableObjectContext<T> : IDisposable, IQueryable<T> where T : class
    {
        private IQueryProvider mCachedQueryProvider = null;

        //===============================================================
        protected abstract IQueryable<T> Objects { get; }
        //===============================================================
        public abstract void SaveChanges();
        //===============================================================
        public abstract void Dispose();
        //===============================================================
        public IEnumerator<T> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }
        //===============================================================
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        //===============================================================
        public TQueryable AsRaw<TQueryable>() where TQueryable : class
        {
            return Objects as TQueryable;
        }
        //===============================================================
        public Expression Expression 
        {
            get { return Objects.Expression; }
        }
        //===============================================================
        public Type ElementType
        {
            get { return Objects.ElementType; }
        }
        //===============================================================
        public IQueryProvider Provider
        {
            get
            {
                if (mCachedQueryProvider == null)
                    mCachedQueryProvider = Objects.Provider;

                return mCachedQueryProvider;
            }
        }
        //===============================================================
    }
}
