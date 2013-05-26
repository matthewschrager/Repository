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
        public FileSystemRepository(Func<T, object[]> keySelector, FileSystemOptions<T> options = null)
            : base(keySelector)
        {
            options = options ?? new FileSystemOptions<T>();
            FileSystemInterface = new FileSystemInterface<T>(options);
        }
        //===============================================================
        public FileSystemRepository(Func<T, object> keySelector, FileSystemOptions<T> options = null)
            : this(x => new[] { keySelector(x) }, options)
        {}
        //===============================================================
        private FileSystemInterface<T> FileSystemInterface { get; set; }
        //===============================================================
        protected override Insert<T> CreateInsert(IEnumerable<object> keys, T value)
        {
            return new FileSystemInsert<T>(keys, value, FileSystemInterface);
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
            get { return new EnumerableObjectContext<T>(FileSystemInterface.EnumerateObjects(), this); }
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
        public FileSystemRepository(Func<TValue, TKey> keySelector, FileSystemOptions<TValue> options = null)
            : base(new FileSystemRepository<TValue>(x => keySelector(x), options))
        {}
        //===============================================================
    }

    public class FileSystemRepository<TValue, TKey1, TKey2> : Repository<TValue, TKey1, TKey2>
    {
        //===============================================================
        public FileSystemRepository(Func<TValue, Tuple<TKey1, TKey2>> keySelector, FileSystemOptions<TValue> options = null)
            : base(new FileSystemRepository<TValue>(x => new object[] { keySelector(x).Item1, keySelector(x).Item2 }, options))
        {}
        //===============================================================
    }

    // Explicit key implementations
    public class ExplicitKeyFileSystemRepository<T> : ExplicitKeyRepository<T>
    {
        //===============================================================
        public ExplicitKeyFileSystemRepository(FileSystemOptions<T> options = null)
            : base(new FileSystemRepository<T>(NullKeySelector, options))
        {}
        //===============================================================

    }

    public class ExplicitKeyFileSystemRepository<TValue, TKey> : ExplicitKeyRepository<TValue, TKey>
    {
        //===============================================================
        public ExplicitKeyFileSystemRepository(FileSystemOptions<TValue> options = null)
            : base(new FileSystemRepository<TValue>(NullKeySelector, options))
        { }
        //===============================================================
    }

    public class ExplicitKeyFileSystemRepository<TValue, TKey1, TKey2> : ExplicitKeyRepository<TValue, TKey1, TKey2>
    {
        //===============================================================
        public ExplicitKeyFileSystemRepository(FileSystemOptions<TValue> options = null)
            : base(new FileSystemRepository<TValue>(NullKeySelector, options))
        { }
        //===============================================================
    }
}
