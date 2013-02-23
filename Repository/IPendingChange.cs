using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPendingChange
    {
        //===============================================================
        void Apply();
        //===============================================================
    }

    public abstract class Insert<TValue> : IPendingChange
    {
        //===============================================================
        public Insert(IEnumerable<Object> keys, TValue value)
        {
            Keys = keys;
            Value = value;
        }
        //===============================================================
        public IEnumerable<Object> Keys { get; private set; }
        //===============================================================
        public TValue Value { get; private set; }
        //===============================================================
        public abstract void Apply();
        //===============================================================
    }

    public abstract class Remove : IPendingChange
    {
        //===============================================================
        public Remove(IEnumerable<Object> keys)
        {
            Keys = keys;
        }
        //===============================================================
        public IEnumerable<Object> Keys { get; private set; }
        //===============================================================
        public abstract void Apply();
        //===============================================================
    }

    public abstract class Modify : IPendingChange
    {
        //===============================================================
        public Modify(Action change)
        {
            Change = change;
        }
        //===============================================================
        public Action Change { get; private set; }
        //===============================================================
        public abstract void Apply();
        //===============================================================
    }

//    internal class Insert<T> : IPendingChange<T> where T : class
//    {
//        //===============================================================
//        public Insert(String key, T value)
//        {
//            Key = key;
//            Value = value;
//        }
//        //===============================================================
//        public String Key { get; private set; }
//        //===============================================================
//        public T Value { get; private set; }
//        //===============================================================
//        public void Apply()
//        {
//            data[Key] = Value;
//        }
//        //===============================================================
//    }
//
//    public class Remove<T> : IPendingChange<T> where T : class
//    {
//        //===============================================================
//        public Remove(String key)
//        {
//            Key = key;
//        }
//        //===============================================================
//        public String Key { get; private set; }
//        //===============================================================
//        public void Apply(IDictionary<String, T> data)
//        {
//            if (data.ContainsKey(Key))
//                data.Remove(Key);
//        }
//        //===============================================================
//    }
//
//    internal class Modify<T> : IPendingChange<T> where T : class
//    {
//        //===============================================================
//        public Modify(String key, Action<T> change)
//        {
//            Key = key;
//            Change = change;
//        }
//        //===============================================================
//        public String Key { get; private set; }
//        //===============================================================
//        public Action<T> Change { get; private set; }
//        //===============================================================
//        public void Apply(IDictionary<String, T> data)
//        {
//            Change(data[Key]);
//        }
//        //===============================================================
//    }
}
