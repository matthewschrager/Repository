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
    internal class FileSystemInterface<T>
    {
        // We have to keep a static dictionary of file locks for each path
        private static IDictionary<String, Object> FileLocks = new ConcurrentDictionary<string, object>(); 

        //===============================================================
        public FileSystemInterface(String repoName, Func<T, object[]> keySelector, FileSystemOptions<T> options)
        {
            Options = options;
            RepositoryName = repoName;
            KeySelector = keySelector;
        }
        //================================================================================
        private Func<T, object[]> KeySelector { get; set; } 
        //===============================================================
        private FileSystemOptions<T> Options { get; set; }
        //================================================================================
        private String RepositoryName { get; set; }
        //===============================================================
        private static String SanitizeName(String name, IEnumerable<char> invalidCharacters, char legalDelimiter = '-')
        {
            foreach (var c in invalidCharacters)
                name = name.Replace(c, legalDelimiter);

            return name;
        }
        //===============================================================
        private static String GetSanitizedTypeName(Type type)
        {
            var name = type.Name;
            if (type.GenericTypeArguments.Any())
                name = type.GenericTypeArguments.Aggregate(name, (current, next) => current + "-" + GetSanitizedTypeName(next));

            return name;
        }
        //================================================================================
        private static Object GetFileLock(String path)
        {
            Object obj;
            if (FileLocks.TryGetValue(path, out obj))
                return obj;

            obj = new object();
            FileLocks[path] = obj;
            return obj;
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
        private IEnumerable<T> RetrieveObjects()
        {
            var filePath = GetRepositoryPath();
            lock (GetFileLock(filePath))
            {
                if (!File.Exists(filePath))
                    return new List<T>();

                using (var reader = new StreamReader(Options.StreamGenerator.GetReadStream(filePath)))
                {
                    var str = reader.ReadToEnd();
                    var objects = Options.Serializer.Deserialize(str);
                    return objects;
                }
            }
        }
        //===============================================================
        private void SaveObjects(IEnumerable<T> objects)
        {
            var filePath = GetRepositoryPath();
            lock (GetFileLock(filePath))
            {
                if (!String.IsNullOrWhiteSpace(Path.GetDirectoryName(filePath)) && !Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var writer = new StreamWriter(Options.StreamGenerator.GetWriteStream(filePath)))
                {
                    writer.Write(Options.Serializer.Serialize(objects));
                    writer.Flush();
                }
            }
        }
        //===============================================================
        public void StoreObject(T value, IEnumerable<object> keys)
        {
            var objKey = GetObjectKey(keys);
            var objects = RetrieveObjects().ToDictionary(x => GetObjectKey(KeySelector(x)));
            objects[objKey] = value;

            SaveObjects(objects.Values);
        }
        //================================================================================
        public void StoreObjects(IEnumerable<KeyValuePair<IEnumerable<object>, T>> keyValuePairs)
        {
            var objects = RetrieveObjects().ToDictionary(x => GetObjectKey(KeySelector(x)));
            foreach (var newValue in keyValuePairs)
                objects[GetObjectKey(newValue.Key)] = newValue.Value;

            SaveObjects(objects.Values);
        }
        //===============================================================
        public ObjectContext<T> GetObject(IEnumerable<object> keys)
        {
            var key = GetObjectKey(keys);
            var objects = RetrieveObjects().ToDictionary(x => GetObjectKey(KeySelector(x)));
            
            T obj;
            if (!objects.TryGetValue(key, out obj))
                return null;

            return new ObjectContext<T>(obj);
        }
        //===============================================================
        public bool Exists(IEnumerable<object> keys)
        {
            return RetrieveObjects().ToDictionary(x => GetObjectKey(KeySelector(x))).ContainsKey(GetObjectKey(keys));
        }
        //===============================================================
        public void DeleteObject(IEnumerable<object> keys)
        {
            var objects = RetrieveObjects().ToDictionary(x => GetObjectKey(KeySelector(x)));
            objects.Remove(GetObjectKey(keys));
            SaveObjects(objects.Values);
        }
        //===============================================================
        public void DeleteFolder()
        {
            Directory.Delete(GetBaseFolderPath());
        }
        //===============================================================
        public IQueryable<T> EnumerateObjects()
        {
            return RetrieveObjects().AsQueryable();
        }
        //===============================================================
    }
}
