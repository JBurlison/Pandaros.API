using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender
{
    public interface IOnSavingColonyExtender : IPandarosExtention
    {
        void OnSavingColony(Colony c, JSONNode n);
    }

    public interface IOnSavingColony
    {
        void OnSavingColony(Colony c, JSONNode n);
    }
}
