using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Abstractions.Commands;
using Raven.Client;
using Raven.Client.Document;

namespace Repository
{
    public class RavenRepository<T> : IRepository<T> where T : class
    {
        //===============================================================
        public RavenRepository(String connectionStr, Func<T, Object[]> keySelector)
        {
            KeySelector = keySelector;
            KeyGenerator = e => DocumentStore.Conventions.GetTypeTagName(e.GetType()) + "/" + KeySelector(e as T).Select(x => x.ToString()).Aggregate((x, y) => x + "/" + y);
            ConnectionString = connectionStr;
            
            try
            {
                DocumentStore = new DocumentStore { Url = connectionStr };
                DocumentStore.Conventions.DocumentKeyGenerator = e => KeyGenerator(KeySelector(e as T));
                DocumentStore.Initialize();
            }

            catch (Exception e)
            {
                throw new Exception("Could not connect to Raven database. Error: " + e.Message, e);
            }
        }
        //===============================================================
        private Func<Object[], String> KeyGenerator { get; set; }
        //===============================================================
        private DocumentStore DocumentStore { get; set; }
        //===============================================================
        private String ConnectionString { get; set; }
        //===============================================================
        private Func<T, Object[]> KeySelector { get; set; }
        //===============================================================
        public void Store(T value)
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(value);
                session.SaveChanges();
            }
        }
        //===============================================================
        public void Remove(params object[] keys)
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Advanced.Defer(new DeleteCommandData { Key = KeyGenerator(keys) });
                session.SaveChanges();
            }
        }
        //===============================================================
        public IObjectContext<T> Find(params object[] keys)
        {
            var session = DocumentStore.OpenSession();
            var obj = session.Load<T>(KeyGenerator(keys));
            return new RavenObjectContext<T>(obj, session);
        }
        //===============================================================
        public IObjectContext<IQueryable<T>> GetItemsContext()
        {
            throw new NotImplementedException();
        }
        //===============================================================
    }

    public class RavenObjectContext<T> : IObjectContext<T> where T : class
    {
        //===============================================================
        public RavenObjectContext(T obj, IDocumentSession session)
        {
            Object = obj;
            Session = session;
        }
        //===============================================================
        private IDocumentSession Session { get; set; }
        //===============================================================
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Session.Dispose();
        }
        //===============================================================
        public T Object { get; private set; }
        //===============================================================
        public void SaveChanges()
        {
            Session.SaveChanges();
        }
        //===============================================================
    }
}
