using Pandaros.API.Models;
using System.Collections.Generic;

namespace Pandaros.API.Items.ConnectedBlocks
{
    public class FenceCalculationType : IConnectedBlockCalculationType
    {
        public List<BlockSide> AvailableBlockSides => new List<BlockSide>()
        {
            BlockSide.Xn,
            BlockSide.Xp,
            BlockSide.Zn,
            BlockSide.Zp
        };

        public string name => "Fence";

        public int MaxConnections => 2;

        public List<RotationAxis> AxisRotations => new List<RotationAxis>()
        {
            RotationAxis.Y
        };
    }
}
