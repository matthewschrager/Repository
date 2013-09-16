using System;
using System.Collections.Generic;
using System.Linq;
using Repository.ChangeTracking;

namespace Repository
{
    public abstract class Repository<T> : IDisposable
    {
        //===============================================================
        protected Repository(Func<T, Object[]> keySelector, bool ignoreChangeTracking = false)
        {
            KeySelector = keySelector;
            UnsavedObjects = new List<ChangeTracker<T>>();
            PendingOperations = new List<IOperation>();

            IgnoreChangeTracking = ignoreChangeTracking;
        }
        //===============================================================
        internal bool IgnoreChangeTracking { get; set; }
        //===============================================================
        protected Func<T, Object[]> KeySelector { get; private set; }
        //===============================================================
        private List<ChangeTracker<T>> UnsavedObjects { get; set; }
        //===============================================================
        private List<IOperation> PendingOperations { get; set; }
        //===============================================================
        protected abstract Insert<T> CreateInsert(IEnumerable<object> keys, T value);
        //================================================================================
        protected virtual BatchInsert<T> CreateBatchInsert(IEnumerable<KeyValuePair<IEnumerable<object>, T>> keyValuePairs)
        {
            return new DefaultBatchInsert<T>(keyValuePairs, CreateInsert);
        }
        //===============================================================
        protected abstract Remove CreateRemove(IEnumerable<object> keys);
        //===============================================================
        protected abstract Modify<T> CreateModify(IEnumerable<object> keys, T value, Action<T> modifier);
        //===============================================================
        public abstract bool ExistsByKey(params Object[] keys);
        //===============================================================
        internal void AddChangeTracker(T obj)
        {
            if (!IgnoreChangeTracking && !ChangeTracker<T>.CanTrackChanges)
                throw new RepositoryException(String.Format("Cannot enable change tracking for type {0}. Please ensure that it has a public default (parameterless) constructor.", typeof(T)));

            UnsavedObjects.Add(new ChangeTracker<T>(obj, KeySelector));
        }
        //===============================================================
        private void AddPendingOperation(IOperation operation)
        {
            PendingOperations.Add(operation);
        }
        //===============================================================
        public void Insert(T value)
        {
            AddPendingOperation(CreateInsert(KeySelector(value), value));
        }
        //===============================================================
        public void Insert(IEnumerable<T> values)
        {
            AddPendingOperation(CreateBatchInsert(values.Select(x => new KeyValuePair<IEnumerable<object>, T>(KeySelector(x), x))));
        }
        //===============================================================
        public void Remove(T obj)
        {
            RemoveByKey(KeySelector(obj));
        }
        //===============================================================
        public void RemoveAll(IEnumerable<T> objects = null)
        {
            if (objects == null)
                PendingOperations.RemoveAll(x => x is Insert<T> || x is BatchInsert<T>);

            objects = objects ?? Items;
            RemoveAllByKey(objects.Select(x => KeySelector(x)));
        }
        //===============================================================
        public void RemoveByKey(params Object[] keys)
        {
            AddPendingOperation(CreateRemove(keys));
        }
        //===============================================================
        public void RemoveAllByKey(IEnumerable<Object[]> keys)
        {
            foreach (var keySet in keys)
                RemoveByKey(keySet);
        }
        //===============================================================
        public bool Exists(T obj)
        {
            return ExistsByKey(KeySelector(obj));
        }
        //===============================================================
        public void Update<TValue>(TValue modifier, params Object[] keys)
        {
            var existingItem = FindWrapper(keys, false);
            if (existingItem == null)
                return;

            AddPendingOperation(CreateModify(keys, existingItem.Object, x => AutoMapper.Mapper.DynamicMap(modifier, x)));
        }
        //===============================================================
        public void Update<TValue, TProperty>(TValue modifier, Func<T, TProperty> getter, params Object[] keys)
        {
            var existingItem = FindWrapper(keys, false);
            if (existingItem == null)
                return;

            AddPendingOperation(CreateModify(keys, existingItem.Object, x => AutoMapper.Mapper.DynamicMap(modifier, getter(x))));
        }
        //===============================================================
        public ObjectContext<T> Find(params Object[] keys)
        {
            return FindWrapper(keys, true);
        }
        //===============================================================
        private ObjectContext<T> FindWrapper(object[] keys, bool trackChanges)
        {
            ObjectContext<T> obj = null;
            try
            {
                obj = FindImpl(keys);
            }

            catch (Exception e)
            {
                throw new RepositoryException("Could not access repository.", e);
            }

            if (trackChanges && obj != null)
                AddChangeTracker(obj.Object);

            return obj;
        }
        //===============================================================
        protected abstract ObjectContext<T> FindImpl(object[] keys);
        //===============================================================
        public void SaveChanges()
        {
            try
            {
                ApplyChanges();
            }

            catch (Exception e)
            {
                throw new RepositoryException("Could not commit changes to repository.", e);
            }
          
            AfterApplyChanges();
        }
        //===============================================================
        private void ApplyChanges()
        {
            // For any modified objects, add an update to the list of pending ops
            foreach (var obj in UnsavedObjects.Where(x => x.HasChanges))
                Update(obj.CurrentValue, KeySelector(obj.CurrentValue));

            // Run all the pending ops in order
            foreach (var change in PendingOperations)
                change.Apply();

            // Remove all pending changes and unsaved objects, since they're all processed
            UnsavedObjects.Clear();
            PendingOperations.Clear();
        }
        //===============================================================
        protected virtual void AfterApplyChanges()
        {
            // Do nothing by default
        }
        //===============================================================
        public abstract EnumerableObjectContext<T> Items { get; }
        //===============================================================
        public abstract void Dispose();
        //===============================================================
        // This is only for use with ExplicitKeyRepository, which changes the key selector with each operation
        internal void SetKeySelector(Func<T, object[]> keySelector)
        {
            KeySelector = keySelector;
        }
        //===============================================================
        public ReadOnlyRepository<T> AsReadOnly()
        {
            return new ReadOnlyRepository<T>(this);
        }
        //================================================================================
    }
}
