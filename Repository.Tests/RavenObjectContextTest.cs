using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Repository.RavenDB;
using System;
using Raven.Client;

namespace Repository.Tests
{
   
    [TestFixture]
    public class RavenObjectContextTest
    {
        private RavenRepository<TestClass> TestClasses;
        private RavenRepository<Fixture> Fixtures;
        private RavenRepository<WorkOrder> WorkOrders;

        //===============================================================
        [SetUp]
        public void SetUp()
        {
            TestClasses = RavenRepository<TestClass>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.Key);
            Fixtures = RavenRepository<Fixture>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.FixtureID);
            WorkOrders = RavenRepository<WorkOrder>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.ID);
        }
        //===============================================================
        [TearDown]
        public void TearDown()
        {
            TestClasses.Dispose();
            Fixtures.Dispose();
            WorkOrders.Dispose();
        }
        //===============================================================
        [Test]
        public void UpdateFromJSONTest()
        {
            var obj = new TestClass();
            TestClasses.Store(obj);

            TestClasses.UpdateFromJSON("Property", "{ Value1: 2 }", UpdateType.Set, 1);
            using (var dbObj = TestClasses.Find(1))
            {
                Assert.AreEqual(dbObj.Object.Property.Value1, 2);
            }

            TestClasses.UpdateFromJSON("{ List: { Value1: 2 } }", UpdateType.Add, 1);
            using (var dbObj = TestClasses.Find(1))
            {
                Assert.AreEqual(dbObj.Object.List.Count, 1);
                Assert.AreEqual(dbObj.Object.List.First().Value1, 2);
            }

            var fixture = new Fixture();
            fixture.FixtureID = Guid.Parse("c7ed2664-58ca-4324-82c1-96fa54140258");
            fixture.Data = new FixtureData();
            fixture.Data.Condition = FixtureCondition.New;

            Fixtures.Store(fixture);
            var date = DateTime.Now;
            Fixtures.UpdateFromJSON("Data", "{ 'Comments': { 'Date': '/Date(1224043200000)/', 'Value': 'Test Comment' } }", UpdateType.Add, "c7ed2664-58ca-4324-82c1-96fa54140258");
            using (var dbObj = Fixtures.Find("c7ed2664-58ca-4324-82c1-96fa54140258"))
            {
                Assert.AreEqual(dbObj.Object.Data.Comments.Count, 1);
                Assert.AreEqual(dbObj.Object.Data.Condition, FixtureCondition.New);
            }

            using (var dbObj = Fixtures.GetItemsContext())
            {
                var result = dbObj.Objects.Where(x => x.Data.Condition == FixtureCondition.New).ToList();
                Assert.AreEqual(result.Count, 1);
            }

            Fixtures.UpdateFromJSON("Data", "{ UnitID: 'c7ed2664-58ca-4324-82c1-96fa54140258' }", UpdateType.Set, "c7ed2664-58ca-4324-82c1-96fa54140258");
            using (var dbObj = Fixtures.Find("c7ed2664-58ca-4324-82c1-96fa54140258"))
            {
                Assert.AreEqual(dbObj.Object.Data.UnitID, Guid.Parse("c7ed2664-58ca-4324-82c1-96fa54140258"));
            }

            var workOrder = new WorkOrder();
            workOrder.ID = Guid.Parse("2f835d08-34c0-406d-8188-0ce5f33325fc");
            workOrder.Data = new WorkOrderData
                             {
                                 Name = "Couch"
                             };

            WorkOrders.Store(workOrder);
            WorkOrders.UpdateFromJSON("Data", "{ 'Comment': 'Test Comment' }", UpdateType.Set, workOrder.ID);
            using (var dbObj = WorkOrders.Find(workOrder.ID))
            {
                Assert.AreEqual(dbObj.Object.Data.Comment, "Test Comment");
            }
        }
        //===============================================================
        [Test, ExpectedException(ExpectedException = typeof(ArgumentException))]
        public void InvalidUpdateTest()
        {
            var obj = new TestClass();
            TestClasses.Store(obj);

            // First-level data
            TestClasses.UpdateFromJSON("{'Value2': 'blah'}", UpdateType.Set, obj.Key);

            // Second-level data
            TestClasses.UpdateFromJSON("TestProperty", "{ 'Value2': 'blah' }", UpdateType.Set, obj.Key);

            // List data
            TestClasses.UpdateFromJSON("{ 'List': 'blah' }", UpdateType.Add, obj.Key);

        }
        //===============================================================
        [Test]
        public void FindFromJSONTest()
        {
            var workOrder = new WorkOrder();
            workOrder.ID = Guid.Parse("2f835d08-34c0-406d-8188-0ce5f33325fc");
            workOrder.Data = new WorkOrderData
            {
                Name = "Couch"
            };

            WorkOrders.Store(workOrder);

            var result = WorkOrders.FindFromJSON("{ 'ID': '2f835d08-34c0-406d-8188-0ce5f33325fc' }").ToList();
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Data.Name, "Couch");

            result = WorkOrders.FindFromJSON("{ 'ID': '2f835d08-34c0-406d-8188-0ce5f33325f3' }").ToList();
            Assert.AreEqual(result.Count, 0);
        }
        //===============================================================
    }
}
