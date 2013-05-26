using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface Operation
    {
        //===============================================================
        void Apply();
        //===============================================================
    }

    public abstract class Insert<TValue> : Operation
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

    public abstract class Remove : Operation
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

    public abstract class Modify<T> : Operation
    {
        //===============================================================
        public Modify(IEnumerable<Object> keys, T value, Action<T> modifier)
        {
            Keys = keys;
            Modifier = modifier;
            Value = value;
        }
        //===============================================================
        public IEnumerable<Object> Keys { get; private set; }
        //===============================================================
        public T Value { get; private set; }
        //===============================================================
        public Action<T> Modifier { get; private set; }
        //===============================================================
        public abstract void Apply();
        //===============================================================
    }
}
