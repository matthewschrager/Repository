using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private List<Tuple<List<Object>, T>> mData = new List<Tuple<List<object>, T>>();

        //===============================================================
        public InMemoryRepository(Func<T, IEnumerable<Object>> keySelector)
        {
            KeySelector = keySelector;
        }
        //===============================================================
        public InMemoryRepository(Func<T, Object> keySelector)
        {
            KeySelector = x => new[] { keySelector(x) };
        }
        //===============================================================
        private Func<T, IEnumerable<Object>> KeySelector { get; set; }
        //===============================================================
        public void Store(T value)
        {
            if (mData.Exists(x => x.Item1.SequenceEqual(KeySelector(value))))
                return;

            mData.Add(Tuple.Create(KeySelector(value).ToList(), value));
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
            mData.RemoveAll(x => x.Item1.SequenceEqual(keys));
        }
        //===============================================================
        public bool Exists(params Object[] keys)
        {
            return mData.Exists(x => x.Item1.SequenceEqual(keys));
        }
        //===============================================================
        public IObjectContext<T> Find(params Object[] keys)
        {
            var obj = mData.SingleOrDefault(x => x.Item1.SequenceEqual(keys));
            return new InMemoryObjectContext<T>(obj != null ? obj.Item2 : (T)null);
        }
        //===============================================================
        public IEnumerableObjectContext<T> GetItemsContext()
        {
            return new InMemoryEnumerableObjectContext<T>(mData.Select(x => x.Item2).AsQueryable());
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
