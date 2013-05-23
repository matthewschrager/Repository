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

    public abstract class Modify<TValue> : IPendingChange
    {
        //===============================================================
        public Modify(TValue value, Action<TValue> modifier)
        {
            Modifier = modifier;
            Value = value;
        }
        //===============================================================
        public TValue Value { get; private set; }
        //===============================================================
        public Action<TValue> Modifier { get; private set; }
        //===============================================================
        public abstract void Apply();
        //===============================================================
    }
}
