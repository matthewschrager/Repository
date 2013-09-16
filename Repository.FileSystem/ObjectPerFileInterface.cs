using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FileSystem
{
    internal class ObjectPerFileInterface<T> : IFileSystemInterface<T>
    {
        //================================================================================
        private FileSystemOptions<T> FileSystemOptions { get; set; } 
        //================================================================================
        private ObjectPerFileOptions<T> Options { get; set; }
        //================================================================================
        public void StoreObject(T value, IEnumerable<object> keys)
        {
            var path = Options.GetObjectPath(keys);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var writer = new StreamWriter(FileSystemOptions.StreamGenerator.GetWriteStream(path)))
            {
                writer.Write(FileSystemOptions.Serializer.Serialize());
            }
        }
        //================================================================================
        public ObjectContext<T> GetObject(IEnumerable<object> keys)
        {
            throw new NotImplementedException();
        }
        //================================================================================
        public bool Exists(IEnumerable<object> keys)
        {
            throw new NotImplementedException();
        }
        //================================================================================
        public void DeleteObject(IEnumerable<object> keys)
        {
            throw new NotImplementedException();
        }
        //================================================================================
        public IQueryable<T> EnumerateObjects()
        {
            throw new NotImplementedException();
        }
        //================================================================================
    }

    internal class ObjectPerFileOptions<T>
    {
        //================================================================================
        public Func<IEnumerable<Object>, String> GetObjectPath { get; private set; }
        //================================================================================
    }
}
