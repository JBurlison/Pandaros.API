using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class OnTryChangeBlockProvider : IOnTryChangeBlockExtender
    { 
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IOnTryChangeBlock);
        public Type ClassType => null;

        public void OnTryChangeBlock(ModLoader.OnTryChangeBlockData tryChangeBlockData)
        {
            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IOnTryChangeBlock afterItemTypes)
                {
                    try
                    {
                        afterItemTypes.OnTryChangeBlock(tryChangeBlockData);
                    }
                    catch (Exception ex)
                    {
                        APILogger.LogError(ex);
                    }
                }
            }
        }
    }
}
