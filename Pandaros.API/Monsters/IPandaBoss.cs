namespace Pandaros.API.Monsters
{
    public interface IPandaBoss : IPandaZombie
    {
        string AnnouncementText { get; }
        string DeathText { get; }
        string AnnouncementAudio { get; }
        float ZombieMultiplier { get; }
        bool KilledBefore { get; set; }
    }
}