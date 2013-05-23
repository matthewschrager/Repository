using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Repository.Azure
{
    internal class AzureInsert<T> : Insert<T> where T : class
    {
        //===============================================================
        public AzureInsert(IEnumerable<Object> keys, T value, AzureApi azureApi, String containerName)
            : base(keys, value)
        {
            AzureApi = azureApi;
            ContainerName = containerName;
        }
        //===============================================================
        private AzureApi AzureApi { get; set; }
        //===============================================================
        private String ContainerName { get; set; }
        //===============================================================
        public override void Apply()
        {
            AzureApi.StoreObject(Value, Keys, ContainerName);
        }
        //===============================================================
    }

    internal class AzureRemove : Remove
    {
        //===============================================================
        public AzureRemove(IEnumerable<Object> keys, AzureApi azureApi, String containerName)
            : base(keys)
        {
            AzureApi = azureApi;
            ContainerName = containerName;
        }
        //===============================================================
        private AzureApi AzureApi { get; set; }
        //===============================================================
        private String ContainerName { get; set; }
        //===============================================================
        public override void Apply()
        {
            AzureApi.DeleteObject(Keys, ContainerName);
        }
        //===============================================================
    }

    internal class AzureRemoveAll : IPendingChange
    {
        //===============================================================
        public AzureRemoveAll(AzureApi azureApi, String containerName)
        {
            AzureApi = azureApi;
            ContainerName = containerName;
        }
        //===============================================================
        private AzureApi AzureApi { get; set; }
        //===============================================================
        private String ContainerName { get; set; }
        //===============================================================
        public void Apply()
        {
            AzureApi.DeleteContainer(ContainerName);
        }
        //===============================================================
    }

    internal class AzureModify<T> : Modify<T>
    {
        //===============================================================
        public AzureModify(T value, IEnumerable<Object> keys, Action<T> modifier, AzureApi azureApi, String containerName)
            : base(value, modifier)
        {
            AzureApi = azureApi;
            ContainerName = containerName;
            Keys = keys;
        }
        //===============================================================
        private IEnumerable<Object> Keys { get; set; }
        //===============================================================
        private AzureApi AzureApi { get; set; }
        //===============================================================
        private String ContainerName { get; set; }
        //===============================================================
        public override void Apply()
        {
            Modifier(Value);
            AzureApi.StoreObject(Value, Keys, ContainerName);
        }
        //===============================================================
    }
}
