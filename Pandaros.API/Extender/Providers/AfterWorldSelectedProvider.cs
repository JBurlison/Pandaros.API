using Pandaros.API.Items;
using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class AfterWorldSelected : IAfterSelectedWorldExtender
    { 
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IAfterSelectedWorld);

        public Type ClassType => null;


        public void AfterSelectedWorld()
        {
            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IAfterSelectedWorld modsLoaded)
                {
                    modsLoaded.AfterSelectedWorld();
                }
            }
        }
    }
}
