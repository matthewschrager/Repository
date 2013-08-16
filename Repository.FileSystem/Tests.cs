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
    }


}
