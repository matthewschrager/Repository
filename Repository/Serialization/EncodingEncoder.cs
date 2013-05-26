using System.Text;

namespace Repository.Serialization
{
    public class EncodingEncoder<T> : StringEncoder<T>
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
