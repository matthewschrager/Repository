using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Repository.Azure
{
    internal class AzureContainerInterface<T>
    {
        //===============================================================
        public AzureContainerInterface(CloudStorageAccount storageAccount, AzureOptions<T> options)
        {
            StorageAccount = storageAccount;
            Options = options;
        }
        //===============================================================
        private AzureOptions<T> Options { get; set; }
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
            
            if (Options.Permissions != null)
                container.SetPermissions(Options.Permissions);

            return container;
        }
        //===============================================================
        private T DeserializeBlock(CloudBlockBlob block)
        {
            using (var stream = new MemoryStream())
            {
                block.DownloadToStream(stream);
                using (var reader = new StreamReader(stream))
                {
                    var obj = Options.Serializer.Deserialize(reader.ReadToEnd());
                    return obj;
                }
            }
        }
        //===============================================================
        public void StoreObject(T value, IEnumerable<Object> keys)
        {
            var block = GetBlock(keys);
            var encodedValue = Options.Serializer.Serialize(value);
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(encodedValue);
                    stream.Seek(0, SeekOrigin.Begin);

                    block.UploadFromStream(stream);
                }
            }

            if (!String.IsNullOrWhiteSpace(Options.ContentType))
            {
                block.Properties.ContentType = Options.ContentType;
                block.SetProperties();
            }
        }
        //===============================================================
        public GetObjectResult<T> GetObject(IEnumerable<Object> keys)
        {
            var block = GetBlock(keys);
            if (!block.Exists())
                return new GetObjectResult<T>();

            return new GetObjectResult<T>(DeserializeBlock(block));
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
        public IEnumerable<T> EnumerateObjects()
        {
            var container = GetContainer();
            return container.ListBlobs(null, true).Cast<CloudBlockBlob>().Select(DeserializeBlock);
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
