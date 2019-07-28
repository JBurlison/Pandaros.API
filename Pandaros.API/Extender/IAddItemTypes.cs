using System.Collections.Generic;

namespace Pandaros.API.Extender
{
    public interface IAddItemTypesExtender : IPandarosExtention
    {
        void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes);
    }
}
