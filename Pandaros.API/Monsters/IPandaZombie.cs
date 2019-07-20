using AI;
using Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Monsters
{
    public interface IPandaZombie : IMonster, IPandaDamage, IPandaArmor, INameable
    {
        float ZombieHPBonus { get; }
        string MosterType { get; }
        int MinColonists { get; }
        IPandaZombie GetNewInstance(Path path, Colony c);
    }
}
