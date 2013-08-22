using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ReadOnlyRepository<T> : IDisposable
    {
        //================================================================================
        public ReadOnlyRepository(Repository<T> innerRepository)
        {
            InnerRepository = innerRepository;
        }
        //================================================================================
        private Repository<T> InnerRepository { get; set; }
        //================================================================================
        public bool Exists(T obj)
        {
            return InnerRepository.Exists(obj);
        }
        //================================================================================
        public bool ExistsByKey(params Object[] keys)
        {
            return InnerRepository.ExistsByKey(keys);
        }
        //================================================================================
        public ObjectContext<T> Find(params Object[] keys)
        {
            return InnerRepository.Find(keys);
        }
        //================================================================================
        public EnumerableObjectContext<T> Items
        {
            get { return InnerRepository.Items; }
        }
        //================================================================================
        public void Dispose()
        {
            InnerRepository.Dispose();
        }
        //================================================================================
    }

    public class ReadOnlyRepository<TValue, TKey> : IDisposable
    {
        //================================================================================
        public ReadOnlyRepository(Repository<TValue, TKey> innerRepository)
        {
            InnerRepository = innerRepository;
        }
        //================================================================================
        private Repository<TValue, TKey> InnerRepository { get; set; }
        //================================================================================
        public bool Exists(TValue obj)
        {
            return InnerRepository.Exists(obj);
        }
        //================================================================================
        public bool ExistsByKey(TKey key)
        {
            return InnerRepository.ExistsByKey(key);
        }
        //================================================================================
        public ObjectContext<TValue> Find(TKey key)
        {
            return InnerRepository.Find(key);
        }
        //================================================================================
        public EnumerableObjectContext<TValue> Items
        {
            get { return InnerRepository.Items; }
        }
        //================================================================================
        public void Dispose()
        {
            InnerRepository.Dispose();
        }
        //================================================================================
    }

    public class ReadOnlyRepository<TValue, TKey1, TKey2> : IDisposable
    {
        //================================================================================
        public ReadOnlyRepository(Repository<TValue, TKey1, TKey2> innerRepository)
        {
            InnerRepository = innerRepository;
        }
        //================================================================================
        private Repository<TValue, TKey1, TKey2> InnerRepository { get; set; }
        //================================================================================
        public bool Exists(TValue obj)
        {
            return InnerRepository.Exists(obj);
        }
        //================================================================================
        public bool ExistsByKey(TKey1 key1, TKey2 key2)
        {
            return InnerRepository.ExistsByKey(key1, key2);
        }
        //================================================================================
        public ObjectContext<TValue> Find(TKey1 key1, TKey2 key2)
        {
            return InnerRepository.Find(key1, key2);
        }
        //================================================================================
        public EnumerableObjectContext<TValue> Items
        {
            get { return InnerRepository.Items; }
        }
        //================================================================================
        public void Dispose()
        {
            InnerRepository.Dispose();
        }
        //================================================================================
    }
}
