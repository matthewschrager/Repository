using System;
using NUnit.Framework;

namespace Repository.Testing
{
        internal class InMemoryRepositoryTests
        {
            //================================================================================
            [Test]
            public void Standard()
            {
                var repository = new InMemoryRepository<TestClass>(x => x.ID);
                StandardTests.All(repository);
            }
            //===============================================================
            [Test]
            public void UpdateTest()
            {
                var repository = new InMemoryRepository<ComplexTestClass>(x => x.ID);
                var obj = new ComplexTestClass() { IntValue = 1 };
                repository.Insert(obj);
                repository.SaveChanges();

                repository.Update(new { IntValue = 2 }, obj.ID);
                repository.SaveChanges();

                var val = repository.Find(obj.ID).Object;
                Assert.AreEqual(2, val.IntValue);

                var updateObj = new { DateTimeValue = DateTime.MaxValue };
                repository.Update(updateObj, x => x.ComplexProperty, obj.ID);
                repository.SaveChanges();

                Assert.AreEqual(val.ComplexProperty.DateTimeValue, DateTime.MaxValue);
            }
            //===============================================================
            [Test]
            public void ChangeTrackingTest()
            {
                var repo = new InMemoryRepository<TestClass>(x => x.ID);
                var initialValue = "myValue";
                var obj = new TestClass { StringValue = initialValue };

                repo.Insert(obj);
                repo.SaveChanges();

                var storedObj = repo.Find(obj.ID);
                storedObj.Object.StringValue = initialValue + 1;
                repo.SaveChanges();

                storedObj = repo.Find(obj.ID);
                Assert.AreEqual(initialValue + 1, storedObj.Object.StringValue);
            }
            //===============================================================
            [Test]
            public void DetectsChangedKeys()
            {
                var repo = new InMemoryRepository<TestClass>(x => x.ID);
                var initialValue = "myValue";
                var obj = new TestClass { ID = "myKey", StringValue = initialValue };

                repo.Insert(obj);
                repo.SaveChanges();

                var storedObj = repo.Find(obj.ID);
                storedObj.Object.ID = "newKey";

                Assert.Throws<RepositoryException>(repo.SaveChanges);
            }
            //===============================================================
        }
}
