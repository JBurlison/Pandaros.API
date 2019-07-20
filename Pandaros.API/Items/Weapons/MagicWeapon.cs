using Pandaros.API.Models;
using System.Collections.Generic;

namespace Pandaros.API.Items.Weapons
{
    public class MagicWeapon : CSType, IWeapon
    {
        public virtual int WepDurability { get; set; }

        public virtual float HPTickRegen { get; set; }

        public virtual float MissChance { get; set; }

        public virtual DamageType ElementalArmor { get; set; } = DamageType.Physical;

        public virtual Dictionary<DamageType, float> AdditionalResistance { get; set; } = new Dictionary<DamageType, float>();

        public virtual float Luck { get; set; }

        public virtual float Skilled { get; set; }

        public virtual bool IsMagical { get; set; }

        public virtual Dictionary<DamageType, float> Damage { get; set; } = new Dictionary<DamageType, float>();

        public void Update()
        {

        }
    }
}
