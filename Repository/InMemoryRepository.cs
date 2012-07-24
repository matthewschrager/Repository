using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private ConcurrentDictionary<String, T> mData = new ConcurrentDictionary<string, T>();

        //===============================================================
        public InMemoryRepository(Func<T, Object> keySelector)
        {
            KeySelector = keySelector;
        }
        //===============================================================
        private Func<T, Object> KeySelector { get; set; }
        //===============================================================
        public void Store(T value)
        {
            mData[KeySelector(value).ToString()] = value;
        }
        //===============================================================
        public void Store(IEnumerable<T> values)
        {
            foreach (var x in values)
                Store(x);
        }
        //===============================================================
        public void Remove(params Object[] keys)
        {
            if (keys.Length > 1)
                throw new NotSupportedException("InMemoryRepository only supports objects with a single key.");
            
            T removedObj = null;
            mData.TryRemove(keys.First().ToString(), out removedObj);
        }
        //===============================================================
        public bool Exists(params Object[] keys)
        {
            if (keys.Length > 1)
                throw new NotSupportedException("InMemoryRepository only supports objects with a single key.");

            return mData.ContainsKey(keys.First().ToString());
        }
        //===============================================================
        public IObjectContext<T> Find(params Object[] keys)
        {
            if (keys.Length > 1)
                throw new NotSupportedException("InMemoryRepository only supports objects with a single key.");

            T obj = null;
            mData.TryGetValue(keys.First().ToString(), out obj);
            return new InMemoryObjectContext<T>(obj);
        }
        //===============================================================
        public IEnumerableObjectContext<T> GetItemsContext()
        {
            return new InMemoryEnumerableObjectContext<T>(mData.Values.AsQueryable());
        }
        //===============================================================
        public void Update<TValue>(TValue value, params Object[] keys)
        {
            using (var obj = Find(keys))
            {
                obj.Update(value);
                obj.SaveChanges();
            }
        }
        //===============================================================
        public void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter, params Object[] keys)
        {
            using (var obj = Find(keys))
            {
                obj.Update(value, getter);
                obj.SaveChanges();
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
            // In-memory repository doesn't need to dispose of anything.
        }
        //===============================================================
    }

    public class InMemoryObjectContext<T> : IObjectContext<T> where T : class
    {
        //===============================================================
        public InMemoryObjectContext(T value)
        {
            Object = value;
        }
        //===============================================================
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // Do nothing, since in memory objects are automatically disposed
        }
        //===============================================================
        public T Object { get; private set; }
        //===============================================================
        public void SaveChanges()
        {
            // Do nothing - stuff is automatically saved.
        }
        //===============================================================
        public void Update<TValue>(TValue value)
        {
            AutoMapper.Mapper.DynamicMap(value, Object);
        }
        //===============================================================
        public void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter)
        {
            AutoMapper.Mapper.DynamicMap(value, getter(Object));
        }
        //===============================================================
    }

    public class InMemoryEnumerableObjectContext<T> : IEnumerableObjectContext<T> where T : class
    {
        //===============================================================
        public InMemoryEnumerableObjectContext(IQueryable<T> objects)
        {
            Objects = objects;
        }
        //===============================================================
        public void Dispose()
        {
            // Do nothing, since in-memory objects are automatically disposed
        }
        //===============================================================
        public IQueryable<T> Objects { get; private set; }
        //===============================================================
        public void SaveChanges()
        {
            // Do nothing, since in-memory takes care of it
        }
        //===============================================================
    }
}
