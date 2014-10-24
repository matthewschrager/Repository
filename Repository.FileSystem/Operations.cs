using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FileSystem
{
    internal class FileSystemInsert<T> : Insert<T>
    {
        //===============================================================
        public FileSystemInsert(IEnumerable<object> keys, T value, FileSystemInterface<T> fsInterface)
            : base(keys, value)
        {
            FileSystemInterface = fsInterface;
        }
        //===============================================================
        private FileSystemInterface<T> FileSystemInterface { get; set; }
        //===============================================================
        public override void Apply()
        {
            FileSystemInterface.StoreObject(Value, Keys);
        }
        //===============================================================
    }

    internal class FileSystemBatchInsert<T> : BatchInsert<T>
    {
        //================================================================================
        public FileSystemBatchInsert(IEnumerable<KeyValuePair<IEnumerable<object>, T>> keyValuePairs, FileSystemInterface<T> fsInterface)
            : base(keyValuePairs)
        {
            FileSystemInterface = fsInterface;
        }
        //================================================================================
        private FileSystemInterface<T> FileSystemInterface { get; set; } 
        //================================================================================
        public override void Apply()
        {
            FileSystemInterface.StoreObjects(KeyValuePairs);
        }
        //================================================================================
    }

    internal class FileSystemRemove<T> : Remove
    {
        //===============================================================
        public FileSystemRemove(IEnumerable<object> keys, FileSystemInterface<T> fsInterface)
            : base(keys)
        {
            FileSystemInterface = fsInterface;
        }
        //===============================================================
        private FileSystemInterface<T> FileSystemInterface { get; set; }
        //===============================================================
        public override void Apply()
        {
            FileSystemInterface.DeleteObject(Keys);
        }
        //===============================================================
    }

    internal class FileSystemModify<T> : Modify<T>
    {
        //===============================================================
        public FileSystemModify(IEnumerable<object> keys, T value, Action<T> modifier, FileSystemInterface<T> fsInterface)
            : base(keys, value, modifier)
        {
            FileSystemInterface = fsInterface;
        }
        //===============================================================
        private FileSystemInterface<T> FileSystemInterface { get; set; }
        //===============================================================
        public override void Apply()
        {
            Modifier(Value);
            FileSystemInterface.StoreObject(Value, Keys);
        }
        //===============================================================
    }
}
