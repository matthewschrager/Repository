using System;
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
        //===============================================================
        public FileSystemInterface(FileSystemOptions<T> options)
        {
            Options = options;
        }
        //===============================================================
        private FileSystemOptions<T> Options { get; set; }
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
        //===============================================================
        private String GetFolderPath()
        {
            return Path.Combine(Options.FolderPath, SanitizeName(GetSanitizedTypeName(typeof(T)), Path.GetInvalidPathChars()));
        }
        //===============================================================
        private String GetFilePath(IEnumerable<object> keys)
        {
            var fileName = keys.Select(x => x.ToString()).Aggregate((curr, next) => curr + "-" + next);
            fileName = SanitizeName(fileName, Path.GetInvalidFileNameChars());

            return Path.Combine(GetFolderPath(), fileName) + Options.FileExtension;
        }
        //===============================================================
        public void StoreObject(T value, IEnumerable<object> keys)
        {
            var filePath = GetFilePath(keys);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var writer = new StreamWriter(Options.StreamGenerator.GetWriteStream(filePath)))
            {
                writer.Write(Options.Serializer.Serialize(value));
                writer.Flush();
            }
        }
        //===============================================================
        public ObjectContext<T> GetObject(IEnumerable<object> keys)
        {
            var path = GetFilePath(keys);
            if (!File.Exists(path))
                return null;

            return GetObject(path);
        }
        //===============================================================
        private ObjectContext<T> GetObject(String path)
        {
            using (var reader = new StreamReader(Options.StreamGenerator.GetReadStream(path)))
            {
                var str = reader.ReadToEnd();
                var obj = Options.Serializer.Deserialize(str);
                return new ObjectContext<T>(obj);
            }
        }
        //===============================================================
        public bool Exists(IEnumerable<object> keys)
        {
            return File.Exists(GetFilePath(keys));
        }
        //===============================================================
        public void DeleteObject(IEnumerable<object> keys)
        {
            File.Delete(GetFilePath(keys));
        }
        //===============================================================
        public void DeleteFolder()
        {
            Directory.Delete(GetFolderPath());
        }
        //===============================================================
        public IQueryable<T> EnumerateObjects()
        {
            var path = GetFolderPath();
            if (!Directory.Exists(path))
                return new List<T>().AsQueryable();

            var files = Directory.EnumerateFiles(GetFolderPath());
            return files.Select(x => GetObject(x).Object).AsQueryable();
        }
        //===============================================================
    }
}
