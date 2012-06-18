using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Abstractions.Commands;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Json.Linq;

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
        private String KeyGenerator(IEnumerable<Object> keys)
        {
            var strings = new List<String>();
            foreach (var x in keys)
            {
                if (x.GetType().Equals(typeof(DateTime)))
                    strings.Add(((DateTime)x).Ticks.ToString());
                else
                    strings.Add(x.ToString());
            }

            // Replace any slashes with hyphens, because slashes create indexes
            strings = strings.Select(x => x.Replace('/', '-').Replace('\\', '-').Replace(' ', '-')).ToList();

            var key = DocumentStore.Conventions.GetTypeTagName(typeof(T)) + "/" + strings.Aggregate((x, y) => x + "/" + y);
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
        public void Store(IEnumerable<T> values)
        {
            try
            {
                using (var session = DocumentStore.OpenSession())
                {
                    foreach (var x in values)
                        session.Store(x);

                    session.SaveChanges();
                }
            }

            catch (Exception e)
            {
                throw new Exception("Could not store values in database. Error: " + e.Message, e);
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
            return new RavenObjectContext<T>(obj, session, x => KeyGenerator(KeySelector(x)));
        }
        //===============================================================
        public IEnumerableObjectContext<T> GetItemsContext()
        {
            var session = DocumentStore.OpenSession();
            var obj = session.Query<T>();
            return new RavenEnumerableObjectContext<T>(obj, session);
        }
        //===============================================================
        public void Update<TValue>(TValue value, params Object[] keys)
        {
            using (var obj = Find(keys))
            {
                obj.Update(value);
                obj.SaveChanges();
            }
        }
        //===============================================================
        public void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter, params Object[] keys)
        {
            using (var obj = Find(keys))
            {
                obj.Update(value, getter);
                obj.SaveChanges();
            }
        }
        //===============================================================
        public void UpdateFromJSON(String json, UpdateType updateType, params Object[] keys)
        {
            using (var obj = Find(keys) as RavenObjectContext<T>)
            {
                obj.UpdateFromJSON(json, updateType);
            }
        }
        //===============================================================
        public void UpdateFromJSON(String pathToProperty, String json, UpdateType updateType, params Object[] keys)
        {
            using (var obj = Find(keys) as RavenObjectContext<T>)
            {
                obj.UpdateFromJSON(pathToProperty, json, updateType);
            }
        }
        //===============================================================
    }

    public enum UpdateType
    {
        Add,
        Set,
    }

    public class RavenObjectContext<T> : IObjectContext<T> where T : class
    {
        //===============================================================
        public RavenObjectContext(T obj, IDocumentSession session, Func<T, String> keyGenerator)
        {
            Object = obj;
            Session = session;
            KeyGenerator = keyGenerator;
        }
        //===============================================================
        private Func<T, String> KeyGenerator { get; set; }
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
        public void Update<TValue>(TValue value)
        {
            Object = AutoMapper.Mapper.DynamicMap<T>(value);
        }
        //===============================================================
        public void Update<TValue, TProperty>(TValue value, Func<T, TProperty> getter)
        {
            AutoMapper.Mapper.DynamicMap(value, getter(Object));
        }
        //===============================================================
        private PatchCommandType ToPatchType(UpdateType updateType)
        {
            switch (updateType)
            {
                case UpdateType.Add:
                    return PatchCommandType.Add;
                
                case UpdateType.Set:
                default:
                    return PatchCommandType.Set;
            }
        }
        //===============================================================
        public void UpdateFromJSON(String json, UpdateType updateType = UpdateType.Set)
        {
            if (Object == null)
                return;

            var obj = JObject.Parse(json);
            var patches = obj.Properties().Where(x => Object.GetType().GetProperty(x.Name) != null)
                                          .Select(x => new PatchRequest { Type = ToPatchType(updateType), Name = x.Name, Value = RavenJToken.Parse(x.Value.ToString()) }).ToArray();

            Session.Advanced.DatabaseCommands.Patch(KeyGenerator(Object), patches);
        }
        //===============================================================    
        public void UpdateFromJSON(String pathToProperty, String json, UpdateType updateType = UpdateType.Set)
        {
            if (Object == null)
                return;

            if (String.IsNullOrEmpty(pathToProperty))
            {
                UpdateFromJSON(json);
                return;
            }

            var patchCommandType = ToPatchType(updateType);
            var propertyNames = pathToProperty.Split('.');
            var baseRequest = new PatchRequest
                               {
                                   Type = PatchCommandType.Modify,
                                   Name = propertyNames.First(),
                                   Nested = new PatchRequest[1],
                               };

            var currRequest = baseRequest;
            var currProperty = Object.GetType().GetProperty(propertyNames.First());
            foreach(var propertyName in propertyNames.Skip(1).Take(propertyNames.Length - 2))
            {
                if (currProperty == null)
                    return;

                currRequest.Nested[0] = new PatchRequest
                                        {
                                            Type = PatchCommandType.Modify,
                                            Name = propertyName,
                                            Nested = new PatchRequest[1],
                                        };

                currRequest = currRequest.Nested[0];
                currProperty = currProperty.PropertyType.GetProperty(propertyName);
            }

            var obj = JObject.Parse(json);
            var patches = obj.Properties().Where(x => currProperty.PropertyType.GetProperty(x.Name) != null)
                                          .Select(x => new PatchRequest { Type = patchCommandType, Name = x.Name, Value = RavenJToken.Parse(x.Value.ToString()) }).ToArray();
            currRequest.Nested = patches;

            Session.Advanced.DatabaseCommands.Patch(KeyGenerator(Object), new[] { baseRequest });
        }
        //===============================================================
    }

    public class RavenEnumerableObjectContext<T> : IEnumerableObjectContext<T> where T : class 
    {
        //===============================================================
        public RavenEnumerableObjectContext(IQueryable<T> objects, IDocumentSession session)
        {
            Objects = objects;
            Session = session;
        }
        //===============================================================
        private IDocumentSession Session { get; set; }
        //===============================================================
        public IQueryable<T> Objects { get; private set; }
        //===============================================================
        public void Dispose()
        {
            Session.Dispose();
        }
        //===============================================================
        public void SaveChanges()
        {
            Session.SaveChanges();
        }
        //===============================================================
    }
}
