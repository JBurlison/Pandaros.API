using NetworkUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Tutorials.Models
{
    public interface ITutorial
    {
        string Name { get; }
        List<ITutorialPrerequisite> Prerequisites { get; }
        NetworkMenu ShowTutorial(Players.Player p);
    }
}
