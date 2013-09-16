using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IOperation
    {
        //===============================================================
        void Apply();
        //===============================================================
    }

    public abstract class Insert<TValue> : IOperation
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

    public abstract class Remove : IOperation
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

    public abstract class Modify<T> : IOperation
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

    public abstract class BatchInsert<TValue> : IOperation
    {
        //================================================================================
        public BatchInsert(IEnumerable<KeyValuePair<IEnumerable<Object>, TValue>> keyValuePairs)
        {
            KeyValuePairs = keyValuePairs;
        }
        //================================================================================
        public IEnumerable<KeyValuePair<IEnumerable<Object>, TValue>> KeyValuePairs { get; private set; } 
        //================================================================================
        public abstract void Apply();
        //================================================================================
    }

    public class DefaultBatchInsert<TValue> : BatchInsert<TValue>
    {
        //================================================================================
        public DefaultBatchInsert(IEnumerable<KeyValuePair<IEnumerable<Object>, TValue>> keyValuePairs, Func<IEnumerable<Object>, TValue, Insert<TValue>> singleInsertGenerator)
            : base(keyValuePairs)
        {
            SingleInsertGenerator = singleInsertGenerator;
        }
        //================================================================================
        public Func<IEnumerable<Object>, TValue, Insert<TValue>> SingleInsertGenerator { get; private set; } 
        //================================================================================
        public override void Apply()
        {
            var inserts = KeyValuePairs.Select(x => SingleInsertGenerator(x.Key, x.Value)).ToList();
            foreach (var insert in inserts)
                insert.Apply();
        }
        //================================================================================
    }
}
