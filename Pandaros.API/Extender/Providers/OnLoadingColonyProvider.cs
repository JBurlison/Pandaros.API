using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pipliz.JSON;

namespace Pandaros.API.Extender.Providers
{
    public class OnLoadingColonyProvider : IOnLoadingColonyExtender
    {
        public List<Type> LoadedAssembalies { get; set; } = new List<Type>();

        public string InterfaceName => nameof(IOnLoadingColony);

        public Type ClassType => null;

        public List<IOnLoadingColony> OnLoadingColonies { get; set; } = new List<IOnLoadingColony>();

        public void OnLoadingColony(Colony c, JSONNode n)
        {
            if (OnLoadingColonies.Count == 0)
                foreach (var assembly in LoadedAssembalies)
                    if (assembly is IOnLoadingColony onloadingColony)
                        OnLoadingColonies.Add(onloadingColony);


            foreach (var onsaving in OnLoadingColonies)
                onsaving.OnLoadingColony(c, n);
        }
    }
}
