using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Repository.Azure
{
    internal class AzureApi
    {
        //===============================================================
        public AzureApi(CloudStorageAccount storageAccount, ISerializer serializer = null, Encoding encoding = null)
        {
            StorageAccount = storageAccount;
            Serializer = serializer ?? new JsonSerializer();
            Encoding = encoding ?? Encoding.ASCII;
        }
        //===============================================================
        private ISerializer Serializer { get; set; }
        //===============================================================
        private CloudStorageAccount StorageAccount { get; set; }
        //===============================================================
        private Encoding Encoding { get; set; }
        //===============================================================
        private static String GetAzureKey(IEnumerable<Object> keys)
        {
            var key = Uri.EscapeUriString(keys.Select(x => x.ToString()).Aggregate((curr, next) => curr + "-" + next));
            return key;
        }
        //===============================================================
        private CloudBlockBlob GetBlock(String containerName, IEnumerable<Object> keys)
        {
            return GetContainer(containerName).GetBlockBlobReference(GetAzureKey(keys));
        }
        //===============================================================
        private CloudBlobContainer GetContainer(String containerName)
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            container.CreateIfNotExists();

            return container;
        }
        //===============================================================
        private T DeserializeBlock<T>(CloudBlockBlob block)
        {
            using (var stream = new MemoryStream())
            {
                block.DownloadToStream(stream);
                var str = Encoding.GetString(stream.ToArray());
                return Serializer.Deserialize<T>(str);
            }
        }
        //===============================================================
        public void StoreObject<T>(T value, IEnumerable<Object> keys, String containerName)
        {
            var block = GetBlock(containerName, keys);
            var encodedValue = Encoding.GetBytes(Serializer.Serialize(value));
            using (var stream = new MemoryStream(encodedValue))
            {
                stream.Seek(0, SeekOrigin.Begin);
                block.UploadFromStream(stream);
            }
        }
        //===============================================================
        public T GetObject<T>(IEnumerable<Object> keys, String containerName) where T : class
        {
            var block = GetBlock(containerName, keys);
            if (!block.Exists())
                return null;

            return DeserializeBlock<T>(block);
        }
        //===============================================================
        public void DeleteObject(IEnumerable<Object> keys, String containerName)
        {
            var block = GetBlock(containerName, keys);
            block.DeleteIfExists();
        }
        //===============================================================
        public void DeleteContainer(String containerName)
        {
            var container = GetContainer(containerName);
            container.DeleteIfExists();
        }
        //===============================================================
        public bool Exists(IEnumerable<Object> keys, String containerName)
        {
            var block = GetBlock(containerName, keys);
            return block.Exists();
        }
        //===============================================================
        public IEnumerable<T> EnumerateObjects<T>(String containerName)
        {
            var container = GetContainer(containerName);
            return container.ListBlobs(null, true).Cast<CloudBlockBlob>().Select(DeserializeBlock<T>);
        }
        //===============================================================
    }
}
