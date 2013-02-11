using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MoreLinq;
using NUnit.Framework;

namespace Repository.EntityFramework
{
    public class EFRepository<TContext, TValue> : Repository<TValue> where TValue : class where TContext : DbContext
    {
        private bool mIsReadOnly;

        //===============================================================
        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, Func<TValue, Object[]> keySelector, TContext context = null)
            : base(keySelector)
        {
            OwnsContext = false;

            if (context == null)
            {
                // Find a parameterless constructor. If none exists, throw an exception that lets the user know he must provide a context to this constructor
                var parameterlessConstructor = typeof(TContext).GetConstructor(new Type[] { });
                if (parameterlessConstructor == null)
                    throw new ArgumentException("A default context can only be created if the context type (TContext) has a parameterless constructor.");

                context = Activator.CreateInstance(typeof(TContext)) as TContext;
                OwnsContext = true;
            }

            Context = context;
            SetSelector = setSelector;
            InsertBatchSize = 100;
        }
        //===============================================================
        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, Func<TValue, Object> keySelector, TContext context = null)
            : this(setSelector, x => new[] { keySelector(x) }, context)
        { }
        //===============================================================
//        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, Func<TValue, Object[]> keySelector, Func<TContext> contextFactory = null)
//        {
//            if (contextFactory != null)
//                ContextFactory = () =>
//                    {
//                        var c = contextFactory();
//                        c.Configuration.IsReadOnly = IsReadOnly;
//                        return c;
//                    };
//            else
//            {
//                // Find a parameterless constructor. If none exists, throw an exception that lets the user know he must provide a factory that handles constructor parameters
//                var parameterlessConstructor = typeof(TContext).GetConstructor(new Type[] { });
//                if (parameterlessConstructor == null)
//                    throw new ArgumentException("A default context factory can only be created if the context type (TContext) has a parameterless constructor.");
//
//                ContextFactory = () =>
//                    {
//                        var c = Activator.CreateInstance(typeof(TContext)) as TContext;
//                        c.Configuration.IsReadOnly = IsReadOnly;
//                        return c;
//                    };
//            }
//
//            SetSelector = setSelector;
//            KeySelector = keySelector;
//            InsertBatchSize = 100;
//            IsReadOnly = true;
//
//            // For now, we use a single context for the lifespan of this repository
//            Context = ContextFactory();
//        }
        //===============================================================
//        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, Func<TValue, Object> keySelector, Func<TContext> contextFactory = null)
//            : this(setSelector, x => new[] { keySelector(x) }, contextFactory)
//        {}
        //===============================================================
        private bool OwnsContext { get; set; }
        //===============================================================
        public TContext Context { get; set; }
        //===============================================================
        public uint InsertBatchSize { get; set; }
        //===============================================================
        private Func<TContext, DbSet<TValue>> SetSelector { get; set; }
        //===============================================================
        public override void Store(TValue value)
        {
            var set = SetSelector(Context);
            set.Add(value);

            Context.SaveChanges();
        }
        //===============================================================
        public override void Store(IEnumerable<TValue> values)
        {
            var oldValue = Context.Configuration.AutoDetectChangesEnabled;
            Context.Configuration.AutoDetectChangesEnabled = false;

            var set = SetSelector(Context);
            foreach (var value in values)
                set.Add(value);

            Context.ChangeTracker.DetectChanges();
            Context.SaveChanges();

            Context.Configuration.AutoDetectChangesEnabled = oldValue;
        }
        //===============================================================
        public override void RemoveByKey(params Object[] keys)
        {
            var set = SetSelector(Context);
            var obj = set.Find(keys);
            set.Remove(obj);

            Context.SaveChanges();
        }
        //===============================================================
        public void RemoveAll()
        {
            var tableName = Context.GetTableName<TValue>();
            tableName = tableName.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
            var query = "DELETE FROM " + tableName;

            var set = SetSelector(Context);
            Context.Database.ExecuteSqlCommand(query);
        }
        //===============================================================
        public override void RemoveAllByKey(IEnumerable<Object[]> keys)
        {
            var set = SetSelector(Context);
            foreach (var keySet in keys)
            {
                var obj = set.Find(keySet);
                set.Remove(obj);
            }

            Context.SaveChanges();
        }
        //===============================================================
        public override bool Exists(params Object[] keys)
        {
            var set = SetSelector(Context);
            return set.Find(keys) != null;
        }
        //===============================================================
        public override ObjectContext<TValue> Find(params Object[] keys)
        {
            var set = SetSelector(Context);
            return new EFObjectContext<TValue>(set.Find(keys), Context);
        }
        //===============================================================
        public override EnumerableObjectContext<TValue> Items
        {
            get { return new EFEnumerableObjectContext<TValue>(SetSelector(Context), Context); }
        }
        //===============================================================
        public override void Update<T>(T value, params Object[] keys)
        {
            using (var obj = Find(keys))
            {
                obj.Update(value);
                obj.SaveChanges();
            }
        }
        //===============================================================
        public override void Update<T, TProperty>(T value, Func<TValue, TProperty> getter, params Object[] keys)
        {
            using (var obj = Find(keys))
            {
                obj.Update(value, getter);
            }
        }
        //===============================================================
        public override void Update(string pathToProperty, string json, UpdateType updateType, params object[] keys)
        {
            throw new NotImplementedException();
        }
        //===============================================================
        public override void Update(string json, UpdateType updateType, params object[] keys)
        {
            throw new NotImplementedException();
        }
        //===============================================================
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            if (OwnsContext)
                Context.Dispose();
        }
        //===============================================================
    }

    public class EFObjectContext<T> : ObjectContext<T> where T : class
    {
        private T mObject;

        //===============================================================
        public EFObjectContext(T value, DbContext context)
        {
            mObject = value;
            Context = context;
        }
        //===============================================================
        public override T Object
        {
            get { return mObject; }
        }
        //===============================================================
        private DbContext Context { get; set; }
        //===============================================================
        public override void SaveChanges()
        {
            Context.SaveChanges();
        }
        //===============================================================
        public override void Update<TValue>(TValue value)
        {
            AutoMapper.Mapper.DynamicMap(value, Object);
        }
        //===============================================================
        public override void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter)
        {
            AutoMapper.Mapper.DynamicMap(value, getter(Object));
        }
        //===============================================================
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            // We no longer dispose of the context in each ObjectContext, because it is handled at the Repository level now
            // Context.Dispose();
        }
        //===============================================================
    }

    public class EFEnumerableObjectContext<T> : EnumerableObjectContext<T> where T : class
    {
        private DbSet<T> mObjects;

        //===============================================================
        public EFEnumerableObjectContext(DbSet<T> objects, DbContext context)
        {
            Context = context;
            mObjects = objects;
        }
        //===============================================================
        private DbContext Context { get; set; }
        //===============================================================
        public override void Dispose()
        {
            // We no longer dispose the context in each ObjectContext, because it is handled at the Repository level now
            //Context.Dispose();
        }
        //===============================================================
        protected override IQueryable<T> Objects
        {
            get { return mObjects; }
        }
        //===============================================================
        public override void SaveChanges()
        {
            Context.SaveChanges();
        }
        //===============================================================
    }
}
