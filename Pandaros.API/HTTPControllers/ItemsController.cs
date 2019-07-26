using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;

namespace Pandaros.API.HTTPControllers
{
    public class ItemsController : IPandaController
    {
        [PandaHttp(OperationType.Get, "/Items/All", "Gets All items registered with the Colony Survival server.")]
        public RestResponse GetAllItems()
        {
            return new RestResponse() { Content = ItemTypes._TypeByUShort.Values.ToUTF8SerializedJson() };
        }

        [PandaHttp(OperationType.Get, "/Items/GetById", "Gets a specific item by its ItemId.")]
        public RestResponse GetItemById(ushort itemId)
        {
            if (ItemTypes.TryGetType(itemId, out ItemTypes.ItemType itemType))
            {
                return new RestResponse() { Content = itemType.ToUTF8SerializedJson() };
            }
            else
                return RestResponse.BlankJsonObject;
        }

        [PandaHttp(OperationType.Get, "/Items/GetByName", "Gets a specific item by its Name.")]
        public RestResponse GetItemByName(string name)
        {
            if (ItemTypes.TryGetType(name, out ItemTypes.ItemType itemType))
            {
                return new RestResponse() { Content = itemType.ToUTF8SerializedJson() };
            }
            else
                return RestResponse.BlankJsonObject;
        }
    }
}
