using Pandaros.API.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender.Providers
{
    public class PandaZombieProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IPandaZombie);

        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Panda Zombies Loaded----------------------");
            var i = 0;

            foreach (var monster in LoadedAssembalies)
            {
                if (Activator.CreateInstance(monster) is IPandaZombie pandaZombie &&
                    !(pandaZombie is IPandaBoss) &&
                    !string.IsNullOrEmpty(pandaZombie.name))
                {
                    sb.Append($"{pandaZombie.name}, ");
                    PandaMonsterSpawner.PandaZombies.Add(pandaZombie);
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            APILogger.LogToFile(sb.ToString());
            APILogger.LogToFile("------------------------------------------------------");
        }
    }
}
