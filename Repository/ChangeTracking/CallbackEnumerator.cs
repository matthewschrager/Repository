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
            try
            {
                return InnerEnumerator.MoveNext();
            }

            catch (Exception e)
            {
                throw new RepositoryException("Could not enumerate repository objects.", e);
            }
        }
        //===============================================================
        public void Reset()
        {
            try
            {
                InnerEnumerator.Reset();
            }

            catch (Exception e)
            {
                throw new RepositoryException("Could not reset repository items enumerator.", e);
            }
        }
        //===============================================================
        public T Current
        {
            get
            {
                try
                {
                    OnEnumerateObject(InnerEnumerator.Current);
                    return InnerEnumerator.Current;
                }

                catch (Exception e)
                {
                    throw new RepositoryException("Could not enumerate repository objects.", e);
                }
            }
        }
        //===============================================================
        object IEnumerator.Current
        {
            get
            {
                try
                {
                    return Current;
                }

                catch (Exception e)
                {
                    throw new RepositoryException("Could not enumerate repository objects.", e);
                }
            }
        }
        //===============================================================
    }
}
