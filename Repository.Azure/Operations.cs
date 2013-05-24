using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Repository.Azure
{
    internal class AzureInsert<T> : Insert<T>
    {
        //===============================================================
        public AzureInsert(IEnumerable<Object> keys, T value, AzureContainerInterface azureContainerInterface)
            : base(keys, value)
        {
            AzureContainerInterface = azureContainerInterface;
        }
        //===============================================================
        private AzureContainerInterface AzureContainerInterface { get; set; }
        //===============================================================
        public override void Apply()
        {
            AzureContainerInterface.StoreObject(Value, Keys);
        }
        //===============================================================
    }

    internal class AzureRemove : Remove
    {
        //===============================================================
        public AzureRemove(IEnumerable<Object> keys, AzureContainerInterface azureContainerInterface)
            : base(keys)
        {
            AzureContainerInterface = azureContainerInterface;
        }
        //===============================================================
        private AzureContainerInterface AzureContainerInterface { get; set; }
        //===============================================================
        public override void Apply()
        {
            AzureContainerInterface.DeleteObject(Keys);
        }
        //===============================================================
    }

    internal class AzureRemoveAll : IPendingChange
    {
        //===============================================================
        public AzureRemoveAll(AzureContainerInterface azureContainerInterface)
        {
            AzureContainerInterface = azureContainerInterface;
        }
        //===============================================================
        private AzureContainerInterface AzureContainerInterface { get; set; }
        //===============================================================
        public void Apply()
        {
            AzureContainerInterface.DeleteContainer();
        }
        //===============================================================
    }

    internal class AzureModify<T> : Modify<T>
    {
        //===============================================================
        public AzureModify(T value, IEnumerable<Object> keys, Action<T> modifier, AzureContainerInterface azureContainerInterface)
            : base(value, modifier)
        {
            AzureContainerInterface = azureContainerInterface;
            Keys = keys;
        }
        //===============================================================
        private IEnumerable<Object> Keys { get; set; }
        //===============================================================
        private AzureContainerInterface AzureContainerInterface { get; set; }
        //===============================================================
        private String ContainerName { get; set; }
        //===============================================================
        public override void Apply()
        {
            Modifier(Value);
            AzureContainerInterface.StoreObject(Value, Keys);
        }
        //===============================================================
    }
}
