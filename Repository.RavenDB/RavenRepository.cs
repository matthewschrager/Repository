using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Abstractions.Commands;
using Raven.Abstractions.Data;
using Raven.Abstractions.Exceptions;
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
            TimeoutInMilliseconds = 5000;
            DocumentStore = documentStore;
            KeySelector = keySelector;

            DocumentStore.Conventions.DocumentKeyGenerator = o => KeyGenerator(KeySelector(o as T));
            DocumentStore.Initialize();
            DocumentStore.JsonRequestFactory.ConfigureRequest += (s, e) => e.Request.Timeout = TimeoutInMilliseconds;
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

            // Replace any illegal characters with hyphens
            strings = strings.Select(x => x.Replace('/', '-').Replace('\\', '-').Replace(' ', '-').Replace('^', '-')).ToList();

            var key = DocumentStore.Conventions.GetTypeTagName(typeof(T)) + "/" + strings.Aggregate((x, y) => x + "/" + y);
            return key;
        }
        //===============================================================
        public int TimeoutInMilliseconds { get; set; }
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
                    Guid? etag = null;
                    using (var existingObj = Find(KeySelector(value)) as RavenObjectContext<T>)
                    {
                        if (existingObj.Object != null)
                            etag = existingObj.Session.Advanced.GetEtagFor(existingObj.Object);
                    }

                    if (etag != null)
                        session.Store(value, etag.Value);
                    else
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
                    {
                        Guid? etag = null;
                        using (var existingObj = Find(KeySelector(x)) as RavenObjectContext<T>)
                        {
                            if (existingObj.Object != null)
                                etag = existingObj.Session.Advanced.GetEtagFor(existingObj.Object);
                        }


                        if (etag != null)
                            session.Store(x, etag.Value);
                        else
                            session.Store(x);
                    }

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
        public IDocumentSession Session { get; private set; }
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
            AutoMapper.Mapper.DynamicMap(value, Object);
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



    [TestFixture]
    public class RavenTests
    {
        private RavenRepository<TestClass> TestClasses;
        private RavenRepository<Fixture> Fixtures;
        private RavenRepository<WorkOrder> WorkOrders;
        private RavenRepository<StringKeyClass> StringKeys;

        //===============================================================
        [SetUp]
        public void SetUp()
        {
            TestClasses = RavenRepository<TestClass>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.Key);
            Fixtures = RavenRepository<Fixture>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.FixtureID);
            WorkOrders = RavenRepository<WorkOrder>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.ID);
            StringKeys = RavenRepository<StringKeyClass>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.Key);

            foreach (var obj in TestClasses.GetItemsContext().Objects)
                TestClasses.Remove(obj.Key);
            foreach (var obj in Fixtures.GetItemsContext().Objects)
                Fixtures.Remove(obj.FixtureID);
            foreach (var obj in WorkOrders.GetItemsContext().Objects)
                WorkOrders.Remove(obj.ID);
        }
        //===============================================================
        [TearDown]
        public void TearDown()
        {
            TestClasses.Dispose();
            Fixtures.Dispose();
            WorkOrders.Dispose();
            StringKeys.Dispose();
        }
        //===============================================================
        [Test]
        public void RetrieveCorrectObjects()
        {
            TestClasses.Store(new TestClass());

            using (var obj = TestClasses.Find(1))
            {
                Assert.NotNull(obj.Object);
            }

            using (var obj = TestClasses.Find(2))
            {
                Assert.Null(obj.Object);
            }

            StringKeys.Store(new StringKeyClass("asarda@umich.edu-blahblahblah"));

            using (var obj = StringKeys.Find("asarda@umich.edu"))
            {
                Assert.Null(obj.Object);
            }
        }
        //===============================================================
        [Test]
        public void UpdateTest()
        {
            var obj = new TestClass();
            var guid = Guid.NewGuid();
            TestClasses.Store(obj);

            using (var dbObj = TestClasses.Find(obj.Key))
            {
                Assert.NotNull(dbObj.Object);

                dbObj.Object.Guid = guid;
                dbObj.SaveChanges();
            }

            using (var dbObj = TestClasses.Find(obj.Key))
            {
                Assert.NotNull(dbObj.Object);
                Assert.AreEqual(guid, dbObj.Object.Guid);
            }

            guid = Guid.NewGuid();
            obj.Guid = guid;

            TestClasses.Update(obj, obj.Key);

            using (var dbObj = TestClasses.Find(obj.Key))
            {
                Assert.NotNull(dbObj.Object);
                Assert.AreEqual(guid, dbObj.Object.Guid);
            }
        }
        //===============================================================
        [Test]
        public void UpdateFromJSONTest()
        {
            // TestClass
            var obj = new TestClass();
            TestClasses.Store(obj);

            TestClasses.Update("Property", "{ Value1: 2 }", UpdateType.Set, 1);
            using (var dbObj = TestClasses.Find(1))
            {
                Assert.AreEqual(dbObj.Object.Property.Value1, 2);
            }

            TestClasses.Update("{ ComplexList: { Value1: 2 } }", UpdateType.Add, 1);
            using (var dbObj = TestClasses.Find(1))
            {
                Assert.AreEqual(dbObj.Object.ComplexList.Count, 1);
                Assert.AreEqual(dbObj.Object.ComplexList.First().Value1, 2);
            }

            TestClasses.Remove(obj.Key);


            // Fixture
            var fixture = new Fixture();
            fixture.FixtureID = Guid.Parse("c7ed2664-58ca-4324-82c1-96fa54140258");
            fixture.Data = new FixtureData();
            fixture.Data.Condition = FixtureCondition.New;

            Fixtures.Store(fixture);
            var date = DateTime.Now;
            Fixtures.Update("Data", "{ 'Comments': { 'Date': '/Date(1224043200000)/', 'Value': 'Test Comment' } }", UpdateType.Add, "c7ed2664-58ca-4324-82c1-96fa54140258");
            using (var dbObj = Fixtures.Find(fixture.FixtureID))
            {
                Assert.AreEqual(dbObj.Object.Data.Comments.Count, 1);
                Assert.AreEqual(dbObj.Object.Data.Condition, FixtureCondition.New);
            }

            Fixtures.Update("Data", "{ UnitID: 'c7ed2664-58ca-4324-82c1-96fa54140258' }", UpdateType.Set, "c7ed2664-58ca-4324-82c1-96fa54140258");
            using (var dbObj = Fixtures.Find("c7ed2664-58ca-4324-82c1-96fa54140258"))
            {
                Assert.AreEqual(dbObj.Object.Data.UnitID, Guid.Parse("c7ed2664-58ca-4324-82c1-96fa54140258"));
            }

            Fixtures.Remove(fixture.FixtureID);

            // Work orders
            var workOrder = new WorkOrder();
            workOrder.ID = Guid.Parse("2f835d08-34c0-406d-8188-0ce5f33325fc");
            workOrder.Data = new WorkOrderData
            {
                Name = "Couch"
            };

            WorkOrders.Store(workOrder);
            WorkOrders.Update("Data", "{ 'Comment': 'Test Comment' }", UpdateType.Set, workOrder.ID);
            using (var dbObj = WorkOrders.Find(workOrder.ID))
            {
                Assert.AreEqual(dbObj.Object.Data.Comment, "Test Comment");
            }

            WorkOrders.Remove(workOrder.ID);
        }
        //===============================================================
        [Test]
        public void InvalidUpdateTest()
        {
            var obj = new TestClass();
            TestClasses.Store(obj);

            // First-level data
            Assert.That(() => TestClasses.Update("{'Value2': 'blah'}", UpdateType.Set, obj.Key), Throws.Exception);

            // Second-level data
            Assert.That(() => TestClasses.Update("TestProperty", "{ 'Value2': 'blah' }", UpdateType.Set, obj.Key), Throws.Exception);

            // List data
            Assert.That(() => TestClasses.Update("{ 'ComplexList': 'blah' }", UpdateType.Add, obj.Key), Throws.Exception);

            // Incorrect update type
            Assert.That(() => TestClasses.Update("{ 'Value2': 'blah' }", UpdateType.Add, obj.Key), Throws.Exception);

            // Incorrect data type (list)
            Assert.That(() => TestClasses.Update("{ 'ComplexList': 'blah' }", UpdateType.Set, obj.Key), Throws.Exception);

            TestClasses.Remove(obj.Key);

        }
        //===============================================================
        [Test]
        public void FindFromJSONTest()
        {
            var workOrder = new WorkOrder();
            workOrder.ID = Guid.Parse("2f835d08-34c0-406d-8188-0ce5f33325fc");
            workOrder.Data = new WorkOrderData
            {
                Name = "Couch"
            };

            WorkOrders.Store(workOrder);

            var result = WorkOrders.FindFromJSON("{ 'ID': '2f835d08-34c0-406d-8188-0ce5f33325fc' }").ToList();
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Data.Name, "Couch");

            result = WorkOrders.FindFromJSON("{ 'ID': '2f835d08-34c0-406d-8188-0ce5f33325f3' }").ToList();
            Assert.AreEqual(result.Count, 0);

            WorkOrders.Remove(workOrder.ID);
        }
        //===============================================================
        [Test]
        public void UpdateGuidTest()
        {
            // First level
            var testObj = new TestClass();
            testObj.Guid = Guid.NewGuid();
            TestClasses.Store(testObj);

            var newGuid = Guid.NewGuid();
            TestClasses.Update("{ 'Guid': '" + newGuid + "' }", UpdateType.Set, testObj.Key);
            using (var dbObj = TestClasses.Find(testObj.Key))
            {
                Assert.AreEqual(newGuid, dbObj.Object.Guid);
            }

            TestClasses.Remove(testObj.Key);

            // Second level
            var fixture = new Fixture();
            fixture.FixtureID = Guid.Parse("2f835d08-34c0-406d-8188-0ce5f33325fc");
            fixture.Data = new FixtureData();

            Fixtures.Store(fixture);
            Fixtures.Update("Data", "{ UnitID: '" + newGuid + "' }", UpdateType.Set, fixture.FixtureID);


            using (var dbFixture = Fixtures.Find(fixture.FixtureID))
            {
                Assert.AreEqual(fixture.FixtureID, dbFixture.Object.FixtureID);
                Assert.AreEqual(newGuid, dbFixture.Object.Data.UnitID);
            }

            Fixtures.Remove(fixture.FixtureID);
        }
        //===============================================================
        [Test]
        public void UpdateListTest()
        {
            var obj = new TestClass();
            TestClasses.Store(obj);

            var list = new[] { 2 };
            TestClasses.Update("{ 'IntegerList': " + JsonConvert.SerializeObject(list) + " }", UpdateType.Set, obj.Key);
            using (var dbObj = TestClasses.Find(obj.Key))
            {
                Assert.NotNull(dbObj.Object);
                Assert.AreEqual(list.Count(), dbObj.Object.IntegerList.Count);
                Assert.IsTrue(list.SequenceEqual(dbObj.Object.IntegerList));
            }

            TestClasses.Remove(obj.Key);
        }
        //===============================================================
        [Test]
        public void SaveObjectTwice()
        {
            var obj = new TestClass();
            TestClasses.Store(obj);

            Thread.Sleep(500);

            obj = new TestClass();
            obj.Guid = Guid.NewGuid();

            TestClasses.Store(obj);
            using (var dbObj = TestClasses.Find(obj.Key))
            {
                Assert.NotNull(dbObj.Object);
                Assert.AreEqual(obj.Guid, dbObj.Object.Guid);
            }

            TestClasses.Remove(obj);
        }
        //===============================================================
        [Test]
        public void SaveObjectsTwice()
        {
            var obj1 = new TestClass { Key = 1 };
            var obj2 = new TestClass { Key = 2 };

            TestClasses.Store(new[] { obj1, obj2 });

            Thread.Sleep(500);

            obj1 = new TestClass { Key = 1, Guid = Guid.NewGuid() };
            obj2 = new TestClass { Key = 2, Guid = Guid.NewGuid() };

            TestClasses.Store(new[] { obj1, obj2 });
            using (var dbObj = TestClasses.Find(obj1.Key))
            {
                Assert.NotNull(dbObj.Object);
                Assert.AreEqual(obj1.Guid, dbObj.Object.Guid);
            }

            using (var dbObj = TestClasses.Find(obj2.Key))
            {
                Assert.NotNull(dbObj.Object);
                Assert.AreEqual(obj2.Guid, dbObj.Object.Guid);
            }

        }
    }
}
