using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Repository.Azure
{
    public class AzureRepository<T> : Repository.Repository<T> where T : class
    {
        //===============================================================
        public AzureRepository(Func<T, object> keySelector, String connectionString, AzureOptions options = null)
            : this(x => new[] { keySelector(x) }, connectionString, options)
        {}
        //===============================================================
        public AzureRepository(Func<T, object[]> keySelector, String connectionString, AzureOptions options = null)
            : this(keySelector, CloudStorageAccount.Parse(connectionString), options)
        {
            ConnectionString = connectionString;
        }
        //===============================================================
        internal AzureRepository(Func<T, object[]> keySelector, CloudStorageAccount storageAccount, AzureOptions options = null)
            : base(keySelector)
        {
            Options = options ?? new AzureOptions();
            Options.ContainerName = Options.ContainerName != null ? AzureUtility.SanitizeContainerName(Options.ContainerName) : AzureUtility.GetSanitizedContainerName<T>();

            AzureContainerInterface = new AzureContainerInterface(storageAccount, Options);
            PendingChanges = new List<IPendingChange>();
        }
        //===============================================================
        public static AzureRepository<T> FromExplicitConnectionString(Func<T, object[]> keySelector, String connectionString, AzureOptions options = null)
        {
            return new AzureRepository<T>(keySelector, connectionString, options);
        }
        //===============================================================
        public static AzureRepository<T> FromExplicitConnectionString(Func<T, object> keySelector, String connectionString, AzureOptions options = null)
        {
            return new AzureRepository<T>(keySelector, connectionString, options);
        }
        //===============================================================
        public static AzureRepository<T> FromNamedConnectionString(Func<T, object[]> keySelector, String connectionStringName, AzureOptions options = null)
        {
            var connStr = AzureUtility.GetNamedConnectionString(connectionStringName);
            return FromExplicitConnectionString(keySelector, connStr, options);
        }
        //===============================================================
        public static AzureRepository<T> FromNamedConnectionString(Func<T, object> keySelector, String connectionStringName, AzureOptions options = null)
        {
            var connStr = AzureUtility.GetNamedConnectionString(connectionStringName);
            return FromExplicitConnectionString(keySelector, connStr, options);
        }
        //===============================================================
        public static AzureRepository<T> ForStorageEmulator(Func<T, object[]> keySelector, AzureOptions options = null)
        {
            return new AzureRepository<T>(keySelector, CloudStorageAccount.DevelopmentStorageAccount, options);
        }
        //===============================================================
        public static AzureRepository<T> ForStorageEmulator(Func<T, object> keySelector, AzureOptions options = null)
        {
            return ForStorageEmulator(x => new[] { keySelector(x) }, options);
        }
        //===============================================================
        private AzureOptions Options { get; set; }
        //===============================================================
        private String ConnectionString { get; set; }
        //===============================================================
        private AzureContainerInterface AzureContainerInterface { get; set; }
        //===============================================================
        private IList<IPendingChange> PendingChanges { get; set; }
        //===============================================================
        public override void Insert(T value)
        {
            PendingChanges.Add(new AzureInsert<T>(KeySelector(value), value, AzureContainerInterface));
        }
        //===============================================================
        public override void RemoveByKey(params object[] keys)
        {
            PendingChanges.Add(new AzureRemove(keys, AzureContainerInterface));
        }
        //===============================================================
        public override void SaveChanges()
        {
            foreach (var change in PendingChanges)
                change.Apply();
        }
        //===============================================================
        public override bool ExistsByKey(params object[] keys)
        {
            return AzureContainerInterface.Exists(keys);
        }
        //===============================================================
        public override void Update<TValue>(TValue value, params object[] keys)
        {
            var existingObj = AzureContainerInterface.GetObject<T>(keys);
            PendingChanges.Add(new AzureModify<T>(existingObj, keys, x => AutoMapper.Mapper.DynamicMap(value, x), AzureContainerInterface));
        }
        //===============================================================
        public override void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter, params object[] keys)
        {
            var existingObj = AzureContainerInterface.GetObject<T>(keys);
            PendingChanges.Add(new AzureModify<T>(existingObj, keys, x => AutoMapper.Mapper.DynamicMap(value, getter(x)), AzureContainerInterface));
        }
        //===============================================================
        public override void Update(string json, UpdateType updateType, params object[] keys)
        {
            throw new NotImplementedException();
        }
        //===============================================================
        public override void Update(string pathToProperty, string json, UpdateType updateType, params object[] keys)
        {
            throw new NotImplementedException();
        }
        //===============================================================
        public override ObjectContext<T> Find(params object[] keys)
        {
            var obj = AzureContainerInterface.GetObject<T>(keys);
            return new ObjectContext<T>(obj);
        }
        //===============================================================
        public Uri GetObjectUri(params Object[] keys)
        {
            return AzureContainerInterface.GetObjectUri(keys);
        }
        //===============================================================
        public override EnumerableObjectContext<T> Items
        {
            get { return new EnumerableObjectContext<T>(AzureContainerInterface.EnumerateObjects<T>().AsQueryable()); }
        }
        //===============================================================
        public override void Dispose()
        {
            // Don't do anything here
        }
        //===============================================================
    }

    public class AzureRepository<TValue, TKey> : Repository<TValue, TKey> where TValue : class
    {
        //===============================================================
        public AzureRepository(Func<TValue, TKey> keySelector, String connectionString, AzureOptions options = null)
            : base(new AzureRepository<TValue>(x => keySelector(x), connectionString, options))
        {}
        //===============================================================
        internal AzureRepository(Func<TValue, TKey> keySelector, CloudStorageAccount storageAccount, AzureOptions options = null)
            : base(new AzureRepository<TValue>(x => new object[] { keySelector(x) }, storageAccount, options))
        {}
        //===============================================================
        public static AzureRepository<TValue, TKey> CreateForStorageEmulator(Func<TValue, TKey> keySelector, AzureOptions options = null)
        {
            return new AzureRepository<TValue, TKey>(keySelector, CloudStorageAccount.DevelopmentStorageAccount, options);
        }
        //===============================================================
        public static AzureRepository<TValue, TKey> FromExplicitConnectionString(Func<TValue, TKey> keySelector, String connectionString, AzureOptions options = null)
        {
            return new AzureRepository<TValue, TKey>(keySelector, connectionString, options);
        }
        //===============================================================
        public static AzureRepository<TValue, TKey> FromNamedConnectionString(Func<TValue, TKey> keySelector, String connectionStringName, AzureOptions options = null)
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

    public class AzureRepository<TValue, TKey1, TKey2> : Repository<TValue, TKey1, TKey2> where TValue : class
    {
        //===============================================================
        public AzureRepository(Func<TValue, Tuple<TKey1, TKey2>> keySelector, String connectionString, AzureOptions options = null)
            : base(new AzureRepository<TValue>(x => new object[] { keySelector(x).Item1, keySelector(x).Item2 }, connectionString, options))
        { }
        //===============================================================
        internal AzureRepository(Func<TValue, Tuple<TKey1, TKey2>> keySelector, CloudStorageAccount storageAccount, AzureOptions options = null)
            : base(new AzureRepository<TValue>(x => new object[] { keySelector(x).Item1, keySelector(x).Item2 }, storageAccount, options))
        { }
        //===============================================================
        public static AzureRepository<TValue, TKey1, TKey2> CreateForStorageEmulator(Func<TValue, Tuple<TKey1, TKey2>> keySelector, AzureOptions options = null)
        {
            return new AzureRepository<TValue, TKey1, TKey2>(keySelector, CloudStorageAccount.DevelopmentStorageAccount, options);
        }
        //===============================================================
        public static AzureRepository<TValue, TKey1, TKey2> FromExplicitConnectionString(Func<TValue, Tuple<TKey1, TKey2>> keySelector, String connectionString, AzureOptions options = null)
        {
            return new AzureRepository<TValue, TKey1, TKey2>(keySelector, connectionString, options);
        }
        //===============================================================
        public static AzureRepository<TValue, TKey1, TKey2> FromNamedConnectionString(Func<TValue, Tuple<TKey1, TKey2>> keySelector, String connectionStringName, AzureOptions options = null)
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
}
