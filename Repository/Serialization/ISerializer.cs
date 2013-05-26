using System;

namespace Repository.Serialization
{
    public interface ISerializer<T>
    {
        //===============================================================
        String Serialize(T obj);
        //===============================================================
        T Deserialize(String str);
        //===============================================================
    }
}
