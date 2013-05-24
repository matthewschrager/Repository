using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Azure.Serialization;

namespace Repository.Azure
{
    public class AzureOptions<T>
    {
        //===============================================================
        public AzureOptions()
        {
            Encoder = new EncodingEncoder<T>(Encoding.UTF8, new JsonSerializer<T>());
        }
        //===============================================================
        public Encoder<T> Encoder { get; set; } 
        //===============================================================
        public String ContainerName { get; set; }
        //===============================================================
        public String ContentType { get; set; }
        //===============================================================
    }
}
