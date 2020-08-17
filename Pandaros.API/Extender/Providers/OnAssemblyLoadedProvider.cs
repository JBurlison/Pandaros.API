using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class OnAssemblyLoadedProvider : IOnAssemblyLoadedExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IOnAssemblyLoaded);
        public Type ClassType => null;

        public void OnAssemblyLoaded(string path)
        {
            foreach(var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IOnAssemblyLoaded afterItemTypes)
                {
                    try
                    {
                        afterItemTypes.OnAssemblyLoaded(path);
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
