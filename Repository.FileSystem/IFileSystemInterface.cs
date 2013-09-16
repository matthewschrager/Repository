using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.FileSystem
{
    internal interface IFileSystemInterface<T>
    {
        //================================================================================
        void StoreObject(T value, IEnumerable<Object> keys);
        //================================================================================
        ObjectContext<T> GetObject(IEnumerable<Object> keys);
        //================================================================================
        bool Exists(IEnumerable<Object> keys);
        //================================================================================
        void DeleteObject(IEnumerable<Object> keys);
        //================================================================================
        IQueryable<T> EnumerateObjects();
        //================================================================================
    }
}
