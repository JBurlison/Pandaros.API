﻿using Pandaros.API.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models
{
    public enum BlockSide
    {
        [BlockSideVectorValues(0, 0, 0)]
        Invalid,
        [BlockSideVectorValues(1, 1, 0, Xp)]
        XpYp,
        [BlockSideVectorValues(1, -1, 0, Xp)]
        XpYn,
        [BlockSideVectorValues(0, 1, 1, Zp)]
        ZpYp,
        [BlockSideVectorValues(0, -1, 1, Zp)]
        ZpYn,
        [BlockSideVectorValues(-1, 1, 0, Xn)]
        XnYp,
        [BlockSideVectorValues(-1, -1, 0, Xn)]
        XnYn,
        [BlockSideVectorValues(0, 1, -1, Zn)]
        ZnYp,
        [BlockSideVectorValues(0, -1, -1, Zn)]
        ZnYn,
        [BlockSideVectorValues(0, 1, 0)]
        Yp,
        [BlockSideVectorValues(0, -1, 0)]
        Yn,
        [BlockSideVectorValues(1, 0, 0, XpYp, XpYn)]
        Xp,
        [BlockSideVectorValues(-1, 0, 0, XnYp, XnYn)]
        Xn,
        [BlockSideVectorValues(0, 0, 1, ZpYp, ZpYn)]
        Zp,
        [BlockSideVectorValues(0, 0, -1, ZnYp, ZnYn)]
        Zn
    }
}
