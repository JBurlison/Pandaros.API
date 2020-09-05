using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models
{
    public interface ICSGuardJobSettings
    {
        string blockType { get; set; }
        int cooldownShot { get; set; }
        int damage { get; set; }
        string jobType { get; set; }
        string npcType { get; set; }
        string onHitAudio { get; set; }
        string onShootAudio { get; set; }
        int range { get; set; }
        IRecruitmentitem recruitmentItem { get; set; }
        IShootrequirement[] shootRequirements { get; set; }
        string sleepType { get; set; }
    }

    public interface IRecruitmentitem
    {
        string type { get; set; }
    }

    public interface IShootrequirement
    {
        string type { get; set; }
    }
}
