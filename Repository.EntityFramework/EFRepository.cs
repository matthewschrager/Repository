using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Mappers;
using MoreLinq;
using NUnit.Framework;

namespace Repository.EntityFramework
{
    internal class EFInsert<TValue> : Insert<TValue> where TValue : class
    {
        //===============================================================
        public EFInsert(IEnumerable<Object> keys, TValue value, DbSet<TValue> set)
            : base(keys, value)
        {
            DbSet = set;
        }
        //===============================================================
        private DbSet<TValue> DbSet { get; set; }
        //===============================================================
        public override void Apply()
        {
            DbSet.Add(Value);
        }
        //===============================================================
    }

    internal class EFRemove<TValue> : Remove where TValue : class
    {
        //===============================================================
        public EFRemove(IEnumerable<Object> keys, DbSet<TValue> dbSet)
            : base(keys)
        {
            DbSet = dbSet;
        }
        //===============================================================
        private DbSet<TValue> DbSet { get; set; }
        //===============================================================
        public override void Apply()
        {
            var obj = DbSet.Find(Keys.ToArray());
            DbSet.Remove(obj);
        }
        //===============================================================
    }

    internal class EFRemoveAll<TValue, TContext> : IPendingChange where TContext : DbContext where TValue : class
    {
        //===============================================================
        public EFRemoveAll(TContext context)
        {
            Context = context;
        }
        //===============================================================
        private TContext Context { get; set; }
        //===============================================================
        public void Apply()
        {
            var tableName = Context.GetTableName<TValue>();
            tableName = tableName.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
            var query = "DELETE FROM " + tableName;

            Context.Database.ExecuteSqlCommand(query);
        }
        //===============================================================
    }


    public class EFRepository<TContext, TValue> : Repository<TValue> where TValue : class where TContext : DbContext
    {
        // We have to keep track of pending changes manually because RemoveAll is done via a straight SQL command, which will
        // execute immediately. Since the semantics of Repository are such that nothing is committed until SaveChanges is called, we have to
        // keep track of all changes manually and apply them on SaveChanges
        private List<IPendingChange> mPendingChanges = new List<IPendingChange>();

        //===============================================================
        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, TContext context = null)
            : base(GetKeySelector(context))
        {
            OwnsContext = false;

            if (context == null)
            {
                context = InstantiateContext();
                OwnsContext = true;
            }

            Context = context;
            SetSelector = setSelector;
            InsertBatchSize = 100;
        }
        //===============================================================
        private bool OwnsContext { get; set; }
        //===============================================================
        public TContext Context { get; set; }
        //===============================================================
        public uint InsertBatchSize { get; set; }
        //===============================================================
        private Func<TContext, DbSet<TValue>> SetSelector { get; set; }
        //===============================================================
        private static Func<TValue, Object[]> GetKeySelector(TContext givenContext)
        {
            var context = givenContext ?? InstantiateContext();
            return context.GetKeySelector<TValue>();
        }
        //===============================================================
        private static void CheckForParameterlessConstructor()
        {
            // Find a parameterless constructor. If none exists, throw an exception that lets the user know he must provide a context to this constructor
            var parameterlessConstructor = typeof(TContext).GetConstructor(new Type[] { });
            if (parameterlessConstructor == null)
                throw new ArgumentException("A default context can only be created if the context type (TContext) has a parameterless constructor.");
        }
        //===============================================================
        private static TContext InstantiateContext()
        {
            CheckForParameterlessConstructor();
            return Activator.CreateInstance(typeof(TContext)) as TContext;

        }
        //===============================================================
        public override void SaveChanges()
        {
            foreach (var change in mPendingChanges)
                change.Apply();

            Context.SaveChanges();
            mPendingChanges.Clear();
        }
        //===============================================================
        public override void Insert(TValue value)
        {
            mPendingChanges.Add(new EFInsert<TValue>(KeySelector(value), value, SetSelector(Context)));
        }
        //===============================================================
        public override void Insert(IEnumerable<TValue> values)
        {
            foreach (var x in values)
                Insert(x);
//            var oldValue = Context.Configuration.AutoDetectChangesEnabled;
//            Context.Configuration.AutoDetectChangesEnabled = false;
//
//            var set = SetSelector(Context);
//            foreach (var value in values)
//                set.Add(value);
//
//            Context.ChangeTracker.DetectChanges();
//            Context.Configuration.AutoDetectChangesEnabled = oldValue;
        }
        //===============================================================
        public override void RemoveByKey(params Object[] keys)
        {
            mPendingChanges.Add(new EFRemove<TValue>(keys, SetSelector(Context)));
        }
        //===============================================================
        public void RemoveAll()
        {
            // Since RemoveAll works differently than the other changes, we have to manually remove any inserts which
            // haven't yet been saved
            mPendingChanges.RemoveAll(x => x is EFInsert<TValue>);
            mPendingChanges.Add(new EFRemoveAll<TValue, TContext>(Context));
        }
        //===============================================================
        public override void RemoveAllByKey(IEnumerable<Object[]> keys)
        {
            foreach (var x in keys)
                mPendingChanges.Add(new EFRemove<TValue>(x, SetSelector(Context)));
        }
        //===============================================================
        public override bool ExistsByKey(params Object[] keys)
        {
            var set = SetSelector(Context);
            return set.Find(keys) != null;
        }
        //===============================================================
        public override ObjectContext<TValue> Find(params Object[] keys)
        {
            var set = SetSelector(Context);
            return new ObjectContext<TValue>(set.Find(keys));
        }
        //===============================================================
        public override EnumerableObjectContext<TValue> Items
        {
            get { return new EnumerableObjectContext<TValue>(SetSelector(Context)); }
        }
        //===============================================================
        public override void Update<T>(T value, params Object[] keys)
        {
            var obj = Find(keys);
            obj.Update(value);
        }
        //===============================================================
        public override void Update<T, TProperty>(T value, Func<TValue, TProperty> getter, params Object[] keys)
        {
            var obj = Find(keys);
            obj.Update(value, getter);
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

    public class EFRepository<TContext, TValue, TKey> : Repository<TValue, TKey> where TValue : class where TContext : DbContext
    {
        //===============================================================
        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, TContext context = null)
            : base(new EFRepository<TContext, TValue>(setSelector, context))
        {}
        //===============================================================
    }

    public class EFRepository<TContext, TValue, TKey1, TKey2> : Repository<TValue, TKey1, TKey2>
        where TValue : class
        where TContext : DbContext
    {
        //===============================================================
        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, TContext context = null)
            : base(new EFRepository<TContext, TValue>(setSelector, context))
        { }
        //===============================================================
    }
}
