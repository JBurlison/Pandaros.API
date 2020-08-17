using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class OnTryChangeBlockProvider : IOnTryChangeBlockExtender
    { 
        public List<Type> LoadedAssembalies { get; } = new List<Type>();
        List<IOnTryChangeBlock> _activatedInstances = new List<IOnTryChangeBlock>();

        public string InterfaceName => nameof(IOnTryChangeBlock);
        public Type ClassType => null;

        public void OnTryChangeBlock(ModLoader.OnTryChangeBlockData tryChangeBlockData)
        {
            if (_activatedInstances.Count == 0 && LoadedAssembalies.Count != 0)
                foreach (var s in LoadedAssembalies)
                    if (Activator.CreateInstance(s) is IOnTryChangeBlock cb)
                        _activatedInstances.Add(cb);

            foreach (var cb in _activatedInstances)
                try
                {
                    cb.OnTryChangeBlock(tryChangeBlockData);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }
    }
}
