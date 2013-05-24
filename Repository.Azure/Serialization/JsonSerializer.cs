using System;
using Newtonsoft.Json;

namespace Repository.Azure.Serialization
{
    public class JsonSerializer<T> : ISerializer<T>
    {
        //===============================================================
        public String Serialize(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        //===============================================================
        public T Deserialize(String str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }
        //===============================================================
    }
}
