using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public interface IRepository<T>
    {
        //===============================================================
        void Store(T value);
        //===============================================================
        void Remove(T value);
        //===============================================================
        IObjectContext<T> Find(params Object[] keys);
        //===============================================================
        IObjectContext<IQueryable<T>> GetItemsContext();
        //===============================================================

    }

    public abstract class QueryList<T> : IQueryable<T>
    {
        //===============================================================
        public abstract QueryList<T> Include<TProperty>(Expression<Func<T, TProperty>> includeExpression);
        //===============================================================
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public abstract IEnumerator<T> GetEnumerator();
        //===============================================================
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        //===============================================================
        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </returns>
        public abstract Expression Expression { get; }
        //===============================================================
        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
        /// </returns>
        public abstract Type ElementType { get; }
        //===============================================================
        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
        /// </returns>
        public abstract IQueryProvider Provider { get; }
        //===============================================================
    }
}
