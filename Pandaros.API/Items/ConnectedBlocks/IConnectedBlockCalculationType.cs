using Pandaros.API.Models;
using System.Collections.Generic;

namespace Pandaros.API.Items
{
    public interface IConnectedBlockCalculationType : INameable
    {
        List<BlockSide> AvailableBlockSides { get; }
        List<RotationAxis> AxisRotations { get; }
        int MaxConnections { get; }
    }
}
