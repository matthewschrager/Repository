using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using NUnit.Framework;
using Repository.Testing;

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
        public DbSet<TestClass> Objects { get; set; }
        //===============================================================
        public DbSet<TestObjectWithExplicitKey> ExplicitKeys { get; set; }
        //===============================================================
        public DbSet<TestObjectWithConventionKey> ConventionKeys { get; set; }
        //===============================================================
    }

    [TestFixture]
    internal class EFRepositoryTests
    {
        List<TestClass> mTestObjects = Enumerable.Range(0, 5).Select(x => new TestClass { ID = x.ToString(), StringValue = x.ToString() }).ToList();

        //===============================================================
        [Test]
        public void Standard()
        {
            var implicitKeyRepo = new EFRepository<TestContext, TestClass>(x => x.Objects);
            var typedKeyRepo = new EFRepository<TestContext, TestClass, String>(x => x.Objects);
            StandardTests.All(implicitKeyRepo, typedKeyRepo);
        }
        //===============================================================
        [Test]
        public void TransactionsWork()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Test")
                Assert.Ignore("Skipped on AppHarbor");

            using (var repo = new EFRepository<TestContext, TestClass>(x => x.Objects))
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

            using (var repo = new EFRepository<TestContext, TestClass>(x => x.Objects))
            {
                Assert.IsTrue(repo.Items.Any());
                repo.RemoveAll();
                repo.SaveChanges();
            }


            using (var repo = new EFRepository<TestContext, TestClass>(x => x.Objects))
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
