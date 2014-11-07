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
            var implicitKeyRepo = new FileSystemRepository<TestClass>("Test", x => x.ID, new FileSystemOptions<TestClass> { FolderPath = "Tests/ImplicitKeyRepositories" });
            var gzipRepo = new FileSystemRepository<TestClass>("Test", x => x.ID, new FileSystemOptions<TestClass> { FolderPath = "Tests/ImplicitKeyRepositories", StreamGenerator = new GZipStreamGenerator(), FileExtension = ".txt.gz" });
            var explicitKeyRepo = new ExplicitKeyFileSystemRepository<TestClass>("Test", new FileSystemOptions<TestClass> { FolderPath = "Tests/ExplicitKeyRepositories" });
            var multipleFileRepo = new FileSystemRepository<TestClass>("Test", x => x.ID, new FileSystemOptions<TestClass> { FolderPath = "Tests/ImplicitKeyRepositories", FileStorageType = FileStorageType.FilePerObject });
            
            StandardTests.All(implicitKeyRepo, null, explicitKeyRepo);
            StandardTests.All(gzipRepo);
            StandardTests.All(multipleFileRepo);
        }
        //===============================================================
        [Test]
        public void MultipleKeys()
        {
            using (var repo = new FileSystemRepository<TestClass, String, String>("Test", x => Tuple.Create(x.ID, x.StringValue)))
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
        //===============================================================
        [Test]
        public void LockingTest()
        {
            Task.Factory.StartNew(() =>
            {
                using (var repo = new FileSystemRepository<TestClass, String, String>("Test", x => Tuple.Create(x.ID, x.StringValue)))
                {
                    var obj = new TestClass("key", "value");
                    repo.Insert(obj);
                    repo.SaveChanges();

                    repo.Remove(obj);
                    repo.SaveChanges();
                }
            });

            Task.Factory.StartNew(() =>
            {
                using (var repo = new FileSystemRepository<TestClass, String, String>("Test", x => Tuple.Create(x.ID, x.StringValue)))
                {
                    var obj = new TestClass("key", "value");
                    repo.Insert(obj);
                    repo.SaveChanges();

                    repo.Remove(obj);
                    repo.SaveChanges();
                }
            });
        }
        //===============================================================
        [Test]
        public void Backup()
        {
            var implicitKeyRepo = new FileSystemRepository<TestClass>("Test", x => x.ID, new FileSystemOptions<TestClass> { FolderPath = "BackupTests/SingleFile" });
            var multipleFileRepo = new FileSystemRepository<TestClass>("Test", x => x.ID, new FileSystemOptions<TestClass> { FolderPath = "BackupTests/FilePerObject", FileStorageType = FileStorageType.FilePerObject });

            var expectedSingleFilePath = "BackupTests/SingleFile/backup/" + DateTime.Today.ToString("yyyyMMdd");
            var expectedFilePerObjectPath = "BackupTests/FilePerObject/Test/backup/" + DateTime.Today.ToString("yyyyMMdd");

            try
            {

                var obj = new TestClass("key", "value");
                implicitKeyRepo.Insert(obj);
                multipleFileRepo.Insert(obj);

                implicitKeyRepo.SaveChanges();
                multipleFileRepo.SaveChanges();

                implicitKeyRepo.CreateBackup();
                multipleFileRepo.CreateBackup();

                Assert.IsTrue(Directory.Exists(expectedSingleFilePath) && Directory.EnumerateFiles(expectedSingleFilePath).Any());
                Assert.IsTrue(Directory.Exists(expectedFilePerObjectPath) && Directory.EnumerateFiles(expectedFilePerObjectPath).Any());

                // Try again to make sure we don't have any issues with overwriting existing files
                implicitKeyRepo.CreateBackup();
                multipleFileRepo.CreateBackup();

                Assert.IsTrue(Directory.Exists(expectedSingleFilePath) && Directory.EnumerateFiles(expectedSingleFilePath).Any());
                Assert.IsTrue(Directory.Exists(expectedFilePerObjectPath) && Directory.EnumerateFiles(expectedFilePerObjectPath).Any());
            }

            catch (Exception)
            {
                throw;
            }

            finally
            {
                Directory.Delete(expectedSingleFilePath, true);
                Directory.Delete(expectedFilePerObjectPath, true);

                implicitKeyRepo.RemoveAll();
                multipleFileRepo.RemoveAll();

                implicitKeyRepo.SaveChanges();
                multipleFileRepo.SaveChanges();
            }
        }
        //===============================================================
    }


}
