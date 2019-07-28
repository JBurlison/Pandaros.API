using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Questing.Models
{
    public interface IPandaQuestPrerequisite
    {
        string GetPrerequisiteText(IPandaQuest quest, Colony colony, Players.Player player);
        bool MeetsPrerequisite(IPandaQuest quest, Colony colony);
    }
}
