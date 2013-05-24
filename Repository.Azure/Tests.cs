using System;
using System.Linq;
using NUnit.Framework;

namespace Repository.Azure
{
    [TestFixture]
    internal class AzureUtilityTests
    {
        private class TestObject<T>
        {
            public T Value = default(T);
        }

        //===============================================================
        [Test]
        public void GetSanitizedContainerName()
        {
            // Ensure no conflicts
            var firstName = AzureUtility.GetSanitizedContainerName<TestObject<int>>();
            var secondName = AzureUtility.GetSanitizedContainerName<TestObject<double>>();

            Assert.AreNotEqual(firstName, secondName);
        }
        //===============================================================
    }

    internal class TestObject
    {
        //===============================================================
        public TestObject(String id, String value)
        {
            ID = id;
            Value = value;
        }
        //===============================================================
        public String ID { get; set; }
        //===============================================================
        public String Value { get; set; }
        //===============================================================
    }

    internal class GenericTestObject<T>
    {
        //===============================================================
        public GenericTestObject(String key1, T value)
        {
            Key1 = key1;
            Value = value;
        }
        //===============================================================
        public String Key1 { get; set; }
        //===============================================================
        public T Value { get; set; }
        //===============================================================
    }

    [TestFixture]
    internal class Tests
    {
        //===============================================================
        private AzureRepository<TestObject, String> TestObjects(AzureOptions<TestObject> options = null)
        {
            return AzureRepository<TestObject, String>.CreateForStorageEmulator(x => x.ID, options);
        }
        //===============================================================
        private AzureRepository<GenericTestObject<int>, String> GenericTestObjects(AzureOptions<GenericTestObject<int>>  options = null)
        {
            return AzureRepository<GenericTestObject<int>, String>.CreateForStorageEmulator(x => x.Key1, options);
        }
        //===============================================================
        private ExplicitKeyAzureRepository<TestObject, String> ExplicitKeyTestObjects(AzureOptions<TestObject> options = null)
        {
            return ExplicitKeyAzureRepository<TestObject, String>.CreateForStorageEmulator(options);
        }
        //===============================================================
        private ExplicitKeyAzureRepository<String, String> Strings(AzureOptions<String> options = null)
        {
            return ExplicitKeyAzureRepository<String, String>.CreateForStorageEmulator(options);
        }
        //===============================================================
        [Test]
        public void InsertAndRemove()
        {
            var testObjects = TestObjects();
            var testObj = new TestObject("myKey", "myValue");
            
            testObjects.Insert(testObj);
            testObjects.SaveChanges();

            var storedObj = testObjects.Find(testObj.ID);
            Assert.NotNull(storedObj.Object);
            Assert.AreEqual(testObj.ID, storedObj.Object.ID);
            Assert.AreEqual(testObj.Value, storedObj.Object.Value);

            testObjects.Remove(testObj);
            testObjects.SaveChanges();

            storedObj = testObjects.Find(testObj.ID);
            Assert.Null(storedObj);

            // Try with generic object
            var genericObjects = GenericTestObjects();
            var genericObj = new GenericTestObject<int>("key1", 1);

            genericObjects.Insert(genericObj);
            genericObjects.SaveChanges();

            var storedGeneric = genericObjects.Find(genericObj.Key1);
            Assert.NotNull(storedGeneric.Object);
            Assert.AreEqual(genericObj.Key1, storedGeneric.Object.Key1);
            Assert.AreEqual(genericObj.Value, storedGeneric.Object.Value);

            genericObjects.Remove(genericObj);
            genericObjects.SaveChanges();

            storedGeneric = genericObjects.Find(genericObj.Key1);
            Assert.Null(storedGeneric);
        }
        //===============================================================
        [Test]
        public void Items()
        {
            var repo = TestObjects();
            var testObjects = Enumerable.Range(0, 10).Select(x => new TestObject(x.ToString(), x.ToString()));

            repo.Insert(testObjects);
            repo.SaveChanges();

            var items = repo.Items.ToList().OrderBy(x => int.Parse(x.Value)).ToList();
            Assert.AreEqual(10, items.Count);
            for (int i = 0; i < 10; ++i)
                Assert.AreEqual(i.ToString(), items[i].Value);

            repo.RemoveAll();
            repo.SaveChanges();

            Assert.AreEqual(0, repo.Items.Count());
        }
        //===============================================================
        [Test]
        public void ExplicitKeyRepository()
        {
            const string objectKey = "wrongKey";
            const string explicitKey = "rightKey";
            
            var repo = ExplicitKeyTestObjects();
            var testObject = new TestObject(objectKey, "myValue");

            repo.Insert(testObject, explicitKey);
            repo.SaveChanges();

            var savedObj = repo.Find(objectKey);
            Assert.Null(savedObj);

            savedObj = repo.Find(explicitKey);
            Assert.NotNull(savedObj);
            Assert.AreEqual(testObject.ID, savedObj.Object.ID);
            Assert.AreEqual(testObject.Value, savedObj.Object.Value);

            repo.RemoveByKey(explicitKey);
            repo.SaveChanges();

            savedObj = repo.Find(explicitKey);
            Assert.Null(savedObj);
        }
        //===============================================================
        [Test]
        public void NonComplexObject()
        {
            const string key = "myKey";
            const string value = "myValue";
            
            var repo = Strings();
            repo.Insert(value, key);
            repo.SaveChanges();

            var storedString = repo.Find(key);
            Assert.NotNull(storedString);
            Assert.AreEqual(value, storedString.Object);

            repo.RemoveByKey(key);
            repo.SaveChanges();

            storedString = repo.Find(key);
            Assert.Null(storedString);
        }
        //===============================================================
    }
}
