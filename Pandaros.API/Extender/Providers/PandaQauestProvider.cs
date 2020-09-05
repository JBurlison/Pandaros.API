using Pandaros.API.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pandaros.API.Questing.Models;
using Pandaros.API.Questing.BuiltinQuests;

namespace Pandaros.API.Extender.Providers
{
    public class PandaQuestProvider : IAfterWorldLoadExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IPandaQuest);

        public Type ClassType => typeof(GenericQuest);

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Panda Quests Loaded----------------------");
            var i = 0;

            foreach (var quest in LoadedAssembalies)
            {
                if (Activator.CreateInstance(quest) is IPandaQuest pandaQuest &&
                    !(pandaQuest is IPandaQuest) &&
                    !string.IsNullOrEmpty(pandaQuest.QuestKey))
                {
                    sb.Append($"{pandaQuest.QuestKey}, ");
                    Questing.QuestingSystem.QuestPool[pandaQuest.QuestKey] = pandaQuest;
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
