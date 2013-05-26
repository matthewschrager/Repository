using System;
using System.Collections;
using System.Collections.Generic;

namespace Repository.ChangeTracking
{
    public class CallbackEnumerator<T> : IEnumerator<T>
    {
        //===============================================================
        public CallbackEnumerator(IEnumerator<T> innerEnumerator, Action<T> onEnumerateObject)
        {
            InnerEnumerator = innerEnumerator;
            OnEnumerateObject = onEnumerateObject;
        }
        //===============================================================
        private Action<T> OnEnumerateObject { get; set; }
        //===============================================================
        private IEnumerator<T> InnerEnumerator { get; set; }
        //===============================================================
        public void Dispose()
        {
            InnerEnumerator.Dispose();
        }
        //===============================================================
        public bool MoveNext()
        {
            return InnerEnumerator.MoveNext();
        }
        //===============================================================
        public void Reset()
        {
            InnerEnumerator.Reset();
        }
        //===============================================================
        public T Current
        {
            get
            {
                OnEnumerateObject(InnerEnumerator.Current);
                return InnerEnumerator.Current;
            }
        }
        //===============================================================
        object IEnumerator.Current
        {
            get { return Current; }
        }
        //===============================================================
    }
}
