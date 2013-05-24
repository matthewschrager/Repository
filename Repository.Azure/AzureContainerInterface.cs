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
    internal class AzureContainerInterface
    {
        //===============================================================
        public AzureContainerInterface(CloudStorageAccount storageAccount, AzureOptions options)
        {
            StorageAccount = storageAccount;
            Options = options;
        }
        //===============================================================
        private AzureOptions Options { get; set; }
        //===============================================================
        private CloudStorageAccount StorageAccount { get; set; }
        //===============================================================
        private static String GetAzureKey(IEnumerable<Object> keys)
        {
            var key = Uri.EscapeUriString(keys.Select(x => x.ToString()).Aggregate((curr, next) => curr + "-" + next));
            return key;
        }
        //===============================================================
        private CloudBlockBlob GetBlock(IEnumerable<Object> keys)
        {
            return GetContainer().GetBlockBlobReference(GetAzureKey(keys));
        }
        //===============================================================
        private CloudBlobContainer GetContainer()
        {
            var client = StorageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(Options.ContainerName);
            container.CreateIfNotExists();

            return container;
        }
        //===============================================================
        private T DeserializeBlock<T>(CloudBlockBlob block)
        {
            using (var stream = new MemoryStream())
            {
                block.DownloadToStream(stream);
                var str = Options.Encoding.GetString(stream.ToArray());
                return Options.Serializer.Deserialize<T>(str);
            }
        }
        //===============================================================
        public void StoreObject<T>(T value, IEnumerable<Object> keys)
        {
            var block = GetBlock(keys);
            var encodedValue = Options.Encoding.GetBytes(Options.Serializer.Serialize(value));
            using (var stream = new MemoryStream(encodedValue))
            {
                stream.Seek(0, SeekOrigin.Begin);
                block.UploadFromStream(stream);
            }

            if (!String.IsNullOrWhiteSpace(Options.ContentType))
            {
                block.Properties.ContentType = Options.ContentType;
                block.SetProperties();
            }
        }
        //===============================================================
        public GetObjectResult<T> GetObject<T>(IEnumerable<Object> keys)
        {
            var block = GetBlock(keys);
            if (!block.Exists())
                return new GetObjectResult<T>();

            return new GetObjectResult<T>(DeserializeBlock<T>(block));
        }
        //===============================================================
        public void DeleteObject(IEnumerable<Object> keys)
        {
            var block = GetBlock(keys);
            block.DeleteIfExists();
        }
        //===============================================================
        public void DeleteContainer()
        {
            var container = GetContainer();
            container.DeleteIfExists();
        }
        //===============================================================
        public bool Exists(IEnumerable<Object> keys)
        {
            var block = GetBlock(keys);
            return block.Exists();
        }
        //===============================================================
        public IEnumerable<T> EnumerateObjects<T>()
        {
            var container = GetContainer();
            return container.ListBlobs(null, true).Cast<CloudBlockBlob>().Select(DeserializeBlock<T>);
        }
        //===============================================================
        public Uri GetObjectUri(IEnumerable<Object> keys)
        {
            var block = GetBlock(keys);
            return block.Uri;
        }
        //===============================================================
    }

    internal class GetObjectResult<T>
    {
        private T mObject;

        //===============================================================
        public GetObjectResult()
        {
            HasObject = false;
        }
        //===============================================================
        public GetObjectResult(T obj)
        {
            mObject = obj;
            HasObject = true;
        }
        //===============================================================
        public bool HasObject { get; private set; }
        //===============================================================
        public T Object
        {
            get
            {
                if (!HasObject)
                    throw new InvalidOperationException("The result doesn't contain any object.");

                return mObject;
            }
        }
        //===============================================================
    }
}
