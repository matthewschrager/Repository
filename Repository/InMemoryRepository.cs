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

    internal class InMemoryRemove<TValue> : Remove
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
        public InMemoryModify(IEnumerable<object> keys, TValue value, Action<TValue> modifier)
            : base(keys, value, modifier)
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

        //===============================================================
        public InMemoryRepository(Func<T, Object> keySelector)
            : base(x => new[] { keySelector(x) })
        {}
        //===============================================================
        protected override Insert<T> CreateInsert(IEnumerable<object> keys, T value)
        {
            return new InMemoryInsert<T>(keys.Select(x => x.ToString()), value, mData);
        }
        //===============================================================
        protected override Remove CreateRemove(IEnumerable<object> keys)
        {
            return new InMemoryRemove<T>(keys.Select(x => x.ToString()), mData);
        }
        //===============================================================
        protected override Modify<T> CreateModify(IEnumerable<object> keys, T value, Action<T> modifier)
        {
            return new InMemoryModify<T>(keys, value, modifier);
        }
        //===============================================================
        public override bool ExistsByKey(params Object[] keys)
        {
            if (keys.Length > 1)
                throw new NotSupportedException("InMemoryRepository only supports objects with a single key.");

            return mData.ContainsKey(keys.First().ToString());
        }
        //===============================================================
        protected override ObjectContext<T> FindImpl(object[] keys)
        {
            var key = keys.First().ToString();
            var obj = default(T);
            if (!mData.TryGetValue(key, out obj))
                return null;

            return new ObjectContext<T>(obj);
        }
        //===============================================================
        public override EnumerableObjectContext<T> Items
        {
            get { return new EnumerableObjectContext<T>(mData.Values.AsQueryable(), this); }
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
}
