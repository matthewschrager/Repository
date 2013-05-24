using System;
using NUnit.Framework;

namespace Repository
{
        internal class InMemoryRepositoryTests
        {
            //===============================================================
            [Test]
            public void UpdateTest()
            {
                var repository = new InMemoryRepository<TestClass>(x => x.Key);
                repository.Insert(new TestClass());
                repository.SaveChanges();

                repository.Update(new { Value2 = DateTime.MaxValue }, 1);
                repository.SaveChanges();

                var val = repository.Find(1).Object;
                Assert.AreEqual(val.Value2, DateTime.MaxValue);

                var obj = new { Value2 = DateTime.MaxValue };
                repository.Update(obj, x => x.Property, 1);
                repository.SaveChanges();

                Assert.AreEqual(val.Property.Value2, DateTime.MaxValue);
                Assert.AreEqual(val.Property.Value1.Value, 1);

            }
            //===============================================================
        }
}
