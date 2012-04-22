using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Repository.EntityFramework
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
        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, Func<TValue, Object> keySelector, Func<TContext> contextFactory = null)
            : this(setSelector, x => new[] { keySelector(x) }, contextFactory)
        {}
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
        public void Store(IEnumerable<TValue> values)
        {
            using (var c = ContextFactory())
            {
                var set = SetSelector(c);
                foreach (var x in values)
                    set.Add(x);

                c.SaveChanges();
            }
        }
        //===============================================================
        public void Remove(params Object[] keys)
        {
            using (var c = ContextFactory())
            {
                var set = SetSelector(c);
                var obj = set.Find(keys);
                set.Remove(obj);
                
                c.SaveChanges();
            }
        }
        //===============================================================
        public bool Exists(params Object[] keys)
        {
            using (var c = ContextFactory())
            {
                var set = SetSelector(c);
                return set.Find(keys) != null;
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

    public class EFObjectContext<T> : IObjectContext<T> where T : class
    {
        //===============================================================
        public EFObjectContext(T value, DbContext context)
        {
            Object = value;
            Context = context;
        }
        //===============================================================
        public T Object { get; private set; }
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
}
