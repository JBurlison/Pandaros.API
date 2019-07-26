using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;

namespace Pandaros.API.HTTPControllers
{
    public class ItemsController : IPandaController
    {
        [PandaHttp(RestVerb.Get, "/Items/All")]
        public RestResponse GetAllItems()
        {
            return new RestResponse() { Content = ItemTypes._TypeByUShort.Values.ToUTF8SerializedJson() };
        }

        [PandaHttp(RestVerb.Get, "/Items/GetById")]
        public RestResponse GetItemById(ushort itemId)
        {
            if (ItemTypes.TryGetType(itemId, out ItemTypes.ItemType itemType))
            {
                return new RestResponse() { Content = itemType.ToUTF8SerializedJson() };
            }
            else
                return RestResponse.BlankJsonObject;
        }

        [PandaHttp(RestVerb.Get, "/Items/GetByName")]
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
