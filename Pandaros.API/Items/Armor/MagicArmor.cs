using Pandaros.API.Models;

namespace Pandaros.API.Items.Armor
{
    public class MagicArmor : PlayerMagicItem, IArmor
    {
        public virtual ArmorFactory.ArmorSlot Slot { get; set; }

        public virtual float ArmorRating { get; set; }

        public virtual int Durability { get; set; }

        public virtual ItemTypesServer.ItemTypeRaw ItemType { get; set; }
    }
}
