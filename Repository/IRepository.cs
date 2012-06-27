using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public interface IRepository<T> : IDisposable where T : class
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
        IObjectContext<T> Find(params Object[] keys);
        //===============================================================
        IEnumerableObjectContext<T> GetItemsContext();
        //===============================================================

    }
}
