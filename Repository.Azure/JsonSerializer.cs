using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Repository.Azure
{
    public class JsonSerializer : ISerializer
    {
        //===============================================================
        public String Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        //===============================================================
        public T Deserialize<T>(String str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }
        //===============================================================
    }
}
