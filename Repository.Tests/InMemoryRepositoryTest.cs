using System.Dynamic;
using NUnit.Framework;
using Repository;
using System;
using System.Collections.Generic;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Repository.Tests
{


    /// <summary>
    ///This is a test class for InMemoryRepositoryTest and is intended
    ///to contain all InMemoryRepositoryTest Unit Tests
    ///</summary>
    [TestFixture]
    internal class InMemoryRepositoryTest
    {


        private NUnit.Framework.TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public NUnit.Framework.TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        [Test]
        public void UpdateTest()
        {
            var repository = new InMemoryRepository<Repository.TestClass>(x => x.Key);
            repository.Store(new Repository.TestClass());
            repository.Update(new { Value2 = DateTime.MaxValue }, 1);

            var val = repository.Find(1).Object;
            NUnit.Framework.Assert.AreEqual(val.Value2, DateTime.MaxValue);

            var obj = new { Value2 = DateTime.MaxValue };
            repository.Update(obj, x => x.Property, 1);
            NUnit.Framework.Assert.AreEqual(val.Property.Value2, DateTime.MaxValue);
            NUnit.Framework.Assert.AreEqual(val.Property.Value1.Value, 1);

        }
    }
}
