using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Repository.Serialization
{
    public class JsonSerializer<T> : ISerializer<T>
    {
        //===============================================================
        public void Serialize(T obj, Stream stream)
        {
            var writer = new StreamWriter(stream);
            writer.Write(JsonConvert.SerializeObject(obj));
            writer.Flush();
        }
        //===============================================================
        public T Deserialize(Stream stream)
        {
            return JsonConvert.DeserializeObject<T>(new StreamReader(stream).ReadToEnd());
        }
        //===============================================================
    }

}
