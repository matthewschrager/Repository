using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public abstract class Repository<TValue, TKey> : IDisposable where TValue : class
    {
        //===============================================================
        protected Repository(Repository<TValue> innerRepository)
        {
            InnerRepository = innerRepository;
        }
        //===============================================================
        private Repository<TValue> InnerRepository { get; set; }
        //===============================================================
        public void Insert(TValue value)
        {
            InnerRepository.Insert(value);
        }
        //===============================================================
        public void Insert(IEnumerable<TValue> values)
        {
            InnerRepository.Insert(values);
        }
        //===============================================================
        public void Remove(TValue obj)
        {
            InnerRepository.Remove(obj);
        }
        //===============================================================
        public void RemoveAll(IEnumerable<TValue> objects = null)
        {
            InnerRepository.RemoveAll(objects);
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
        public bool Exists(TValue obj)
        {
            return InnerRepository.Exists(obj);
        }
        //===============================================================
        public bool ExistsByKey(TKey key)
        {
            return InnerRepository.ExistsByKey(key);
        }
        //===============================================================
        public void Update<TUpdate>(TKey key, TUpdate updateObj)
        {
            InnerRepository.Update(updateObj, key);
        }
        //===============================================================
        public void Update<TUpdate, TProperty>(TKey key, TUpdate updateObj, Func<TValue, TProperty> getter)
        {
            InnerRepository.Update(updateObj, getter, key);
        }
        //===============================================================
        public void Update(TKey key, String json, UpdateType updateType)
        {
            InnerRepository.Update(json, updateType, key);
        }
        //===============================================================
        public void Update(TKey key, String pathToProperty, String json, UpdateType updateType)
        {
            InnerRepository.Update(pathToProperty, json, updateType, key);
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

    public abstract class Repository<TValue, TKey1, TKey2> : IDisposable where TValue : class
    {
        //===============================================================
        protected Repository(Repository<TValue> innerRepository)
        {
            InnerRepository = innerRepository;
        }
        //===============================================================
        private Repository<TValue> InnerRepository { get; set; }
        //===============================================================
        public void Insert(TValue value)
        {
            InnerRepository.Insert(value);
        }
        //===============================================================
        public void Insert(IEnumerable<TValue> values)
        {
            InnerRepository.Insert(values);
        }
        //===============================================================
        public void Remove(TValue obj)
        {
            InnerRepository.Remove(obj);
        }
        //===============================================================
        public void RemoveAll(IEnumerable<TValue> objects)
        {
            InnerRepository.RemoveAll(objects);
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
        public bool Exists(TValue obj)
        {
            return InnerRepository.Exists(obj);
        }
        //===============================================================
        public bool ExistsByKey(TKey1 key1, TKey2 key2)
        {
            return InnerRepository.ExistsByKey(key1, key2);
        }
        //===============================================================
        public void Update<TUpdate>(TKey1 key1, TKey2 key2, TUpdate updateObj)
        {
            InnerRepository.Update(updateObj, key1, key2);
        }
        //===============================================================
        public void Update<TUpdate, TProperty>(TKey1 key1, TKey2 key2, TUpdate updateObj, Func<TValue, TProperty> getter)
        {
            InnerRepository.Update(updateObj, getter, key1, key2);
        }
        //===============================================================
        public void Update(TKey1 key1, TKey2 key2, String json, UpdateType updateType)
        {
            InnerRepository.Update(json, updateType, key1, key2);
        }
        //===============================================================
        public void Update(TKey1 key1, TKey2 key2, String pathToProperty, String json, UpdateType updateType)
        {
            InnerRepository.Update(pathToProperty, json, updateType, key1, key2);
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
