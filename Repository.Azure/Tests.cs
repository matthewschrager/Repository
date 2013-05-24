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
        public GenericTestObject(String key1, String key2, T value)
        {
            Key1 = key1;
            Key2 = key2;
            Value = value;
        }
        //===============================================================
        public String Key1 { get; set; }
        //===============================================================
        public String Key2 { get; set; }
        //===============================================================
        public T Value { get; set; }
        //===============================================================
    }

    [TestFixture]
    internal class Tests
    {
        private static readonly String EmulatorConnectionString = "DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
        
        //===============================================================
        private AzureRepository<TestObject, String> TestObjects()
        {
            return AzureRepository<TestObject, String>.CreateForStorageEmulator(x => x.ID);
        }
        //===============================================================
        private AzureRepository<GenericTestObject<int>, String, String> GenericTestObjects()
        {
            return AzureRepository<GenericTestObject<int>, String, String>.CreateForStorageEmulator(x => Tuple.Create(x.Key1, x.Key2));
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
            Assert.Null(storedObj.Object);

            // Try with generic object
            var genericObjects = GenericTestObjects();
            var genericObj = new GenericTestObject<int>("key1", "key2", 1);

            genericObjects.Insert(genericObj);
            genericObjects.SaveChanges();

            var storedGeneric = genericObjects.Find(genericObj.Key1, genericObj.Key2);
            Assert.NotNull(storedGeneric.Object);
            Assert.AreEqual(genericObj.Key1, storedGeneric.Object.Key1);
            Assert.AreEqual(genericObj.Key2, storedGeneric.Object.Key2);
            Assert.AreEqual(genericObj.Value, storedGeneric.Object.Value);

            genericObjects.Remove(genericObj);
            genericObjects.SaveChanges();

            storedGeneric = genericObjects.Find(genericObj.Key1, genericObj.Key2);
            Assert.Null(storedGeneric.Object);
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
    }
}
