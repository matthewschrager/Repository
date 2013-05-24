using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Azure
{
    public class AzureOptions
    {
        //===============================================================
        public AzureOptions()
        {
            Serializer = new JsonSerializer();
            Encoding = Encoding.UTF8;
        }
        //===============================================================
        public ISerializer Serializer { get; set; } 
        //===============================================================
        public String ContainerName { get; set; }
        //===============================================================
        public String ContentType { get; set; }
        //===============================================================
        public Encoding Encoding { get; set; }
        //===============================================================
    }
}
