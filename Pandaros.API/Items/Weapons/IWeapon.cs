namespace Pandaros.API.Items.Weapons
{
    public interface IWeapon : IMagicEffect
    {
        int WepDurability { get; set; }
    }
}