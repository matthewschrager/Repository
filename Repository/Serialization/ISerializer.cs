using System;
using System.IO;

namespace Repository.Serialization
{
    public interface ISerializer<T>
    {
        //===============================================================
        void Serialize(T obj, Stream stream);
        //===============================================================
        T Deserialize(Stream stream);
        //===============================================================
    }
}
