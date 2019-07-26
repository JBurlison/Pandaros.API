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
        [PandaHttp(OperationType.Get, "/Localization/ItemName", "Localizes a item by name. Returns all localization for that item with localized descriptions.")]
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

        [PandaHttp(OperationType.Get, "/Localization/Sentence", "Gets the localization for a Sentence.")]
        public RestResponse GetLocalizationSentence(string key)
        {
            Dictionary<string, string> translations = new Dictionary<string, string>();

            foreach (var localization in Localization.LocaleTexts)
            {
                if (Localization.TryGetSentence(localization.Key, key, out string result))
                    translations[localization.Key] = result;
            }

            return new RestResponse() { Content = translations.ToUTF8SerializedJson() };
        }

        [PandaHttp(OperationType.Get, "/Localization/Science", "Gets the localization for a science key.")]
        public RestResponse GetLocalizationScience(string key)
        {
            Dictionary<string, ScienceLocalizationModel> translations = new Dictionary<string, ScienceLocalizationModel>();

            foreach (var localization in Localization.LocaleTexts)
            {
                var model = new ScienceLocalizationModel();

                if (Localization.TryGetSentence(localization.Key, key + ".name", out string result))
                    model.Name = result;

                if (Localization.TryGetSentence(localization.Key, key + ".description", out string descript))
                    model.Description = descript;

                translations[localization.Key] = model;
            }

            return new RestResponse() { Content = translations.ToUTF8SerializedJson() };
        }
    }
}
