using Pandaros.API.Monsters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class BossesProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IPandaBoss);

        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Bosses Loaded----------------------");
            var i = 0;

            foreach (var monster in LoadedAssembalies)
            {
                if (Activator.CreateInstance(monster) is IPandaBoss pandaBoss &&
                    !string.IsNullOrEmpty(pandaBoss.name))
                {
                    sb.Append($"{pandaBoss.name}, ");
                    MonsterManager.AddBoss(pandaBoss);
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
