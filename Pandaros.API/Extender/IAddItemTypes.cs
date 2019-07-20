using System.Collections.Generic;

namespace Pandaros.API.Extender
{
    public interface IAddItemTypes : IPandarosExtention
    {
        void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes);
    }
}
