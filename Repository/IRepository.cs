using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public interface IRepository<T> : IDisposable, IQueryable<T> where T : class
    {
        //===============================================================
        void Store(T value);
        //===============================================================
        void Store(IEnumerable<T> values);
        //===============================================================
        void Remove(params Object[] keys);
        //===============================================================
        bool Exists(params Object[] keys);
        //===============================================================
        void Update<TValue>(TValue value, params Object[] keys);
        //===============================================================
        void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter, params Object[] keys);
        //===============================================================
        void Update(String json, UpdateType updateType, params Object[] keys);
        //===============================================================
        void Update(String pathToProperty, String json, UpdateType updateType, params Object[] keys);
        //===============================================================
        ObjectContext<T> Find(params Object[] keys);
        //===============================================================

    }

    public abstract class Repository<T> : IRepository<T> where T : class
    {
        //===============================================================
        public IEnumerator<T> GetEnumerator()
        {
            return GetItemsContext().GetEnumerator();
        }
        //===============================================================
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        //===============================================================
        public Expression Expression
        {
            get { return GetItemsContext().Expression; }
        }
        //===============================================================
        public Type ElementType
        {
            get { return GetItemsContext().ElementType; }
        }
        //===============================================================
        public IQueryProvider Provider
        {
            get { return GetItemsContext().Provider; }
        }
        //===============================================================
        public abstract void Store(T value);
        //===============================================================
        public abstract void Store(IEnumerable<T> values);
        //===============================================================
        public abstract void Remove(params object[] keys);
        //===============================================================
        public abstract bool Exists(params object[] keys);
        //===============================================================
        public abstract void Update<TValue>(TValue value, params object[] keys);
        //===============================================================
        public abstract void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter, params object[] keys);
        //===============================================================
        public abstract void Update(string json, UpdateType updateType, params object[] keys);
        //===============================================================
        public abstract void Update(string pathToProperty, string json, UpdateType updateType, params object[] keys);
        //===============================================================
        public abstract ObjectContext<T> Find(params object[] keys);
        //===============================================================
        protected abstract EnumerableObjectContext<T> GetItemsContext();
        //===============================================================
        public abstract void Dispose();
        //===============================================================
    }
}
