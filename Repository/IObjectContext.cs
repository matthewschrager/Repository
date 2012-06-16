using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Repository
{
    public interface IObjectContext<T> : IDisposable where T : class
    {
        //===============================================================
        T Object { get; }
        //===============================================================
        void SaveChanges();
        //===============================================================
        void Update<TValue>(TValue value);
        //===============================================================
        void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter);
        //===============================================================
    }

    public interface IEnumerableObjectContext<T> : IDisposable where T : class
    {
        //===============================================================
        IQueryable<T> Objects { get; }
        //===============================================================
        void SaveChanges();
        //===============================================================
    }
}
