using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class Session<TContext> : IDisposable
    {
        //===============================================================
        public Session(TContext context)
        {
            Context = context;
        }
        //===============================================================
        public TContext Context { get; private set; }
        //===============================================================
        public Repository<T> Add<T>(Func<TContext, Repository<T>> factory) where T : class
        {
            var repo = factory(Context);
            return repo;
        }
        //===============================================================
        public void Dispose()
        {
            var disposable = Context as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
        //===============================================================
    }
}
