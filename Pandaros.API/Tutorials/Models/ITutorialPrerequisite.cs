using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Tutorials.Models
{
    public interface ITutorialPrerequisite
    {
        string Name { get; }
        bool MeetsCondition(Players.Player p);
    }
}
