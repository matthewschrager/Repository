using System;
using System.Linq;
using NUnit.Framework;
using Repository.Testing;

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

    [TestFixture]
    internal class Tests
    {
        //===============================================================
        private AzureRepository<TestClass> TestObjects(AzureOptions<TestClass> options = null)
        {
            return AzureRepository<TestClass>.ForStorageEmulator(x => x.ID, options);
        }
        //===============================================================
        private ExplicitKeyAzureRepository<TestClass> ExplicitKeyTestObjects(AzureOptions<TestClass> options = null)
        {
            return ExplicitKeyAzureRepository<TestClass>.ForStorageEmulator(options);
        }
        //===============================================================
        private ExplicitKeyAzureRepository<String, String> Strings(AzureOptions<String> options = null)
        {
            return ExplicitKeyAzureRepository<String, String>.ForStorageEmulator(options);
        }
        //===============================================================
        [Test]
        public void Standard()
        {
            StandardTests.All(TestObjects(), null, ExplicitKeyTestObjects());
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
        [Test]
        public void ExplicitKeyRepositoryGetObjectUri()
        {
            var repo = ExplicitKeyTestObjects();
            repo.GetObjectUri("testKey");
        }
        //===============================================================
    }
}
