using Pandaros.API.Items;
using Pandaros.API.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class TextureMappingProvider : IAfterSelectedWorld
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSTextureMapping);
        public Type ClassType => null;

        public void AfterSelectedWorld()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Texture Mapping Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSTextureMapping texture &&
                    !string.IsNullOrEmpty(texture.name))
                {
                    ItemTypesServer.SetTextureMapping(texture.name, new ItemTypesServer.TextureMapping(texture.JsonSerialize()));
                    sb.Append($"{texture.name}, ");
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            APILogger.LogToFile(sb.ToString());
            APILogger.LogToFile("---------------------------------------------------------");
        }
    }
}
