using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender
{
    public interface IOnLoadingColonyExtender : IPandarosExtention
    {
        void OnLoadingColony(Colony c, JSONNode n);
    }
}
