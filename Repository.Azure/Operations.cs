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
        public AzureInsert(IEnumerable<Object> keys, T value, AzureContainerInterface<T> azureContainerInterface)
            : base(keys, value)
        {
            AzureContainerInterface = azureContainerInterface;
        }
        //===============================================================
        private AzureContainerInterface<T> AzureContainerInterface { get; set; }
        //===============================================================
        public override void Apply()
        {
            AzureContainerInterface.StoreObject(Value, Keys);
        }
        //===============================================================
    }

    internal class AzureRemove<T> : Remove
    {
        //===============================================================
        public AzureRemove(IEnumerable<Object> keys, AzureContainerInterface<T> azureContainerInterface)
            : base(keys)
        {
            AzureContainerInterface = azureContainerInterface;
        }
        //===============================================================
        private AzureContainerInterface<T> AzureContainerInterface { get; set; }
        //===============================================================
        public override void Apply()
        {
            AzureContainerInterface.DeleteObject(Keys);
        }
        //===============================================================
    }

    internal class AzureRemoveAll<T> : IPendingChange
    {
        //===============================================================
        public AzureRemoveAll(AzureContainerInterface<T> azureContainerInterface)
        {
            AzureContainerInterface = azureContainerInterface;
        }
        //===============================================================
        private AzureContainerInterface<T> AzureContainerInterface { get; set; }
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
        public AzureModify(T value, IEnumerable<Object> keys, Action<T> modifier, AzureContainerInterface<T> azureContainerInterface)
            : base(value, modifier)
        {
            AzureContainerInterface = azureContainerInterface;
            Keys = keys;
        }
        //===============================================================
        private IEnumerable<Object> Keys { get; set; }
        //===============================================================
        private AzureContainerInterface<T> AzureContainerInterface { get; set; }
        //===============================================================
        public override void Apply()
        {
            Modifier(Value);
            AzureContainerInterface.StoreObject(Value, Keys);
        }
        //===============================================================
    }
}
