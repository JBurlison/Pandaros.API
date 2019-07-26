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
    public class LocalizationController : IPandaController
    {
        [PandaHttp(OperationType.Get, "/Localization/ItemName", "")]
        public RestResponse GetItemByName(string name)
        {
            if (ItemTypes.TryGetType(name, out ItemTypes.ItemType itemType))
            {
                Dictionary<string, LocalizedItem> translations = new Dictionary<string, LocalizedItem>();

                foreach (var localization in Localization.LocaleTexts)
                {
                    LocalizedItem localizedItem = new LocalizedItem();

                    if (Localization.TryGetType(localization.Key, itemType, out string localized))
                        localizedItem.Name = localized;

                    if (Localization.TryGetTypeUse(localization.Key, itemType, out string localizeduse))
                        localizedItem.Description = localizeduse;

                    translations[localization.Key] = localizedItem;
                }

                return new RestResponse() { Content = translations.ToUTF8SerializedJson() };
            }
            else
                return RestResponse.BlankJsonObject;
        }


    }
}
