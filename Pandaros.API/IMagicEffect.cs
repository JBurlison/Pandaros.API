using Pandaros.API.Extender;
using Pandaros.API.Items;
using Pandaros.API.Models;

namespace Pandaros.API
{
    public interface IMagicEffect : IPandaArmor, IPandaDamage, INameable, ILucky
    {
        bool IsMagical { get; set; }
        float Skilled { get; set; }
        float HPTickRegen { get; }
        void Update();
    }
}