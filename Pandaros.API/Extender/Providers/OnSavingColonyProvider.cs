using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pipliz.JSON;

namespace Pandaros.API.Extender.Providers
{
    public class OnSavingColonyProvider : IOnSavingColonyExtnder
    {
        public List<Type> LoadedAssembalies { get; set; } = new List<Type>();

        public string InterfaceName => nameof(IOnSavingColony);

        public Type ClassType => null;

        public void OnSavingColony(Colony c, JSONNode n)
        {
            
        }
    }
}
