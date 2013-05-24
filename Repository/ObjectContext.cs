using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public class ObjectContext<T>
    {
        //===============================================================
        public ObjectContext(T obj)
        {
            Object = obj;
        }
        //===============================================================
        public T Object { get; private set; }
        //===============================================================
        public void Update<TValue>(TValue value)
        {
            AutoMapper.Mapper.DynamicMap(value, Object);
        }
        //===============================================================
        public void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter)
        {
            AutoMapper.Mapper.DynamicMap(value, getter(Object));
        }
        //===============================================================
    }

    public class EnumerableObjectContext<T> : IQueryable<T>
    {
        private IQueryProvider mCachedQueryProvider = null;

        //===============================================================
        public EnumerableObjectContext(IQueryable<T> objects)
        {
            Objects = objects;
        }
        //===============================================================
        protected IQueryable<T> Objects { get; private set; }
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
