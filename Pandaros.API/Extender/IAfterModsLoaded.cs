﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender
{
    public interface IAfterModsLoaded
    {
        void AfterModsLoaded(List<ModLoader.ModDescription> list);
    }
}
