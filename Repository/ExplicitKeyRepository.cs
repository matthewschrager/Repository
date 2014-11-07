using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ExplicitKeyRepository<T> : IDisposable
    {
        protected static readonly Func<T, object[]> NullKeySelector = x => new object[] { }; 

        //===============================================================
        public ExplicitKeyRepository(Repository<T> innerRepository)
        {
            InnerRepository = innerRepository;

            // We allow change tracking to be ignored ONLY for string repositories, because String has no
            // default constructor (and so cannot be change-tracked) but we still want to allow strings to be stored
            // with explicit keys
            InnerRepository.IgnoreChangeTracking = typeof(T) == typeof(String);
        }
        //===============================================================
        public Repository<T> InnerRepository { get; private set; }
        //===============================================================
        public void Insert(T value, params object[] keys)
        {
            InnerRepository.SetKeySelector(x => keys);
            InnerRepository.Insert(value);
        }
        //===============================================================
        public void Insert(IEnumerable<Tuple<object[], T>> values)
        {
            foreach (var value in values)
                Insert(value.Item2, value.Item1);
        }
        //===============================================================
        public void RemoveByKey(params object[] keys)
        {
            InnerRepository.RemoveByKey(keys);
        }
        //===============================================================
        public void RemoveAllByKey(IEnumerable<object[]> keys)
        {
            foreach (var key in keys)
                RemoveByKey(key);
        }
        //===============================================================
        public void SaveChanges()
        {
            InnerRepository.SaveChanges();
        }
        //===============================================================
        public bool ExistsByKey(params object[] keys)
        {
            return InnerRepository.ExistsByKey(keys);
        }
        //===============================================================
        public ObjectContext<T> Find(params object[] keys)
        {
            return InnerRepository.Find(keys);
        }
        //===============================================================
        public EnumerableObjectContext<T> Items
        {
            get { return InnerRepository.Items; }
        }
        //===============================================================
        public void Dispose()
        {
            InnerRepository.Dispose();
        }
        //===============================================================
    }

    public class ExplicitKeyRepository<TValue, TKey> : IDisposable
    {
        protected static readonly Func<TValue, object[]> NullKeySelector = x => new object[] { }; 

        //===============================================================
        protected ExplicitKeyRepository(Repository<TValue> innerRepository)
        {
            InnerRepository = new ExplicitKeyRepository<TValue>(innerRepository);
        }
        //===============================================================
        public ExplicitKeyRepository<TValue> InnerRepository { get; private set; }
        //===============================================================
        public void Insert(TValue value, TKey key)
        {
            InnerRepository.Insert(value, key);
        }
        //===============================================================
        public void Insert(IEnumerable<Tuple<TKey, TValue>> values)
        {
            InnerRepository.Insert(values.Select(x => Tuple.Create(new object[] { x.Item1 }, x.Item2)));
        }
        //===============================================================
        public void RemoveByKey(TKey key)
        {
            InnerRepository.RemoveByKey(key);
        }
        //===============================================================
        public void RemoveAllByKey(IEnumerable<TKey> keys)
        {
            InnerRepository.RemoveAllByKey(keys.Select(x => new object[] { x }));
        }
        //===============================================================
        public void SaveChanges()
        {
            InnerRepository.SaveChanges();
        }
        //===============================================================
        public bool ExistsByKey(TKey key)
        {
            return InnerRepository.ExistsByKey(key);
        }
        //===============================================================
        public ObjectContext<TValue> Find(TKey key)
        {
            return InnerRepository.Find(key);
        }
        //===============================================================
        public EnumerableObjectContext<TValue> Items
        {
            get { return InnerRepository.Items; }
        }
        //===============================================================
        public void Dispose()
        {
            InnerRepository.Dispose();
        }
        //===============================================================

    }

    public class ExplicitKeyRepository<TValue, TKey1, TKey2> : IDisposable
    {
        protected static readonly Func<TValue, object[]> NullKeySelector = x => new object[] { }; 

        //===============================================================
        protected ExplicitKeyRepository(Repository<TValue> innerRepository)
        {
            InnerRepository = new ExplicitKeyRepository<TValue>(innerRepository);
        }
        //===============================================================
        public ExplicitKeyRepository<TValue> InnerRepository { get; private set; }
        //===============================================================
        public void Insert(TValue value, TKey1 key1, TKey2 key2)
        {
            InnerRepository.Insert(value, key1, key2);
        }
        //===============================================================
        public void Insert(IEnumerable<Tuple<TKey1, TKey2, TValue>> values)
        {
            InnerRepository.Insert(values.Select(x => Tuple.Create(new object[] { x.Item1, x.Item2 }, x.Item3)));
        }
        //===============================================================
        public void RemoveByKey(TKey1 key1, TKey2 key2)
        {
            InnerRepository.RemoveByKey(key1, key2);
        }
        //===============================================================
        public void RemoveAllByKey(IEnumerable<Tuple<TKey1, TKey2>> keys)
        {
            InnerRepository.RemoveAllByKey(keys.Select(x => new object[] { x.Item1, x.Item2 }));
        }
        //===============================================================
        public void SaveChanges()
        {
            InnerRepository.SaveChanges();
        }
        //===============================================================
        public bool ExistsByKey(TKey1 key1, TKey2 key2)
        {
            return InnerRepository.ExistsByKey(key1, key2);
        }
        //===============================================================
        public ObjectContext<TValue> Find(TKey1 key1, TKey2 key2)
        {
            return InnerRepository.Find(key1, key2);
        }
        //===============================================================
        public EnumerableObjectContext<TValue> Items
        {
            get { return InnerRepository.Items; }
        }
        //===============================================================
        public void Dispose()
        {
            InnerRepository.Dispose();
        }
        //===============================================================

    }
}
