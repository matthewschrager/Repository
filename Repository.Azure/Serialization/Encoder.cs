using System;

namespace Repository.Azure.Serialization
{
    public abstract class Encoder<T>
    {
        //===============================================================
        protected Encoder(ISerializer<T> serializer)
        {
            Serializer = serializer;
        }
        //===============================================================
        public ISerializer<T> Serializer { get; private set; }
        //===============================================================
        public byte[] Encode(T obj)
        {
            return EncodeString(Serializer.Serialize(obj));
        }
        //===============================================================
        public T Decode(byte[] bytes)
        {
            return Serializer.Deserialize(DecodeString(bytes));
        }
        //===============================================================
        protected abstract byte[] EncodeString(String str);
        //===============================================================
        protected abstract String DecodeString(byte[] bytes);
        //===============================================================
    }

    public abstract class StringEncoder : Encoder<String>
    {
        //===============================================================
        protected StringEncoder()
            : base(new NullSerializer())
        {}
        //===============================================================
    }
}
