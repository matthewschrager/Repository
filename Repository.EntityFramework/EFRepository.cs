using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Repository.EntityFramework
{
    public class EFRepository<TContext, TValue> : Repository<TValue> where TValue : class where TContext : DbContext
    {
        //===============================================================
        public EFRepository(Func<TContext, DbSet<TValue>> setSelector, TContext context = null)
            : base(GetKeySelector(context), true)
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
        protected override Insert<TValue> CreateInsert(IEnumerable<object> keys, TValue value)
        {
            return new EFInsert<TValue>(keys, value, SetSelector(Context));
        }
        //===============================================================
        protected override Remove CreateRemove(IEnumerable<object> keys)
        {
            return new EFRemove<TValue>(keys, SetSelector(Context));
        }
        //===============================================================
        protected override Modify<TValue> CreateModify(IEnumerable<object> keys, TValue value, Action<TValue> modifier)
        {
            return new EFModify<TValue>(keys, value, modifier);
        }
        //===============================================================
//        public void RemoveAll()
//        {
//            // Since RemoveAll works differently than the other changes, we have to manually remove any inserts which
//            // haven't yet been saved
//            mPendingChanges.RemoveAll(x => x is EFInsert<TValue>);
//            mPendingChanges.Add(new EFRemoveAll<TValue, TContext>(Context));
//        }
        //===============================================================
        public override bool ExistsByKey(params Object[] keys)
        {
            var set = SetSelector(Context);
            return set.Find(keys) != null;
        }
        //===============================================================
        protected override ObjectContext<TValue> FindImpl(object[] keys)
        {
            var obj = SetSelector(Context).Find(keys);
            if (obj == null)
                return null;

            return new ObjectContext<TValue>(obj);
        }
        //===============================================================
        protected override void AfterApplyChanges()
        {
            try
            {
                base.AfterApplyChanges();
                Context.SaveChanges();
            }

            catch (Exception e)
            {
                throw e;
            }
        }
        //===============================================================
        public override EnumerableObjectContext<TValue> Items
        {
            get { return new EnumerableObjectContext<TValue>(SetSelector(Context), this); }
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
