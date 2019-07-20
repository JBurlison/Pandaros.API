using Pandaros.API.Models;
using System.Collections.Generic;

namespace Pandaros.API.Items.ConnectedBlocks
{
    public class PipeCalculationType : IConnectedBlockCalculationType
    {
        public List<BlockSide> AvailableBlockSides => new List<BlockSide>()
        {
            BlockSide.Xn,
            BlockSide.Xp,
            BlockSide.Zn,
            BlockSide.Zp,
            BlockSide.Yp,
            BlockSide.Yn
        };

        public List<RotationAxis> AxisRotations => new List<RotationAxis>()
        {
            RotationAxis.X,
            RotationAxis.Y,
            RotationAxis.Z
        };

        public string name => "Pipe";

        public int MaxConnections => 6;
    }
}
