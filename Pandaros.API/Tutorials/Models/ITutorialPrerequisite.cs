using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Tutorials.Models
{
    public interface ITutorialPrerequisite
    {
        bool MeetsCondition(Players.Player p);
    }
}
