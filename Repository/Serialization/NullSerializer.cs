using System;

namespace Repository.Serialization
{
    public class NullSerializer : ISerializer<String>
    {
        //===============================================================
        public String Serialize(String str)
        {
            return str;
        }
        //===============================================================
        public String Deserialize(String str)
        {
            return str;
        }
        //===============================================================
    }
}
