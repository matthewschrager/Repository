using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Commands;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Linq;
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

            DocumentStore.Conventions.DocumentKeyGenerator = (e, o) => KeyGenerator(KeySelector(o as T));
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
        public void Update(String json, UpdateType updateType, params Object[] keys)
        {
            using (var obj = Find(keys) as RavenObjectContext<T>)
            {
                obj.Update(json, updateType);
            }
        }
        //===============================================================
        public void Update(String pathToProperty, String json, UpdateType updateType, params Object[] keys)
        {
            using (var obj = Find(keys) as RavenObjectContext<T>)
            {
                obj.Update(pathToProperty, json, updateType);
            }
        }
        //===============================================================
        public IEnumerable<T> FindFromJSON(String json)
        {
            using (var session = DocumentStore.OpenSession())
            {
                return RunLuceneQuery(json, session);
            }
//            var queryStr = JSONToLucene(json);
//            var query = new IndexQuery();
//            query.Query = queryStr;
//
//            var indexName = "dynamic/" + typeof(T).Name + "s";
//            using (var session = DocumentStore.OpenSession())
//            {
//                session.Advanced.LuceneQuery<T>(indexName).
//                var result = session.Advanced.DatabaseCommands.Query(indexName, query, new string[0]);
//                return result.Results.Select(x => JsonConvert.DeserializeObject<T>(x.ToString()));
//            }
        }
        //===============================================================
        private IEnumerable<T> RunLuceneQuery(String json, IDocumentSession session)
        {
            var obj = JObject.Parse(json);
            var indexName = "dynamic/" + typeof(T).Name + "s";
            var query = session.Advanced.LuceneQuery<T>(indexName);

            foreach (var p in obj.Properties())
                query = query.WhereEquals(p.Name, p.Value);

            return query;
        }
        //===============================================================
        private String JSONToLucene(String json)
        {
            var query = "(";
            var obj = JObject.Parse(json);
            foreach (var p in obj.Properties())
                query += p.Name + ": \"" + p.Value + "\" AND ";

            // Remove the trailing AND
            query = query.Remove(query.LastIndexOf(" AND"));

            // Add the closing parenthesis
            query += " )";

            return query;
        }
        //===============================================================

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            DocumentStore.Dispose();
        }
        //===============================================================
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
        private bool IsValidJSON(String json, Type type)
        {
            // If the JSON is a "raw" object, surround it with quotes so the deserializer sees it as a string
            if (!json.Trim().StartsWith("{") && !json.Trim().StartsWith("["))
                json = "'" + json + "'";

            try
            {
                // Nullables don't deserialize correctly for some reason, so we check the underlying type
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments().Length > 0)
                    type = type.GetGenericArguments()[0];

                JsonConvert.DeserializeObject(json, type);
                return true;
            }

            catch (Exception e)
            {
                return false;
            }
        }
        //===============================================================
        public bool IsValidUpdate(String json, UpdateType updateType, Type updateTargetType, ref String error)
        {
            var obj = JObject.Parse(json);
            foreach (var property in obj.Properties())
            {
                // JSON properties that don't have any equivalents in the real object are fine, because we'll just ignore them
                var realProperty = updateTargetType.GetProperty(property.Name);
                if (realProperty == null)
                    continue;

                // If this is a SET update, see if the JSON string can be deserialized into the property type
                if (updateType == UpdateType.Set)
                {
                    if (IsValidJSON(property.Value.ToString(), realProperty.PropertyType))
                        return true;

                    error = "Could not deserialize JSON string into type " + realProperty.PropertyType + ". Received JSON: " + property.Value;
                    return false;
                }

                // Otherwise, this is an ADD update. First make sure the real property type is enumerable
                if (!typeof(IEnumerable).IsAssignableFrom(realProperty.PropertyType))
                {
                    error = "The requested update was of type ADD, but the targeted property '" + realProperty.Name + "' was not assignable to IEnumerable. ADD updates can only " +
                            "be applied to properties that derived from IEnumerable (lists, arrays, etc.).";
                    return false;
                }

                // Next, make sure that the type of the list can be deserialized from the given JSON. For now, this only works with "simple"
                // enumerables, list List<T>
                if (!realProperty.PropertyType.IsGenericType || realProperty.PropertyType.GetGenericArguments().Length > 1)
                {
                    error = "The requested update was of type ADD, but the targeted property '" + realProperty.Name + "' was not assignable to IEnumerable<T>. ADD updates can only " +
                            " be applied to properties that derive from IEnumerable<T> for some type T, such as List<T> or Array<T>";
                    return false;
                }

                var genericType = realProperty.PropertyType.GetGenericArguments()[0];
                if (!IsValidJSON(property.Value.ToString(), genericType))
                {
                    error = "Could not deserialize JSON string into type " + genericType + ". Received JSON: " + property.Value;
                    return false;
                }
            }

            return true;
        }
        //===============================================================
        public void Update(String json, UpdateType updateType = UpdateType.Set)
        {
            if (Object == null)
                return;

            var error = "";
            if (!IsValidUpdate(json, updateType, typeof(T), ref error))
                throw new ArgumentException(error, "json");

            var obj = JObject.Parse(json);
            var patches = obj.Properties().Where(x => Object.GetType().GetProperty(x.Name) != null)
                                          .Select(x => new PatchRequest { Type = ToPatchType(updateType), Name = x.Name, Value = RavenJValue.FromObject(x.Value) }).ToArray();

            Session.Advanced.Defer(new PatchCommandData { Key = KeyGenerator(Object), Patches = patches });
            Session.SaveChanges();
        }
        //===============================================================    
        public void Update(String pathToProperty, String json, UpdateType updateType = UpdateType.Set)
        {
            if (Object == null)
                return;

            if (String.IsNullOrEmpty(pathToProperty))
            {
                Update(json, updateType);
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

            var error = "";
            if (!IsValidUpdate(json, updateType, currProperty.PropertyType, ref error))
                throw new ArgumentException(error, "json");

            var obj = JObject.Parse(json);
            var patches = obj.Properties().Where(x => currProperty.PropertyType.GetProperty(x.Name) != null)
                                          .Select(x => new PatchRequest 
                                          { 
                                              Type = patchCommandType, 
                                              Name = x.Name, 
                                              Value = RavenJValue.FromObject(x.Value)
                                          }).ToArray();
            currRequest.Nested = patches;

            Session.Advanced.Defer(new PatchCommandData { Key = KeyGenerator(Object), Patches = new[] { baseRequest } });
            Session.SaveChanges();
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
