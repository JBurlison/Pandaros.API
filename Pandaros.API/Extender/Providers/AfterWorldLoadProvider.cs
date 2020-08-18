using Pandaros.API.Items;
using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class AfterWorldLoadProvider : IAfterWorldLoadExtender
    { 
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IAfterWorldLoad);

        public Type ClassType => null;


        public void AfterWorldLoad()
        {
            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IAfterWorldLoad modsLoaded)
                {
                    modsLoaded.AfterWorldLoad();
                }
            }
        }
    }
}
