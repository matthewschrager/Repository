using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Repository.Testing
{
    public static class StandardTests
    {
        //===============================================================
        public static void InsertAndRemove(Repository<TestClass> testObjects)
        {
            var testObj = new TestClass("myKey", "myValue");

            testObjects.Insert(testObj);
            testObjects.SaveChanges();

            var storedObj = testObjects.Find(testObj.ID);
            Assert.NotNull(storedObj.Object);
            Assert.AreEqual(testObj.ID, storedObj.Object.ID);
            Assert.AreEqual(testObj.ID, storedObj.Object.ID);

            testObjects.Remove(testObj);
            testObjects.SaveChanges();

            storedObj = testObjects.Find(testObj.ID);
            Assert.Null(storedObj);

            testObjects.RemoveAll();
            testObjects.SaveChanges();
        }
        //===============================================================
        public static void Items(Repository<TestClass> repo)
        {
            var testObjects = Enumerable.Range(0, 10).Select(x => new TestClass(x.ToString(), x.ToString()));

            repo.Insert(testObjects);
            repo.SaveChanges();

            var items = repo.Items.ToList().OrderBy(x => x.StringValue).ToList();
            Assert.AreEqual(10, items.Count);
            for (int i = 0; i < 10; ++i)
                Assert.AreEqual(i.ToString(), items[i].StringValue);

            repo.RemoveAll();
            repo.SaveChanges();

            Assert.AreEqual(0, repo.Items.Count());
        }
        //===============================================================
        public static void ExplicitKeyRepository(ExplicitKeyRepository<TestClass> repo)
        {
            const string objectKey = "wrongKey";
            const string explicitKey = "rightKey";

            var testObject = new TestClass(objectKey, "myValue");

            repo.Insert(testObject, explicitKey);
            repo.SaveChanges();

            var savedObj = repo.Find(objectKey);
            Assert.Null(savedObj);

            savedObj = repo.Find(explicitKey);
            Assert.NotNull(savedObj);
            Assert.AreEqual(testObject.ID, savedObj.Object.ID);
            Assert.AreEqual(testObject.StringValue, savedObj.Object.StringValue);

            repo.RemoveByKey(explicitKey);
            repo.SaveChanges();

            savedObj = repo.Find(explicitKey);
            Assert.Null(savedObj);
        }
        //===============================================================
        public static void ChangeTracking(Repository<TestClass> repo)
        {
            var obj = new TestClass("myKey", "myValue");

            repo.Insert(obj);
            repo.SaveChanges();

            var storedObj = repo.Find(obj.ID);
            storedObj.Object.StringValue = "newValue";
            repo.SaveChanges();

            storedObj = repo.Find(obj.ID);
            Assert.AreEqual("newValue", storedObj.Object.StringValue);

            // Test Update function
            repo.Update(new { StringValue = "newValue2" }, storedObj.Object.ID);
            repo.SaveChanges();

            storedObj = repo.Find(obj.ID);
            Assert.AreEqual("newValue2", storedObj.Object.StringValue);

            repo.RemoveAll();
            repo.SaveChanges();

            // Test changing enumerated objects
            var objects = Enumerable.Range(0, 10).Select(x => new TestClass(x.ToString(), x.ToString()));
            repo.Insert(objects);
            repo.SaveChanges();

            foreach (var item in repo.Items)
                Assert.AreEqual(item.ID, item.StringValue);

            foreach (var item in repo.Items)
                item.StringValue = ((int.Parse(item.StringValue)) + 1).ToString();

            repo.SaveChanges();

            foreach (var item in repo.Items)
                Assert.AreNotEqual(item.ID, item.StringValue);

            repo.RemoveAll();
            repo.SaveChanges();
        }
        //===============================================================
        public static void TypedRepositoryTest(Repository<TestClass, String> repo)
        {
            repo.RemoveAll();
            repo.SaveChanges();

            var testObj = new TestClass("myKey", "myValue");
            repo.Insert(testObj);
            repo.SaveChanges();

            var newObj = new TestClass { ID = testObj.ID, StringValue = "NEW VALUE" };

            var dbObj = repo.Find(newObj.ID);
            dbObj.Object.StringValue = newObj.StringValue;
            repo.SaveChanges();

            dbObj = repo.Find(newObj.ID);
            Assert.AreEqual(newObj.StringValue, dbObj.Object.StringValue);

            repo.RemoveAll();
        }
        //===============================================================
        public static void BatchInsertAndRemove(Repository<TestClass> repo)
        {
            var testObjects = Enumerable.Range(0, 5).Select(x => new TestClass(x.ToString(), x.ToString())).ToList();

            repo.RemoveAll(repo.Items);
            repo.SaveChanges();
            Assert.IsTrue(!repo.Items.Any());


            repo.Insert(testObjects);
            repo.SaveChanges();
            Assert.IsTrue(repo.Items.Any());
            Assert.IsTrue(repo.Items.ToList().All(x => testObjects.Exists(y => y.ID == x.ID)));

            // Remove all batch
            repo.RemoveAll();
            repo.SaveChanges();
            Assert.IsTrue(!repo.Items.Any());

            // Remove all without saving in between
            repo.Insert(testObjects);
            repo.RemoveAll();
            repo.SaveChanges();
            Assert.IsTrue(!repo.Items.Any());

            // Remove all with inserts before and after
            repo.Insert(testObjects);
            repo.RemoveAll();
            repo.Insert(testObjects);
            repo.RemoveAll();
            repo.SaveChanges();
            Assert.IsTrue(!repo.Items.Any());


            repo.Insert(testObjects);
            repo.SaveChanges();

            // Remove all by key
            repo.RemoveAllByKey(repo.Items.ToList().Select(x => new object[] { x.ID }));
            repo.SaveChanges();

            Assert.IsTrue(!repo.Items.Any());

            // Remove all by object
            repo.Insert(testObjects);
            repo.SaveChanges();

            repo.RemoveAll(testObjects);
            repo.SaveChanges();

            Assert.IsTrue(!repo.Items.Any());
        }
        //===============================================================
        public static void Exists(Repository<TestClass> repo)
        {
            repo.RemoveAll();
            repo.SaveChanges();

            var item = new TestClass { ID = "1", StringValue = "blah" };
            repo.Insert(item);
            repo.SaveChanges();

            Assert.IsTrue(repo.Exists(item));
            repo.RemoveAll();
            repo.SaveChanges();
        }
        //===============================================================
        public static void All(Repository<TestClass> implicitKeyRepository, Repository<TestClass, String> typedKeyRepository = null,  ExplicitKeyRepository<TestClass> explicitKeyRepository= null)
        {
            InsertAndRemove(implicitKeyRepository);
            Items(implicitKeyRepository);
            ChangeTracking(implicitKeyRepository);
            BatchInsertAndRemove(implicitKeyRepository);
            Exists(implicitKeyRepository);

            if (typedKeyRepository != null)
            {
                TypedRepositoryTest(typedKeyRepository);
            }

            if (explicitKeyRepository != null)
            {
                ExplicitKeyRepository(explicitKeyRepository);
            }
        }
        //===============================================================
    }
}
