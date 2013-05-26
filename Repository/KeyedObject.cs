using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class KeyedObject<T>
    {
        //===============================================================
        public KeyedObject(object[] keys, T obj)
        {
            Keys = keys;
            Object = obj;
        }
        //===============================================================
        public object[] Keys { get; private set; }
        //===============================================================
        public T Object { get; private set; }
        //===============================================================
    }

    public class KeyedObject<TKey, TValue>
    {
        //===============================================================
        public KeyedObject(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
        //===============================================================
        public TKey Key { get; private set; }
        //===============================================================
        public TValue Value { get; private set; }
        //===============================================================
    }

    public class KeyedObject<TKey1, TKey2, TValue>
    {
        //===============================================================
        public KeyedObject(TKey1 key1, TKey2 key2, TValue value)
        {
            Key1 = key1;
            Key2 = key2;
            Value = value;
        }
        //===============================================================
        public TKey1 Key1 { get; private set; }
        //===============================================================
        public TKey2 Key2 { get; private set; }
        //===============================================================
        public TValue Value { get; private set; }
        //===============================================================
    }
}
