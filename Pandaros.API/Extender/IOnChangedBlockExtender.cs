﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender
{
    public interface IOnChangedBlockExtender : IPandarosExtention
    {
        void OnChangedBlock(ModLoader.OnTryChangeBlockData changeBlockData);
    }
}
