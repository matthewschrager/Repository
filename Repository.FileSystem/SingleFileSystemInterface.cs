using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Repository.FileSystem
{
    internal abstract class FileSystemInterface<T>
    {
        // We have to keep a static dictionary of file locks for each path
        private static IDictionary<String, Object> FileLocks = new ConcurrentDictionary<string, object>();

        public FileSystemInterface(String repoName, Func<T, object[]> keySelector, FileSystemOptions<T> options)
        {
            RepositoryName = repoName;
            KeySelector = keySelector;
            Options = options;
        }

        public Func<T, object[]> KeySelector { get; set; }
        public FileSystemOptions<T> Options { get; private set; }
        public String RepositoryName { get; private set; }

        public abstract void StoreObject(T value, IEnumerable<Object> keys);
        public abstract void StoreObjects(IEnumerable<KeyValuePair<IEnumerable<Object>, T>> keyValuePairs);
        public abstract ObjectContext<T> GetObject(IEnumerable<Object> keys);
        public abstract bool Exists(IEnumerable<Object> keys);
        public abstract void DeleteObject(IEnumerable<Object> keys);
        public abstract IEnumerable<T> EnumerateObjects();

        protected static Object GetFileLock(String path)
        {
            Object obj;
            if (FileLocks.TryGetValue(path, out obj))
                return obj;

            obj = new object();
            FileLocks[path] = obj;
            return obj;
        }

        protected static String SanitizeName(String name, IEnumerable<char> invalidCharacters, char legalDelimiter = '-')
        {
            foreach (var c in invalidCharacters)
                name = name.Replace(c, legalDelimiter);

            return name;
        }

        protected IEnumerable<T> RetrieveObjects(String filePath)
        {
            lock (GetFileLock(filePath))
            {
                if (!File.Exists(filePath))
                    return new List<T>();

                using (var stream = Options.StreamGenerator.GetReadStream(filePath))
                {
                    var objects = Options.Serializer.Deserialize(stream);
                    return objects;
                }
            }
        }

        protected void SaveObjects(IEnumerable<T> objects, String filePath)
        {
            lock (GetFileLock(filePath))
            {
                if (!String.IsNullOrWhiteSpace(Path.GetDirectoryName(filePath)) && !Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = Options.StreamGenerator.GetWriteStream(filePath))
                {
                    Options.Serializer.Serialize(objects.ToList(), stream);
                    stream.Flush();
                }
            }
        }
    }

    internal class SingleFileSystemInterface<T> : FileSystemInterface<T>
    {
        //===============================================================
        public SingleFileSystemInterface(String repoName, Func<T, object[]> keySelector, FileSystemOptions<T> options)
            : base(repoName, keySelector, options)
        {}
        //===============================================================
        private static String GetSanitizedTypeName(Type type)
        {
            var name = type.Name;
            if (type.GenericTypeArguments.Any())
                name = type.GenericTypeArguments.Aggregate(name, (current, next) => current + "-" + GetSanitizedTypeName(next));

            return name;
        }
        //================================================================================
        private String GetObjectKey(IEnumerable<Object> keys)
        {
            return keys.Aggregate("", (curr, next) => curr + "-" + next);
        }
        //===============================================================
        private String GetBaseFolderPath()
        {
            if (Options.UseTypeNameFolder)
                return Path.Combine(Options.FolderPath, SanitizeName(GetSanitizedTypeName(typeof(T)), Path.GetInvalidPathChars()));
            else
                return Options.FolderPath;
        }
        //===============================================================
        private String GetRepositoryPath()
        {
            return Path.Combine(GetBaseFolderPath(), SanitizeName(RepositoryName, Path.GetInvalidFileNameChars())) + Options.FileExtension;
        }
        //===============================================================
        public override void StoreObject(T value, IEnumerable<object> keys)
        {
            var objKey = GetObjectKey(keys);
            var objects = RetrieveObjects(GetRepositoryPath()).ToDictionary(x => GetObjectKey(KeySelector(x)));
            objects[objKey] = value;

            SaveObjects(objects.Values, GetRepositoryPath());
        }
        //================================================================================
        public override void StoreObjects(IEnumerable<KeyValuePair<IEnumerable<object>, T>> keyValuePairs)
        {
            var objects = RetrieveObjects(GetRepositoryPath()).ToDictionary(x => GetObjectKey(KeySelector(x)));
            foreach (var newValue in keyValuePairs)
                objects[GetObjectKey(newValue.Key)] = newValue.Value;

            SaveObjects(objects.Values, GetRepositoryPath());
        }
        //===============================================================
        public override ObjectContext<T> GetObject(IEnumerable<object> keys)
        {
            var key = GetObjectKey(keys);
            var objects = RetrieveObjects(GetRepositoryPath()).ToDictionary(x => GetObjectKey(KeySelector(x)));
            
            T obj;
            if (!objects.TryGetValue(key, out obj))
                return null;

            return new ObjectContext<T>(obj);
        }
        //===============================================================
        public override bool Exists(IEnumerable<object> keys)
        {
            return RetrieveObjects(GetRepositoryPath()).ToDictionary(x => GetObjectKey(KeySelector(x))).ContainsKey(GetObjectKey(keys));
        }
        //===============================================================
        public override void DeleteObject(IEnumerable<object> keys)
        {
            var objects = RetrieveObjects(GetRepositoryPath()).ToDictionary(x => GetObjectKey(KeySelector(x)));
            objects.Remove(GetObjectKey(keys));
            SaveObjects(objects.Values, GetRepositoryPath());
        }
        //===============================================================
        public void DeleteFolder()
        {
            Directory.Delete(GetBaseFolderPath());
        }
        //===============================================================
        public override IEnumerable<T> EnumerateObjects()
        {
            return RetrieveObjects(GetRepositoryPath());
        }
        //===============================================================
    }

    internal class MultipleFileSystemInterface<T> : FileSystemInterface<T>
    {
        public MultipleFileSystemInterface(string repoName, Func<T, object[]> keySelector, FileSystemOptions<T> options) 
            : base(repoName, keySelector, options)
        {}

        private String GetRepositoryFolder()
        {
            return Path.Combine(Options.FolderPath, SanitizeName(RepositoryName, Path.GetInvalidPathChars()));
        }

        private String GetObjectKey(IEnumerable<Object> keys)
        {
            var key = keys.Count() > 1 ? keys.Aggregate("", (curr, next) => curr + "-" + next) : keys.First().ToString();
            return SanitizeName(key, Path.GetInvalidFileNameChars());
        }

        private String GetObjectPath(IEnumerable<object> keys)
        {
            return Path.Combine(GetRepositoryFolder(), GetObjectKey(keys)) + Options.FileExtension;
        }

        public override void StoreObject(T value, IEnumerable<object> keys)
        {
            SaveObjects(new[] { value }, GetObjectPath(keys));
        }

        public override void StoreObjects(IEnumerable<KeyValuePair<IEnumerable<object>, T>> keyValuePairs)
        {
            foreach (var pair in keyValuePairs)
                StoreObject(pair.Value, pair.Key);
        }

        public override ObjectContext<T> GetObject(IEnumerable<object> keys)
        {
            var objects = RetrieveObjects(GetObjectPath(keys));
            return objects.Any() ? new ObjectContext<T>(objects.First()) : null;
        }

        public override bool Exists(IEnumerable<object> keys)
        {
            return File.Exists(GetObjectPath(keys));
        }

        public override void DeleteObject(IEnumerable<object> keys)
        {
            File.Delete(GetObjectPath(keys));
        }

        public override IEnumerable<T> EnumerateObjects()
        {
            var folderPath = GetRepositoryFolder();
            if (!Directory.Exists(folderPath))
                yield break;

            var files = Directory.EnumerateFiles(folderPath);
            foreach (var f in files)
                yield return RetrieveObjects(f).First();
        }
    }
}
