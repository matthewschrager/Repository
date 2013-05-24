using System;

namespace Repository.Azure.Serialization
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
