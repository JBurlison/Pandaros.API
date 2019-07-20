using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender
{
    public interface IAfterModsLoadedExtention : IPandarosExtention
    {
        void AfterModsLoaded(List<ModLoader.ModDescription> list);
    }
}
