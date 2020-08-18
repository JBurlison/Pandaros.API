using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class OnChangedBlockProvider : IOnChangedBlockExtender
    { 
        public List<Type> LoadedAssembalies { get; } = new List<Type>();
        List<IOnChangedBlock> _activatedInstances = new List<IOnChangedBlock>();

        public string InterfaceName => nameof(IOnChangedBlock);
        public Type ClassType => null;

        public void OnChangedBlock(ModLoader.OnTryChangeBlockData tryChangeBlockData)
        {
            if (_activatedInstances.Count == 0 && LoadedAssembalies.Count != 0)
                foreach (var s in LoadedAssembalies)
                    if (Activator.CreateInstance(s) is IOnChangedBlock cb)
                        _activatedInstances.Add(cb);

            foreach (var cb in _activatedInstances)
                try
                {
                    cb.OnChangedBlock(tryChangeBlockData);
                }
                catch (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }
    }
}
