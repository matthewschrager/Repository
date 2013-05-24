using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using NUnit.Framework;

namespace Repository.EntityFramework
{
    internal class TestObjectWithExplicitKey
    {
        //===============================================================
        [Key, Column]
        public String ID { get; set; }
        //===============================================================
        public String Value { get; set; }
        //===============================================================
    }

    internal class TestObjectWithConventionKey
    {
        //===============================================================
        public String ID { get; set; }
        //===============================================================
        public String Value { get; set; }
        //===============================================================
    }

    internal class TestContext : DbContext
    {
        //===============================================================
        public DbSet<TestObjectWithExplicitKey> ExplicitObjects { get; set; }
        //===============================================================
        public DbSet<TestObjectWithConventionKey> ConventionObjects { get; set; }
        //===============================================================
    }

    [TestFixture]
    internal class EFRepositoryTests
    {
        List<TestObjectWithExplicitKey> mTestObjects = Enumerable.Range(0, 100).Select(x => new TestObjectWithExplicitKey { ID = x.ToString(), Value = x.ToString() }).ToList();

        //===============================================================
        [Test]
        public void TypedRepositoryTest()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey, String>(x => x.ExplicitObjects))
            {
                repo.RemoveAll();
                repo.SaveChanges();

                var testObj = mTestObjects.First();
                repo.Insert(testObj);
                repo.SaveChanges();

                var newObj = new TestObjectWithExplicitKey { ID = testObj.ID, Value = "NEW VALUE" };

                var dbObj = repo.Find(newObj.ID);
                dbObj.Update(newObj);
                repo.SaveChanges();

                dbObj = repo.Find(newObj.ID);
                Assert.AreEqual(newObj.Value, dbObj.Object.Value);

                repo.RemoveAll();
            }
        }
        //===============================================================
        [Test]
        public void Update()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey>(x => x.ExplicitObjects))
            {
                repo.RemoveAll();
                repo.SaveChanges();

                var testObj = mTestObjects.First();
                repo.Insert(testObj);
                repo.SaveChanges();

                var newObj = new TestObjectWithExplicitKey { ID = testObj.ID, Value = "NEW VALUE" };

                var dbObj = repo.Find(newObj.ID);
                dbObj.Update(newObj);
                repo.SaveChanges();

                dbObj = repo.Find(newObj.ID);
                Assert.AreEqual(newObj.Value, dbObj.Object.Value);

                repo.RemoveAll();
            }
        }
        //===============================================================
        [Test]
        public void BatchInsertAndRemoveTest()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey>(x => x.ExplicitObjects))
            {
                repo.RemoveAll(repo.Items);
                repo.Insert(mTestObjects);
                repo.SaveChanges();

                var storedObjects = repo.Items.ToList();
                Assert.IsTrue(storedObjects.All(x => mTestObjects.Exists(y => y.ID == x.ID)));

                repo.RemoveAll(repo.Items);
                Assert.IsTrue(repo.Items.Any());

                repo.SaveChanges();
                
                storedObjects = repo.Items.ToList();
                Assert.IsTrue(!storedObjects.Any());
            }
        }
        //===============================================================
        [Test]
        public void RemoveAll()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey>(x => x.ExplicitObjects))
            {
                repo.RemoveAll(repo.Items);
                repo.SaveChanges();
                Assert.IsTrue(!repo.Items.Any());


                repo.Insert(mTestObjects);
                repo.SaveChanges();
                Assert.IsTrue(repo.Items.Any());

                // Remove all batch
                repo.RemoveAll();
                repo.SaveChanges();
                Assert.IsTrue(!repo.Items.Any());

                // Remove all without saving in between
                repo.Insert(mTestObjects);
                repo.RemoveAll();
                repo.SaveChanges();
                Assert.IsTrue(!repo.Items.Any());

                // Remove all with inserts before and after
                repo.Insert(mTestObjects);
                repo.RemoveAll();
                repo.Insert(mTestObjects);
                repo.RemoveAll();
                repo.SaveChanges();
                Assert.IsTrue(!repo.Items.Any());


                repo.Insert(mTestObjects);
                repo.SaveChanges();

                // Remove all by key
                repo.RemoveAllByKey(repo.Items.ToList().Select(x => new object[] { x.ID }));
                repo.SaveChanges();

                Assert.IsTrue(!repo.Items.Any());

                // Remove all by object
                repo.Insert(mTestObjects);
                repo.SaveChanges();

                repo.RemoveAll(mTestObjects);
                repo.SaveChanges();

                Assert.IsTrue(!repo.Items.Any());
            }
        }
        //===============================================================
        [Test]
        public void BatchInsertTest()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey>(x => x.ExplicitObjects))
            {
                repo.RemoveAllByKey(repo.Items.ToList().Select(x => new object[] { x.ID }));


                var objects = Enumerable.Range(0, 100).Select(x => new TestObjectWithExplicitKey { ID = x.ToString(), Value = x.ToString() }).ToList();
                repo.Insert(objects);
                repo.RemoveAll(objects);
            }
        }
        //===============================================================
        [Test]
        public void SaveItemsChangesTest()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey>(x => x.ExplicitObjects))
            {
                repo.RemoveAll();
                repo.SaveChanges();

                var objects = Enumerable.Range(0, 100).Select(x => new TestObjectWithExplicitKey { ID = x.ToString(), Value = x.ToString() }).ToList();
                repo.Insert(objects);
                repo.SaveChanges();

                var items = repo.Items.ToList();
                items.ForEach(x => x.Value = "MODIFIED");
                repo.SaveChanges();

                items = repo.Items.ToList();
                Assert.IsTrue(items.All(x => x.Value == "MODIFIED"));
            }
        }
        //===============================================================
        [Test]
        public void ExistsWorks()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey>(x => x.ExplicitObjects))
            {
                repo.RemoveAll();
                repo.SaveChanges();

                var item = new TestObjectWithExplicitKey { ID = "1", Value = "blah" };
                repo.Insert(item);
                repo.SaveChanges();

                Assert.IsTrue(repo.Exists(item));
                repo.RemoveAll();
                repo.SaveChanges();
            }
        }
        //===============================================================
        [Test]
        public void TransactionsWork()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey>(x => x.ExplicitObjects))
            {
                repo.RemoveAll();
                repo.SaveChanges();

                using (var scope = new TransactionScope())
                {
                    repo.Insert(mTestObjects.First());
                    repo.SaveChanges();
                    scope.Complete();
                }
            }

            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey>(x => x.ExplicitObjects))
            {
                Assert.IsTrue(repo.Items.Any());
                repo.RemoveAll();
                repo.SaveChanges();
            }


            using (var repo = new EFRepository<TestContext, TestObjectWithExplicitKey>(x => x.ExplicitObjects))
            {

                try
                {
                    using (var scope = new TransactionScope())
                    {
                        repo.Insert(mTestObjects.First());
                        repo.SaveChanges();

                        throw new Exception();
                    }
                }

                catch (Exception e)
                {
                    Assert.IsFalse(repo.Items.Any());
                }
            }
        }
        //===============================================================
    }

    [TestFixture]
    internal class DbContextExtensionTests
    {
        //===============================================================
        [Test]
        public void GetKeySelector()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            var explicitKey = new TestObjectWithExplicitKey { ID = "myKey" };
            var explicitKeySelector = new TestContext().GetKeySelector<TestObjectWithExplicitKey>();
            CollectionAssert.AreEquivalent(new[] { explicitKey.ID }, explicitKeySelector(explicitKey));

            var conventionKey = new TestObjectWithConventionKey { ID = "myKey2" };
            var conventionKeySelector = new TestContext().GetKeySelector<TestObjectWithConventionKey>();
            CollectionAssert.AreEquivalent(new[] { conventionKey.ID }, conventionKeySelector(conventionKey));
        }
        //===============================================================
    }
}
