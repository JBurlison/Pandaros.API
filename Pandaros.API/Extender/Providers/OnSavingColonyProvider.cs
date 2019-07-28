using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pipliz.JSON;

namespace Pandaros.API.Extender.Providers
{
    public class OnSavingColonyProvider : IOnSavingColonyExtender
    {
        public List<Type> LoadedAssembalies { get; set; } = new List<Type>();

        public string InterfaceName => nameof(IOnSavingColony);

        public Type ClassType => null;

        public List<IOnSavingColony> OnSavingColonies { get; set; } = new List<IOnSavingColony>();

        public void OnSavingColony(Colony c, JSONNode n)
        {
            if (OnSavingColonies.Count == 0)
                foreach (var assembly in LoadedAssembalies)
                    if (assembly is IOnSavingColony onSavingColony)
                        OnSavingColonies.Add(onSavingColony);


            foreach (var onsaving in OnSavingColonies)
                onsaving.OnSavingColony(c, n);
        }
    }
}
