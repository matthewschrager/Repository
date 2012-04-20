using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Commands;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace Repository.RavenDB
{
    public class RavenRepository<T> : IRepository<T> where T : class
    {
        //===============================================================
        private RavenRepository(DocumentStore documentStore, Func<T, Object[]> keySelector)
        {
            DocumentStore = documentStore;
            KeySelector = keySelector;

            DocumentStore.Conventions.DocumentKeyGenerator = e => KeyGenerator(KeySelector(e as T));
            DocumentStore.Initialize();
        }
        //===============================================================
        public static RavenRepository<T> FromUrlAndApiKey(String url, String apiKey, Func<T, Object[]> keySelector)
        {
            return new RavenRepository<T>(new DocumentStore { Url = url, ApiKey = apiKey }, keySelector);
        }
        //===============================================================
        public static RavenRepository<T> FromUrlAndApiKey(String url, String apiKey, Func<T, Object> keySelector)
        {
            return FromUrlAndApiKey(url, apiKey, x => new[] { keySelector(x) });
        }
        //===============================================================
        public static RavenRepository<T> FromNamedConnectionString(String connectionStringName, Func<T, Object[]> keySelector)
        {
            return new RavenRepository<T>(new DocumentStore { ConnectionStringName = connectionStringName }, keySelector);
        }
        //===============================================================
        public static RavenRepository<T> FromNamedConnectionString(String connectionStringName, Func<T, Object> keySelector)
        {
            return FromNamedConnectionString(connectionStringName, x => new[] { keySelector(x) });
        }
        //===============================================================
        public static RavenRepository<T> AsEmbeddedDocumentStore(String dataDirectory, Func<T, Object[]> keySelector)
        {
            return new RavenRepository<T>(new EmbeddableDocumentStore { DataDirectory = dataDirectory }, keySelector);
        }
        //===============================================================
        public static RavenRepository<T> AsEmbeddedDocumentStore(String dataDirectory, Func<T, Object> keySelector)
        {
            return AsEmbeddedDocumentStore(dataDirectory, x => new[] { keySelector(x) });
        }
        //===============================================================
        private String KeyGenerator(IEnumerable<Object> keys)
        {
            var key = DocumentStore.Conventions.GetTypeTagName(typeof(T)) + "/" + keys.Select(x => x.ToString()).Aggregate((x, y) => x + "/" + y);
            return key;
        }
        //===============================================================
        private DocumentStore DocumentStore { get; set; }
        //===============================================================
        private Func<T, Object[]> KeySelector { get; set; }
        //===============================================================
        public void Store(T value)
        {
            try
            {
                using (var session = DocumentStore.OpenSession())
                {
                    session.Store(value);
                    session.SaveChanges();
                }
            }

            catch (Exception e)
            {
                throw new Exception("Could not store value in database. Error: " + e.Message, e);
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
        public bool Exists(params Object[] keys)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var obj = session.Load<T>(KeyGenerator(keys));
                return obj != null;
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
            var session = DocumentStore.OpenSession();
            var obj = session.Query<T>();
            return new RavenObjectContext<IQueryable<T>>(obj, session);
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
