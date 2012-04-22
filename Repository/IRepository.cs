using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public interface IRepository<T> where T : class
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
        IObjectContext<T> Find(params Object[] keys);
        //===============================================================
        IObjectContext<IQueryable<T>> GetItemsContext();
        //===============================================================

    }
}
