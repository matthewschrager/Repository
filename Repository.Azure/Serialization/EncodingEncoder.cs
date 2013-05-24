using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Azure.Serialization
{
    public class EncodingEncoder<T> : Encoder<T>
    {
        //===============================================================
        public EncodingEncoder(Encoding encoding, ISerializer<T> serializer)
            : base(serializer)
        {
            Encoding = encoding;
        }
        //===============================================================
        public Encoding Encoding { get; private set; }
        //===============================================================
        protected override byte[] EncodeString(string str)
        {
            return Encoding.GetBytes(str);
        }
        //===============================================================
        protected override string DecodeString(byte[] bytes)
        {
            return Encoding.GetString(bytes);
        }
        //===============================================================
    }
}
