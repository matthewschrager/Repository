using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;

namespace Repository.Azure
{
    public class AzureRepository<T> : Repository.Repository<T>
    {
        //===============================================================
        public AzureRepository(Func<T, object> keySelector, String connectionString, AzureOptions<T> options = null)
            : this(x => new[] { keySelector(x) }, connectionString, options)
        {}
        //===============================================================
        public AzureRepository(Func<T, object[]> keySelector, String connectionString, AzureOptions<T> options = null)
            : this(keySelector, CloudStorageAccount.Parse(connectionString), options)
        {
            ConnectionString = connectionString;
        }
        //===============================================================
        internal AzureRepository(Func<T, object[]> keySelector, CloudStorageAccount storageAccount, AzureOptions<T> options = null)
            : base(keySelector)
        {
            Options = options ?? new AzureOptions<T>();
            Options.ContainerName = Options.ContainerName != null ? AzureUtility.SanitizeContainerName(Options.ContainerName) : AzureUtility.GetSanitizedContainerName<T>();

            AzureContainerInterface = new AzureContainerInterface<T>(storageAccount, Options);
            PendingChanges = new List<IOperation>();
        }
        //===============================================================
        public static AzureRepository<T> FromExplicitConnectionString(Func<T, object[]> keySelector, String connectionString, AzureOptions<T> options = null)
        {
            return new AzureRepository<T>(keySelector, connectionString, options);
        }
        //===============================================================
        public static AzureRepository<T> FromExplicitConnectionString(Func<T, object> keySelector, String connectionString, AzureOptions<T> options = null)
        {
            return new AzureRepository<T>(keySelector, connectionString, options);
        }
        //===============================================================
        public static AzureRepository<T> FromNamedConnectionString(Func<T, object[]> keySelector, String connectionStringName, AzureOptions<T> options = null)
        {
            var connStr = AzureUtility.GetNamedConnectionString(connectionStringName);
            return FromExplicitConnectionString(keySelector, connStr, options);
        }
        //===============================================================
        public static AzureRepository<T> FromNamedConnectionString(Func<T, object> keySelector, String connectionStringName, AzureOptions<T> options = null)
        {
            var connStr = AzureUtility.GetNamedConnectionString(connectionStringName);
            return FromExplicitConnectionString(keySelector, connStr, options);
        }
        //===============================================================
        public static AzureRepository<T> ForStorageEmulator(Func<T, object[]> keySelector, AzureOptions<T> options = null)
        {
            return new AzureRepository<T>(keySelector, CloudStorageAccount.DevelopmentStorageAccount, options);
        }
        //===============================================================
        public static AzureRepository<T> ForStorageEmulator(Func<T, object> keySelector, AzureOptions<T> options = null)
        {
            return ForStorageEmulator(x => new[] { keySelector(x) }, options);
        }
        //===============================================================
        private AzureOptions<T> Options { get; set; }
        //===============================================================
        private String ConnectionString { get; set; }
        //===============================================================
        private AzureContainerInterface<T> AzureContainerInterface { get; set; }
        //===============================================================
        private IList<IOperation> PendingChanges { get; set; }
        //===============================================================
        protected override Insert<T> CreateInsert(IEnumerable<object> keys, T value)
        {
            return new AzureInsert<T>(KeySelector(value), value, AzureContainerInterface);
        }
        //===============================================================
        protected override Remove CreateRemove(IEnumerable<object> keys)
        {
            return new AzureRemove<T>(keys, AzureContainerInterface);
        }
        //===============================================================
        protected override Modify<T> CreateModify(IEnumerable<object> keys, T value, Action<T> modifier)
        {
            return new AzureModify<T>(value, keys, modifier, AzureContainerInterface);
        }
        //===============================================================
        public override bool ExistsByKey(params object[] keys)
        {
            return AzureContainerInterface.Exists(keys);
        }
        //===============================================================
        public Uri GetObjectUri(params Object[] keys)
        {
            return AzureContainerInterface.GetObjectUri(keys);
        }
        //===============================================================
        protected override ObjectContext<T> FindImpl(object[] keys)
        {
            var obj = AzureContainerInterface.GetObject(keys);
            if (!obj.HasObject)
                return null;

            return new ObjectContext<T>(obj.Object);
        }
        //===============================================================
        public override EnumerableObjectContext<T> Items
        {
            get { return new EnumerableObjectContext<T>(AzureContainerInterface.EnumerateObjects().AsQueryable(), this); }
        }
        //===============================================================
        public override void Dispose()
        {
            // Don't do anything here
        }
        //===============================================================
    }

    public class AzureRepository<TValue, TKey> : Repository<TValue, TKey>
    {
        //===============================================================
        public AzureRepository(Func<TValue, TKey> keySelector, String connectionString, AzureOptions<TValue> options = null)
            : base(new AzureRepository<TValue>(x => keySelector(x), connectionString, options))
        {}
        //===============================================================
        internal AzureRepository(Func<TValue, TKey> keySelector, CloudStorageAccount storageAccount, AzureOptions<TValue> options = null)
            : base(new AzureRepository<TValue>(x => new object[] { keySelector(x) }, storageAccount, options))
        {}
        //===============================================================
        public static AzureRepository<TValue, TKey> ForStorageEmulator(Func<TValue, TKey> keySelector, AzureOptions<TValue> options = null)
        {
            return new AzureRepository<TValue, TKey>(keySelector, CloudStorageAccount.DevelopmentStorageAccount, options);
        }
        //===============================================================
        public static AzureRepository<TValue, TKey> FromExplicitConnectionString(Func<TValue, TKey> keySelector, String connectionString, AzureOptions<TValue> options = null)
        {
            return new AzureRepository<TValue, TKey>(keySelector, connectionString, options);
        }
        //===============================================================
        public static AzureRepository<TValue, TKey> FromNamedConnectionString(Func<TValue, TKey> keySelector, String connectionStringName, AzureOptions<TValue> options = null)
        {
            var connStr = AzureUtility.GetNamedConnectionString(connectionStringName);
            return FromExplicitConnectionString(keySelector, connStr, options);
        }
        //===============================================================
        public Uri GetObjectUri(TKey key)
        {
            return (InnerRepository as AzureRepository<TValue>).GetObjectUri(key);
        }
        //===============================================================
    }

    public class AzureRepository<TValue, TKey1, TKey2> : Repository<TValue, TKey1, TKey2>
    {
        //===============================================================
        public AzureRepository(Func<TValue, Tuple<TKey1, TKey2>> keySelector, String connectionString, AzureOptions<TValue> options = null)
            : base(new AzureRepository<TValue>(x => new object[] { keySelector(x).Item1, keySelector(x).Item2 }, connectionString, options))
        { }
        //===============================================================
        internal AzureRepository(Func<TValue, Tuple<TKey1, TKey2>> keySelector, CloudStorageAccount storageAccount, AzureOptions<TValue> options = null)
            : base(new AzureRepository<TValue>(x => new object[] { keySelector(x).Item1, keySelector(x).Item2 }, storageAccount, options))
        { }
        //===============================================================
        public static AzureRepository<TValue, TKey1, TKey2> ForStorageEmulator(Func<TValue, Tuple<TKey1, TKey2>> keySelector, AzureOptions<TValue> options = null)
        {
            return new AzureRepository<TValue, TKey1, TKey2>(keySelector, CloudStorageAccount.DevelopmentStorageAccount, options);
        }
        //===============================================================
        public static AzureRepository<TValue, TKey1, TKey2> FromExplicitConnectionString(Func<TValue, Tuple<TKey1, TKey2>> keySelector, String connectionString, AzureOptions<TValue> options = null)
        {
            return new AzureRepository<TValue, TKey1, TKey2>(keySelector, connectionString, options);
        }
        //===============================================================
        public static AzureRepository<TValue, TKey1, TKey2> FromNamedConnectionString(Func<TValue, Tuple<TKey1, TKey2>> keySelector, String connectionStringName, AzureOptions<TValue> options = null)
        {
            var connStr = AzureUtility.GetNamedConnectionString(connectionStringName);
            return FromExplicitConnectionString(keySelector, connStr, options);
        }
        //===============================================================
        public Uri GetObjectUri(TKey1 key1, TKey2 key2)
        {
            return (InnerRepository as AzureRepository<TValue>).GetObjectUri(key1, key2);
        }
        //===============================================================
    }

    public class ExplicitKeyAzureRepository<TValue> : ExplicitKeyRepository<TValue>
    {
        //===============================================================
        public ExplicitKeyAzureRepository(String connectionString, AzureOptions<TValue> options = null)
            : base(new AzureRepository<TValue>(x => new object[] { }, connectionString, options))
        {}
        //===============================================================
        public static ExplicitKeyAzureRepository<TValue> ForStorageEmulator(AzureOptions<TValue> options = null)
        {
            return new ExplicitKeyAzureRepository<TValue>(AzureUtility.EMULATOR_CONNECTION_STRING, options);
        }
        //===============================================================
        public static ExplicitKeyAzureRepository<TValue> FromExplicitConnectionString(String connectionString, AzureOptions<TValue> options = null)
        {
            return new ExplicitKeyAzureRepository<TValue>(connectionString, options);
        }
        //===============================================================
        public static ExplicitKeyAzureRepository<TValue> FromNamedConnectionString(String connectionStringName, AzureOptions<TValue> options = null)
        {
            var connStr = AzureUtility.GetNamedConnectionString(connectionStringName);
            return FromExplicitConnectionString(connStr, options);
        }
        //===============================================================
        public Uri GetObjectUri(params object[] keys)
        {
            return (ImplicitKeyRepository as AzureRepository<TValue>).GetObjectUri(keys);
        }
        //===============================================================
    }

    public class ExplicitKeyAzureRepository<TValue, TKey> : ExplicitKeyRepository<TValue, TKey>
    {
        //===============================================================
        public ExplicitKeyAzureRepository(String connectionString, AzureOptions<TValue> options = null)
            : base(new AzureRepository<TValue>(x => new object[] { }, connectionString, options))
        {}
        //===============================================================
        public static ExplicitKeyAzureRepository<TValue, TKey> ForStorageEmulator(AzureOptions<TValue> options = null)
        {
            return new ExplicitKeyAzureRepository<TValue, TKey>(AzureUtility.EMULATOR_CONNECTION_STRING, options);
        }
        //===============================================================
        public static ExplicitKeyAzureRepository<TValue, TKey> FromExplicitConnectionString(String connectionString, AzureOptions<TValue> options = null)
        {
            return new ExplicitKeyAzureRepository<TValue, TKey>(connectionString, options);
        }
        //===============================================================
        public static ExplicitKeyAzureRepository<TValue, TKey> FromNamedConnectionString(String connectionStringName, AzureOptions<TValue> options = null)
        {
            var connStr = AzureUtility.GetNamedConnectionString(connectionStringName);
            return FromExplicitConnectionString(connStr, options);
        }
        //===============================================================
        public Uri GetObjectUri(TKey key)
        {
            return (InnerRepository.ImplicitKeyRepository as AzureRepository<TValue>).GetObjectUri(key);
        }
        //===============================================================
    }

    public class ExplicitKeyAzureRepository<TValue, TKey1, TKey2> : ExplicitKeyRepository<TValue, TKey1, TKey2>
    {
        //===============================================================
        public ExplicitKeyAzureRepository(String connectionString, AzureOptions<TValue> options = null)
            : base(new AzureRepository<TValue>(x => new object[] { }, connectionString, options))
        { }
        //===============================================================
        public static ExplicitKeyAzureRepository<TValue, TKey1, TKey2> ForStorageEmulator(AzureOptions<TValue> options = null)
        {
            return new ExplicitKeyAzureRepository<TValue, TKey1, TKey2>(AzureUtility.EMULATOR_CONNECTION_STRING, options);
        }
        //===============================================================
        public static ExplicitKeyAzureRepository<TValue, TKey1, TKey2> FromExplicitConnectionString(String connectionString, AzureOptions<TValue> options = null)
        {
            return new ExplicitKeyAzureRepository<TValue, TKey1, TKey2>(connectionString, options);
        }
        //===============================================================
        public static ExplicitKeyAzureRepository<TValue, TKey1, TKey2> FromNamedConnectionString(String connectionStringName, AzureOptions<TValue> options = null)
        {
            var connStr = AzureUtility.GetNamedConnectionString(connectionStringName);
            return FromExplicitConnectionString(connStr, options);
        }
        //===============================================================
        public Uri GetObjectUri(TKey1 key1, TKey2 key2)
        {
            return (InnerRepository.ImplicitKeyRepository as AzureRepository<TValue>).GetObjectUri(key1, key2);
        }
        //===============================================================
    }
}
