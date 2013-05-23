using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;

namespace Repository
{
    internal class InMemoryInsert<TValue> : Insert<TValue>
    {
        //===============================================================
        public InMemoryInsert(IEnumerable<String> keys, TValue value, IDictionary<String, TValue> dictionary)
            : base(keys, value)
        {
            Dictionary = dictionary;
        }
        //===============================================================
        private IDictionary<String, TValue> Dictionary { get; set; }
        //===============================================================
        public override void Apply()
        {
            Dictionary[Keys.First() as String] = Value;
        }
        //===============================================================
    }

    internal class InMemoryRemove<TKey, TValue> : Remove
    {
        //==============================================================='
        public InMemoryRemove(IEnumerable<String> keys, IDictionary<String, TValue> dictionary)
            : base(keys)
        {
            Dictionary = dictionary;
        }
        //===============================================================
        private IDictionary<String, TValue> Dictionary { get; set; }
        //===============================================================
        public override void Apply()
        {
            if (Dictionary.ContainsKey(Keys.First() as String))
                Dictionary.Remove(Keys.First() as String);
        }
        //===============================================================
    }

    internal class InMemoryModify<TValue> : Modify<TValue>
    {
        //===============================================================
        public InMemoryModify(TValue value, Action<TValue> modifier)
            : base(value, modifier)
        {}
        //===============================================================
        public override void Apply()
        {
            Modifier(Value);
        }
        //===============================================================
    }

    public class InMemoryRepository<T> : Repository<T> where T : class
    {
        private ConcurrentDictionary<String, T> mData = new ConcurrentDictionary<string, T>();
        private IList<IPendingChange> mPendingChanges = new List<IPendingChange>();

        //===============================================================
        public InMemoryRepository(Func<T, Object> keySelector)
            : base(x => new object[] { keySelector(x) })
        {}
        //===============================================================
        public override void SaveChanges()
        {
            foreach (var change in mPendingChanges)
                change.Apply();

            mPendingChanges.Clear();
        }
        //===============================================================
        public override void Insert(T value)
        {
            mPendingChanges.Add(new InMemoryInsert<T>(KeySelector(value).Select(x => x.ToString()), value, mData));
        }
        //===============================================================
        public override void RemoveByKey(Object[] keys)
        {
            if (keys.Length > 1)
                throw new NotSupportedException("InMemoryRepository only supports objects with a single key.");

            mPendingChanges.Add(new InMemoryRemove<String, T>(keys.Select(x => x.ToString()), mData));
        }
        //===============================================================
        public override bool ExistsByKey(params Object[] keys)
        {
            if (keys.Length > 1)
                throw new NotSupportedException("InMemoryRepository only supports objects with a single key.");

            return mData.ContainsKey(keys.First().ToString());
        }
        //===============================================================
        public override ObjectContext<T> Find(params Object[] keys)
        {
            if (keys.Length > 1)
                throw new NotSupportedException("InMemoryRepository only supports objects with a single key.");

            T obj = null;
            mData.TryGetValue(keys.First().ToString(), out obj);
            return new ObjectContext<T>(obj);
        }
        //===============================================================
        public override EnumerableObjectContext<T> Items
        {
            get { return new EnumerableObjectContext<T>(mData.Values.AsQueryable()); }
        }
        //===============================================================
        public override void Update<TValue>(TValue value, params Object[] keys)
        {
            if (keys.Length > 1)
                throw new NotSupportedException("InMemoryRepository only supports objects with a single key.");

            var existingValue = mData[keys.First().ToString()];
            mPendingChanges.Add(new InMemoryModify<T>(existingValue, x => AutoMapper.Mapper.DynamicMap(value, x)));
        }
        //===============================================================
        public override void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter, params Object[] keys)
        {
            if (keys.Length > 1)
                throw new NotSupportedException("InMemoryRepository only supports objects with a single key.");

            var existingValue = mData[keys.First().ToString()];
            mPendingChanges.Add(new InMemoryModify<T>(existingValue, x => AutoMapper.Mapper.DynamicMap(value, getter(x))));
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
            // In-memory repository doesn't need to dispose of anything.
        }
        //===============================================================
    }

    internal class InMemoryRepositoryTests
    {
        //===============================================================
        [Test]
        public void UpdateTest()
        {
            var repository = new InMemoryRepository<TestClass>(x => x.Key);
            repository.Insert(new TestClass());
            repository.SaveChanges();

            repository.Update(new { Value2 = DateTime.MaxValue }, 1);
            repository.SaveChanges();

            var val = repository.Find(1).Object;
            Assert.AreEqual(val.Value2, DateTime.MaxValue);

            var obj = new { Value2 = DateTime.MaxValue };
            repository.Update(obj, x => x.Property, 1);
            repository.SaveChanges();

            Assert.AreEqual(val.Property.Value2, DateTime.MaxValue);
            Assert.AreEqual(val.Property.Value1.Value, 1);

        }
        //===============================================================
    }
}
