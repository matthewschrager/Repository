using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Azure
{
    public interface ISerializer
    {
        //===============================================================
        String Serialize<T>(T obj);
        //===============================================================
        T Deserialize<T>(String str);
        //===============================================================
    }
}
