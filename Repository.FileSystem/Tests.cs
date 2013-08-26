using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Repository.Testing;

namespace Repository.FileSystem
{
    [TestFixture]
    internal class Tests
    {
        //===============================================================
        [Test]
        public void Standard()
        {
            var implicitKeyRepo = new FileSystemRepository<TestClass>(x => x.ID, new FileSystemOptions<TestClass> { FolderPath = "Tests/ImplicitKeyRepositories" });
            var gzipRepo = new FileSystemRepository<TestClass>(x => x.ID, new FileSystemOptions<TestClass> { FolderPath = "Tests/ImplicitKeyRepositories", StreamGenerator = new GZipStreamGenerator(), FileExtension = ".txt.gz" });
            
            StandardTests.All(implicitKeyRepo);
            StandardTests.All(gzipRepo);
        }
        //===============================================================
        [Test]
        public void MultipleKeys()
        {
            using (var repo = new FileSystemRepository<TestClass, String, String>(x => Tuple.Create(x.ID, x.StringValue)))
            {
                var obj = new TestClass("key", "value");
                repo.Insert(obj);
                repo.SaveChanges();

                Assert.AreEqual(1, repo.Items.Count());

                repo.Remove(obj);
                repo.SaveChanges();
                Assert.AreEqual(0, repo.Items.Count());
            }
        }
        //================================================================================
    }


}
