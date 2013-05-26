using System;
using System.IO;

namespace Repository.Serialization
{
    public abstract class Encoder<T>
    {
        //===============================================================
        public abstract byte[] Encode(T obj);
        //===============================================================
        public abstract T Decode(byte[] bytes);
        //===============================================================
    }

    public abstract class StringEncoder<T> : Encoder<T>
    {
        //===============================================================
        protected StringEncoder(ISerializer<T> serializer)
        {
            Serializer = serializer;
        }
        //===============================================================
        private ISerializer<T> Serializer { get; set; }
        //===============================================================
        public override byte[] Encode(T obj)
        {
            return EncodeString(Serializer.Serialize(obj));
        }
        //===============================================================
        public override T Decode(byte[] bytes)
        {
            return Serializer.Deserialize(DecodeString(bytes));
        }
        //===============================================================
        protected abstract byte[] EncodeString(String str);
        //===============================================================
        protected abstract String DecodeString(byte[] bytes);
        //===============================================================
    }
}
