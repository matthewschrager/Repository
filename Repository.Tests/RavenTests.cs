using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Newtonsoft.Json;
using Repository.RavenDB;
using System;
using Raven.Client;

namespace Repository.Tests
{
   
    [TestFixture]
    public class RavenTests
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

            foreach (var obj in TestClasses.GetItemsContext().Objects)
                TestClasses.Remove(obj.Key);
            foreach (var obj in Fixtures.GetItemsContext().Objects)
                Fixtures.Remove(obj.FixtureID);
            foreach (var obj in WorkOrders.GetItemsContext().Objects)
                WorkOrders.Remove(obj.ID);
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
        public void UpdateTest()
        {
            var obj = new TestClass();
            var guid = Guid.NewGuid();
            TestClasses.Store(obj);

            using (var dbObj = TestClasses.Find(obj.Key))
            {
                Assert.NotNull(dbObj.Object);

                dbObj.Object.Guid = guid;
                dbObj.SaveChanges();
            }

            using (var dbObj = TestClasses.Find(obj.Key))
            {
                Assert.NotNull(dbObj.Object);
                Assert.AreEqual(guid, dbObj.Object.Guid);
            }

            guid = Guid.NewGuid();
            obj.Guid = guid;

            TestClasses.Update(obj, obj.Key);
            
            using (var dbObj = TestClasses.Find(obj.Key))
            {
                Assert.NotNull(dbObj.Object);
                Assert.AreEqual(guid, dbObj.Object.Guid);
            }
        }
        //===============================================================
        [Test]
        public void UpdateFromJSONTest()
        {
            // TestClass
            var obj = new TestClass();
            TestClasses.Store(obj);

            TestClasses.Update("Property", "{ Value1: 2 }", UpdateType.Set, 1);
            using (var dbObj = TestClasses.Find(1))
            {
                Assert.AreEqual(dbObj.Object.Property.Value1, 2);
            }

            TestClasses.Update("{ ComplexList: { Value1: 2 } }", UpdateType.Add, 1);
            using (var dbObj = TestClasses.Find(1))
            {
                Assert.AreEqual(dbObj.Object.ComplexList.Count, 1);
                Assert.AreEqual(dbObj.Object.ComplexList.First().Value1, 2);
            }

            TestClasses.Remove(obj.Key);


            // Fixture
            var fixture = new Fixture();
            fixture.FixtureID = Guid.Parse("c7ed2664-58ca-4324-82c1-96fa54140258");
            fixture.Data = new FixtureData();
            fixture.Data.Condition = FixtureCondition.New;

            Fixtures.Store(fixture);
            var date = DateTime.Now;
            Fixtures.Update("Data", "{ 'Comments': { 'Date': '/Date(1224043200000)/', 'Value': 'Test Comment' } }", UpdateType.Add, "c7ed2664-58ca-4324-82c1-96fa54140258");
            using (var dbObj = Fixtures.Find(fixture.FixtureID))
            {
                Assert.AreEqual(dbObj.Object.Data.Comments.Count, 1);
                Assert.AreEqual(dbObj.Object.Data.Condition, FixtureCondition.New);
            }

            Fixtures.Update("Data", "{ UnitID: 'c7ed2664-58ca-4324-82c1-96fa54140258' }", UpdateType.Set, "c7ed2664-58ca-4324-82c1-96fa54140258");
            using (var dbObj = Fixtures.Find("c7ed2664-58ca-4324-82c1-96fa54140258"))
            {
                Assert.AreEqual(dbObj.Object.Data.UnitID, Guid.Parse("c7ed2664-58ca-4324-82c1-96fa54140258"));
            }

            Fixtures.Remove(fixture.FixtureID);

            // Work orders
            var workOrder = new WorkOrder();
            workOrder.ID = Guid.Parse("2f835d08-34c0-406d-8188-0ce5f33325fc");
            workOrder.Data = new WorkOrderData
                             {
                                 Name = "Couch"
                             };

            WorkOrders.Store(workOrder);
            WorkOrders.Update("Data", "{ 'Comment': 'Test Comment' }", UpdateType.Set, workOrder.ID);
            using (var dbObj = WorkOrders.Find(workOrder.ID))
            {
                Assert.AreEqual(dbObj.Object.Data.Comment, "Test Comment");
            }

            WorkOrders.Remove(workOrder.ID);
        }
        //===============================================================
        [Test]
        public void InvalidUpdateTest()
        {
            var obj = new TestClass();
            TestClasses.Store(obj);

            // First-level data
            Assert.That(() => TestClasses.Update("{'Value2': 'blah'}", UpdateType.Set, obj.Key), Throws.Exception);

            // Second-level data
            Assert.That(() => TestClasses.Update("TestProperty", "{ 'Value2': 'blah' }", UpdateType.Set, obj.Key), Throws.Exception);

            // List data
            Assert.That(() => TestClasses.Update("{ 'ComplexList': 'blah' }", UpdateType.Add, obj.Key), Throws.Exception);
            
            // Incorrect update type
            Assert.That(() => TestClasses.Update("{ 'Value2': 'blah' }", UpdateType.Add, obj.Key), Throws.Exception);

            // Incorrect data type (list)
            Assert.That(() => TestClasses.Update("{ 'ComplexList': 'blah' }", UpdateType.Set, obj.Key), Throws.Exception);

            TestClasses.Remove(obj.Key);

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

            WorkOrders.Remove(workOrder.ID);
        }
        //===============================================================
        [Test]
        public void UpdateGuidTest()
        {
            // First level
            var testObj = new TestClass();
            testObj.Guid = Guid.NewGuid();
            TestClasses.Store(testObj);

            var newGuid = Guid.NewGuid();
            TestClasses.Update("{ 'Guid': '" + newGuid + "' }", UpdateType.Set, testObj.Key);
            using (var dbObj = TestClasses.Find(testObj.Key))
            {
                Assert.AreEqual(newGuid, dbObj.Object.Guid);
            }

            TestClasses.Remove(testObj.Key);

            // Second level
            var fixture = new Fixture();
            fixture.FixtureID = Guid.Parse("2f835d08-34c0-406d-8188-0ce5f33325fc");
            fixture.Data = new FixtureData();

            Fixtures.Store(fixture);
            Fixtures.Update("Data", "{ UnitID: '" + newGuid + "' }", UpdateType.Set, fixture.FixtureID);


            using (var dbFixture = Fixtures.Find(fixture.FixtureID))
            {
                Assert.AreEqual(fixture.FixtureID, dbFixture.Object.FixtureID);
                Assert.AreEqual(newGuid, dbFixture.Object.Data.UnitID);
            }

            Fixtures.Remove(fixture.FixtureID);
        }
        //===============================================================
        [Test]
        public void UpdateListTest()
        {
            var obj = new TestClass();
            TestClasses.Store(obj);

            var list = new[] { 2 };
            TestClasses.Update("{ 'IntegerList': " + JsonConvert.SerializeObject(list) + " }", UpdateType.Set, obj.Key);
            using (var dbObj = TestClasses.Find(obj.Key))
            {
                Assert.NotNull(dbObj.Object);
                Assert.AreEqual(list.Count(), dbObj.Object.IntegerList.Count);
                Assert.IsTrue(list.SequenceEqual(dbObj.Object.IntegerList));
            }

            TestClasses.Remove(obj.Key);
        }
        //===============================================================
        [Test]
        public void SaveObjectTwice()
        {
            using (var repo1 = TestClasses = RavenRepository<TestClass>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.Key))
            {
                var obj = new TestClass();
                repo1.Store(obj);
            }

            Thread.Sleep(500);

            using (var repo2 = TestClasses = RavenRepository<TestClass>.FromUrlAndApiKey("https://1.ravenhq.com/databases/AppHarbor_c73ea268-8421-480b-8c4c-517eefb1750a", "e8e26c07-b6d5-4513-a7a6-d26d58ec2d33", x => x.Key))
            {
                var obj = new TestClass();
                obj.Guid = Guid.NewGuid();

                repo2.Store(obj);
                using (var dbObj = repo2.Find(obj.Key))
                {
                    Assert.NotNull(dbObj.Object);
                    Assert.AreEqual(obj.Guid, dbObj.Object.Guid);
                }

                repo2.Remove(obj);
            }
        }
        //===============================================================
    }
}
