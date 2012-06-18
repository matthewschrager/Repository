using System.Collections.Generic;
using System.Linq;
using Repository.RavenDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Raven.Client;

namespace Repository.Tests
{
    
    
    /// <summary>
    ///This is a test class for RavenObjectContextTest and is intended
    ///to contain all RavenObjectContextTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RavenObjectContextTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void UpdateFromJSONTest()
        {
            var repository = RavenRepository<TestClass>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.Key);

            var obj = new TestClass();
            repository.Store(obj);

            repository.UpdateFromJSON("Property", "{ Value1: 2 }", UpdateType.Set, 1);
            using (var dbObj = repository.Find(1))
            {
                Assert.AreEqual(dbObj.Object.Property.Value1, 2);
            }

            repository.UpdateFromJSON("{ List: { Value1: 2 } }", UpdateType.Add, 1);
            using (var dbObj = repository.Find(1))
            {
                Assert.AreEqual(dbObj.Object.List.Count, 1);
                Assert.AreEqual(dbObj.Object.List.First().Value1, 2);
            }

            var fixtureRepo = RavenRepository<Fixture>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.FixtureID);
            var fixture = new Fixture();
            fixture.FixtureID = Guid.Parse("c7ed2664-58ca-4324-82c1-96fa54140258");
            fixture.Data = new FixtureData();

            fixtureRepo.Store(fixture);
            var date = DateTime.Now;
            fixtureRepo.UpdateFromJSON("Data", "{ 'Comments': { 'Date': '/Date(1224043200000)/', 'Value': 'Test Comment' } }", UpdateType.Add, "c7ed2664-58ca-4324-82c1-96fa54140258");
            using (var dbObj = fixtureRepo.Find("c7ed2664-58ca-4324-82c1-96fa54140258"))
            {
                Assert.AreEqual(dbObj.Object.Data.Comments.Count, 1);
            }
        }
    }
}
