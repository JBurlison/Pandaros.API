using System.Collections.Generic;
using NPC;
using Pandaros.API.Extender;

namespace Pandaros.API.Models
{
    public interface IPlayerMagicItem : ICSType, IMagicEffect
    {
        float MovementSpeed { get; }
        float JumpPower { get; }
        float FlySpeed { get; }
        float MoveSpeed { get; }
        float FallDamage { get; }
        float FallDamagePerUnit { get; }
        float BuildDistance { get; }
        float Gravity { get; }
    }
}