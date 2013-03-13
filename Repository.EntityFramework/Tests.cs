using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace Repository.EntityFramework
{
    internal class TestObject
    {
        //===============================================================
        [Key]
        public String ID { get; set; }
        //===============================================================
        public String Value { get; set; }
        //===============================================================
    }

    internal class TestObject2
    {
        //===============================================================
        [Key]
        public int ID { get; set; }
        //===============================================================
        public String Value { get; set; }
    }

    internal class TestContext : DbContext
    {
        //===============================================================
        public DbSet<TestObject> Objects { get; set; }
        //===============================================================
        public DbSet<TestObject2> Objects2 { get; set; }
        //===============================================================
    }

    [TestFixture]
    internal class EFRepositoryTests
    {
        List<TestObject> mTestObjects = Enumerable.Range(0, 100).Select(x => new TestObject { ID = x.ToString(), Value = x.ToString() }).ToList();

        //===============================================================
        [Test]
        public void BatchInsertAndRemoveTest()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObject>(x => x.Objects, x => x.ID))
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

            using (var repo = new EFRepository<TestContext, TestObject>(x => x.Objects, x => x.ID))
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

            using (var repo = new EFRepository<TestContext, TestObject>(x => x.Objects, x => x.ID))
            {
                repo.RemoveAllByKey(repo.Items.ToList().Select(x => new object[] { x.ID }));


                var objects = Enumerable.Range(0, 100).Select(x => new TestObject { ID = x.ToString(), Value = x.ToString() }).ToList();
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

            using (var repo = new EFRepository<TestContext, TestObject>(x => x.Objects, x => x.ID))
            {
                repo.RemoveAll();
                repo.SaveChanges();

                var objects = Enumerable.Range(0, 100).Select(x => new TestObject { ID = x.ToString(), Value = x.ToString() }).ToList();
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
        public void TransactionsWork()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestObject>(x => x.Objects, x => x.ID))
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

            using (var repo = new EFRepository<TestContext, TestObject>(x => x.Objects, x => x.ID))
            {
                Assert.IsTrue(repo.Items.Any());
                repo.RemoveAll();
                repo.SaveChanges();
            }


            using (var repo = new EFRepository<TestContext, TestObject>(x => x.Objects, x => x.ID))
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
}
