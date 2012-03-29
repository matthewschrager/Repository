using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public class EFRepository<TContext, TValue> : IRepository<TValue> where TValue : class where TContext : DbContext
    {
        //===============================================================
        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, Func<TValue, Object[]> keySelector, Func<TContext> contextFactory = null)
        {
            if (contextFactory != null)
                ContextFactory = contextFactory;
            else
            {
                // Find a parameterless constructor. If none exists, throw an exception that lets the user know he must provide a factory that handles constructor parameters
                var parameterlessConstructor = typeof(TContext).GetConstructor(new Type[] { });
                if (parameterlessConstructor == null)
                    throw new ArgumentException("A default context factory can only be created if the context type (TContext) has a parameterless constructor.");

                ContextFactory = () => Activator.CreateInstance(typeof(TContext)) as TContext;
            }

            SetSelector = setSelector;
            KeySelector = keySelector;
        }
        //===============================================================
        private Func<TContext> ContextFactory { get; set; }
        //===============================================================
        private Func<TContext, DbSet<TValue>> SetSelector { get; set; }
        //===============================================================
        private Func<TValue, Object[]> KeySelector { get; set; }
        //===============================================================
        public void Store(TValue value)
        {
            using (var c = ContextFactory())
            {
                var set = SetSelector(c);
                var existingValue = set.Find(KeySelector(value));
                if (existingValue != null)
                    return;

                set.Add(value);
                c.SaveChanges();
            }
        }
        //===============================================================
        public void Remove(TValue value)
        {
            using (var c = ContextFactory())
            {
                var set = SetSelector(c);
                var obj = set.Find(KeySelector(value));
                set.Remove(obj);
                
                c.SaveChanges();
            }
        }
        //===============================================================
        public IObjectContext<TValue> Find(params Object[] keys)
        {
            var c = ContextFactory();
            var set = SetSelector(c);
            return new EFObjectContext<TValue>(set.Find(keys), c);
        }
        //===============================================================
        public IObjectContext<IQueryable<TValue>> GetItemsContext()
        {
            var context = ContextFactory();
            return new EFObjectContext<IQueryable<TValue>>(SetSelector(context), context);
        }
        //===============================================================
    }

    public class EFObjectContext<T> : IObjectContext<T>
    {
        //===============================================================
        public EFObjectContext(T value, DbContext context)
        {
            Value = value;
        }
        //===============================================================
        public T Value { get; private set; }
        //===============================================================
        private DbContext Context { get; set; }
        //===============================================================
        public void SaveChanges()
        {
            Context.SaveChanges();
        }
        //===============================================================
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Context.Dispose();
        }
        //===============================================================
    }

    public class EFQueryList<T> : QueryList<T> where T : class
    {
        //===============================================================
        public EFQueryList(IQueryable<T> query)
        {
            Query = query;
        }
        //===============================================================
        private IQueryable<T> Query { get; set; }
        //===============================================================
        public override QueryList<T> Include<TProperty>(Expression<Func<T, TProperty>> includeExpression)
        {
            return new EFQueryList<T>(Query.Include(includeExpression));
        }
        //===============================================================
        public override IEnumerator<T> GetEnumerator()
        {
            return Query.GetEnumerator();
        }
        //===============================================================
        public override Expression Expression
        {
            get { return Query.Expression; }
        }
        //===============================================================
        public override Type ElementType
        {
            get { return Query.ElementType; }
        }
        //===============================================================
        public override IQueryProvider Provider
        {
            get { return Query.Provider; }
        }
        //===============================================================
    }
}
