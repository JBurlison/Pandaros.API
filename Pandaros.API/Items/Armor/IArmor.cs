using static Pandaros.API.Items.Armor.ArmorFactory;

namespace Pandaros.API.Items.Armor
{
    public interface IArmor : IMagicEffect
    {
        ArmorSlot Slot { get; }
        float ArmorRating { get; }
        int Durability { get; set; }
    }
}
