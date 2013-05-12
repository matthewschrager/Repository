using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Repository
{
    public abstract class Repository<T> : IDisposable where T : class
    {
        //===============================================================
        protected Repository(Func<T, Object[]> keySelector)
        {
            KeySelector = keySelector;
        }
        //===============================================================
        protected Func<T, Object[]> KeySelector { get; private set; }
        //===============================================================
        public abstract void Insert(T value);
        //===============================================================
        public virtual void Insert(IEnumerable<T> values)
        {
            foreach (var value in values)
                Insert(value);
        }
        //===============================================================
        public void Remove(T obj)
        {
            RemoveByKey(KeySelector(obj));
        }
        //===============================================================
        public void RemoveAll(IEnumerable<T> objects = null)
        {
            objects = objects ?? Items;
            RemoveAllByKey(objects.Select(x => KeySelector(x)));
        }
        //===============================================================
        public abstract void RemoveByKey(params Object[] keys);
        //===============================================================
        public virtual void RemoveAllByKey(IEnumerable<Object[]> keys)
        {
            foreach (var keySet in keys)
                RemoveByKey(keySet);
        }
        //===============================================================
        public abstract void SaveChanges();
        //===============================================================
        public abstract bool ExistsByKey(params Object[] keys);
        //===============================================================
        public bool Exists(T obj)
        {
            return ExistsByKey(KeySelector(obj));
        }
        //===============================================================
        public abstract void Update<TValue>(TValue value, params Object[] keys);
        //===============================================================
        public abstract void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter, params Object[] keys);
        //===============================================================
        public abstract void Update(String json, UpdateType updateType, params Object[] keys);
        //===============================================================
        public abstract void Update(String pathToProperty, String json, UpdateType updateType, params Object[] keys);
        //===============================================================
        public abstract ObjectContext<T> Find(params Object[] keys);
        //===============================================================
        public abstract EnumerableObjectContext<T> Items { get; }
        //===============================================================
        public abstract void Dispose();
        //===============================================================
    }

    

    public abstract class Repository<TValue, TKey1, TKey2> : Repository<TValue> where TValue : class
    {
        private static readonly Func<TKey1, TKey2, Object[]> mKeyTransformer = (key1, key2) => new object[] { key1, key2 };

        //===============================================================
        protected Repository(Func<TValue, TKey1> key1Selector, Func<TValue, TKey2> key2Selector)
            : base(x => mKeyTransformer(key1Selector(x), key2Selector(x)))
        { }
        //===============================================================
        public ObjectContext<TValue> Find(TKey1 key1, TKey2 key2)
        {
            return Find(mKeyTransformer(key1, key2));
        }
        //===============================================================
        public void RemoveByKey(TKey1 key1, TKey2 key2)
        {
            RemoveByKey(mKeyTransformer(key1, key2));
        }
        //===============================================================
        public void RemoveAllByKey(IEnumerable<Tuple<TKey1, TKey2>> keys)
        {
            RemoveAllByKey(keys.Select(x => mKeyTransformer(x.Item1, x.Item2)));
        }
        //===============================================================
        public bool ExistsByKey(TKey1 key1, TKey2 key2)
        {
            return ExistsByKey(mKeyTransformer(key1, key2));
        }
        //===============================================================
    }
}
