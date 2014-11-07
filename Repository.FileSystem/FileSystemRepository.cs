using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FileSystem
{
    public class FileSystemRepository<T> : Repository<T>
    {
        //===============================================================
        public FileSystemRepository(String name, Func<T, object[]> keySelector, FileSystemOptions<T> options = null)
            : base(keySelector)
        {
            options = options ?? new FileSystemOptions<T>();
            FileSystemInterface = options.FileStorageType == FileStorageType.SingleFile ? (FileSystemInterface<T>)new SingleFileSystemInterface<T>(name, keySelector, options) : new MultipleFileSystemInterface<T>(name, keySelector, options);
        }
        //===============================================================
        public FileSystemRepository(String name, Func<T, object> keySelector, FileSystemOptions<T> options = null)
            : this(name, x => new[] { keySelector(x) }, options)
        {}
        //===============================================================
        private FileSystemInterface<T> FileSystemInterface { get; set; }
        //===============================================================
        protected override Insert<T> CreateInsert(IEnumerable<object> keys, T value)
        {
            return new FileSystemInsert<T>(keys, value, FileSystemInterface);
        }
        //================================================================================
        protected override BatchInsert<T> CreateBatchInsert(IEnumerable<KeyValuePair<IEnumerable<object>, T>> keyValuePairs)
        {
            return new FileSystemBatchInsert<T>(keyValuePairs, FileSystemInterface);
        }
        //===============================================================
        protected override Remove CreateRemove(IEnumerable<object> keys)
        {
            return new FileSystemRemove<T>(keys, FileSystemInterface);
        }
        //===============================================================
        protected override Modify<T> CreateModify(IEnumerable<object> keys, T value, Action<T> modifier)
        {
            return new FileSystemModify<T>(keys, value, modifier, FileSystemInterface);
        }
        //===============================================================
        public override bool ExistsByKey(params object[] keys)
        {
            return FileSystemInterface.Exists(keys);
        }
        //===============================================================
        protected override ObjectContext<T> FindImpl(object[] keys)
        {
            return FileSystemInterface.GetObject(keys);
        }
        //===============================================================
        public override EnumerableObjectContext<T> Items
        {
            get { return new EnumerableObjectContext<T>(FileSystemInterface.EnumerateObjects().AsQueryable(), this); }
        }
        //===============================================================
        public override void OnKeySelectorChanged(Func<T, object[]> newKeySelector)
        {
            FileSystemInterface.KeySelector = newKeySelector;
        }
        //===============================================================
        public void CreateBackup()
        {
            FileSystemInterface.CreateBackup();
        }
        //===============================================================
        public override void Dispose()
        {
            // Don't do anything, nothing to dispose
        }
        //===============================================================
    }

    // Typed key implementations
    public class FileSystemRepository<TValue, TKey> : Repository<TValue, TKey>
    {
        //===============================================================
        public FileSystemRepository(String name, Func<TValue, TKey> keySelector, FileSystemOptions<TValue> options = null)
            : base(new FileSystemRepository<TValue>(name, x => keySelector(x), options))
        {}
        //===============================================================
    }

    public class FileSystemRepository<TValue, TKey1, TKey2> : Repository<TValue, TKey1, TKey2>
    {
        //===============================================================
        public FileSystemRepository(String name, Func<TValue, Tuple<TKey1, TKey2>> keySelector, FileSystemOptions<TValue> options = null)
            : base(new FileSystemRepository<TValue>(name, x => new object[] { keySelector(x).Item1, keySelector(x).Item2 }, options))
        {}
        //===============================================================
    }

    // Explicit key implementations
    public class ExplicitKeyFileSystemRepository<T> : ExplicitKeyRepository<T>
    {
        //===============================================================
        public ExplicitKeyFileSystemRepository(String name, FileSystemOptions<T> options = null)
            : base(new FileSystemRepository<T>(name, NullKeySelector, options))
        {}
        //===============================================================

    }

    public class ExplicitKeyFileSystemRepository<TValue, TKey> : ExplicitKeyRepository<TValue, TKey>
    {
        //===============================================================
        public ExplicitKeyFileSystemRepository(String name, FileSystemOptions<TValue> options = null)
            : base(new FileSystemRepository<TValue>(name, NullKeySelector, options))
        { }
        //===============================================================
    }

    public class ExplicitKeyFileSystemRepository<TValue, TKey1, TKey2> : ExplicitKeyRepository<TValue, TKey1, TKey2>
    {
        //===============================================================
        public ExplicitKeyFileSystemRepository(String name, FileSystemOptions<TValue> options = null)
            : base(new FileSystemRepository<TValue>(name, NullKeySelector, options))
        { }
        //===============================================================
    }
}
