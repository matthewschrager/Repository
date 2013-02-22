using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                repo.Store(mTestObjects);

                var storedObjects = repo.Items.ToList();
                Assert.IsTrue(storedObjects.All(x => mTestObjects.Exists(y => y.ID == x.ID)));

                repo.RemoveAll(repo.Items);
                
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
                repo.Store(mTestObjects);

                repo.RemoveAll();
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
                repo.Store(objects);
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
                var objects = Enumerable.Range(0, 100).Select(x => new TestObject { ID = x.ToString(), Value = x.ToString() }).ToList();
                repo.Store(objects);

                var items = repo.Items.ToList();
                items.ForEach(x => x.Value = "MODIFIED");
                repo.Items.SaveChanges();

                items = repo.Items.ToList();
                Assert.IsTrue(items.All(x => x.Value == "MODIFIED"));
            }
        }
        //===============================================================
    }
}
