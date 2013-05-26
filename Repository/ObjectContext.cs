using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using Repository.ChangeTracking;

namespace Repository
{
    public class ObjectContext<T>
    {
        //===============================================================
        public ObjectContext(T obj)
        {
            Object = obj;
        }
        //===============================================================
        public T Object { get; private set; }
        //===============================================================
    }

    public class EnumerableObjectContext<T> : IQueryable<T>
    {
        private IQueryProvider mCachedQueryProvider = null;

        //===============================================================
        public EnumerableObjectContext(IQueryable<T> objects, Repository<T> parentRepository)
        {
            Objects = objects;
            ParentRepository = parentRepository;
        }
        //===============================================================
        private Repository<T> ParentRepository { get; set; }
        //===============================================================
        private IQueryable<T> Objects { get; set; }
        //===============================================================
        public IEnumerator<T> GetEnumerator()
        {
            return new CallbackEnumerator<T>(Objects.GetEnumerator(), x => ParentRepository.AddChangeTracker(x));
        }
        //===============================================================
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        //===============================================================
        public TQueryable AsRaw<TQueryable>() where TQueryable : class
        {
            return Objects as TQueryable;
        }
        //===============================================================
        public Expression Expression 
        {
            get { return Objects.Expression; }
        }
        //===============================================================
        public Type ElementType
        {
            get { return Objects.ElementType; }
        }
        //===============================================================
        public IQueryProvider Provider
        {
            get
            {
                if (mCachedQueryProvider == null)
                    mCachedQueryProvider = Objects.Provider;

                return mCachedQueryProvider;
            }
        }
        //===============================================================
    }
}
