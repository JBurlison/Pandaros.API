using Pandaros.API.Extender;
using System.Collections.Generic;

namespace Pandaros.API
{
    public interface IPandaArmor : INameable
    {
        float MissChance { get; }
        DamageType ElementalArmor { get; }
        Dictionary<DamageType, float> AdditionalResistance { get; }
    }
}