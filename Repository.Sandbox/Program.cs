using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.EntityFramework;

namespace Repository.Sandbox
{
    internal class TestObject
    {
        //===============================================================
        [Key]
        public int ID { get; set; }
        //===============================================================
        public String Value { get; set; }
        //===============================================================
    }

    internal class TestContext : DbContext
    {
        //===============================================================
        public DbSet<TestObject> Objects { get; set; }
        //===============================================================
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var repo = new EFRepository<TestContext, TestObject>(x => x.Objects, x => x.ID))
            {
                repo.Store(new TestObject { ID = 1, Value = 1.ToString() });
                var objects = Enumerable.Range(1, 100).Select(x => new TestObject { ID = x, Value = x.ToString() }).ToList();
                repo.Store(objects);
            }
        }
    }
}
