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
        public IEnumerableObjectContext<TValue> GetItemsContext()
        {
            var context = ContextFactory();
            return new EFEnumerableObjectContext<TValue>(SetSelector(context), context);
        }
        //===============================================================
        public void Update<T>(T value, params Object[] keys)
        {
            using (var obj = Find(keys))
            {
                obj.Update(value);
                obj.SaveChanges();
            }
        }
        //===============================================================
        public void Update<T, TProperty>(T value, Func<TValue, TProperty> getter, params Object[] keys)
        {
            using (var obj = Find(keys))
            {
                obj.Update(value, getter);
            }
        }
        //===============================================================
        public void Update(string pathToProperty, string json, UpdateType updateType, params object[] keys)
        {
            throw new NotImplementedException();
        }
        //===============================================================
        public void Update(string json, UpdateType updateType, params object[] keys)
        {
            throw new NotImplementedException();
        }
        //===============================================================
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // EF repository doesn't need to do anything, because it creates contexts for each request.
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
        public void Update<TValue>(TValue value)
        {
            Object = AutoMapper.Mapper.DynamicMap<T>(value);
        }
        //===============================================================
        public void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter)
        {
            AutoMapper.Mapper.DynamicMap(value, getter(Object));
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

    public class EFEnumerableObjectContext<T> : IEnumerableObjectContext<T> where T : class
    {
        //===============================================================
        public EFEnumerableObjectContext(IQueryable<T> objects, DbContext context)
        {
            Context = context;
            Objects = objects;
        }
        //===============================================================
        private DbContext Context { get; set; }
        //===============================================================
        public void Dispose()
        {
            Context.Dispose();
        }
        //===============================================================
        public IQueryable<T> Objects { get; private set; }
        //===============================================================
        public void SaveChanges()
        {
            Context.SaveChanges();
        }
        //===============================================================
    }
}
