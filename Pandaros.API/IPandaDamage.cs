using System.Collections.Generic;

namespace Pandaros.API
{
    public interface IPandaDamage
    {
        Dictionary<DamageType, float> Damage { get; }
    }
}