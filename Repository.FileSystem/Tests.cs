using System;
using System.Collections.Generic;
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
            var typedRepo = new FileSystemRepository<TestClass, String>(x => x.ID, new FileSystemOptions<TestClass> { FolderPath = "Tests/TypedKeyRepositories" });
            var explicitKeyRepo = new ExplicitKeyFileSystemRepository<TestClass>(new FileSystemOptions<TestClass> { FolderPath = "Tests/ExplicitKeyRepositories" });
            StandardTests.All(implicitKeyRepo);
        }
        //===============================================================
    }
}
