using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models
{

    public class CSGuardJobSettings : ICSGuardJobSettings
    {
        public string blockType { get; set; }
        public int cooldownShot { get; set; }
        public int damage { get; set; }
        public string jobType { get; set; }
        public string npcType { get; set; }
        public string onHitAudio { get; set; }
        public string onShootAudio { get; set; }
        public int range { get; set; }
        public IRecruitmentitem recruitmentItem { get; set; }
        public IShootrequirement[] shootRequirements { get; set; }
        public string sleepType { get; set; }
    }

    public class Recruitmentitem : IRecruitmentitem
    {
        public string type { get; set; }
    }

    public class Shootrequirement : IShootrequirement
    {
        public string type { get; set; }
    }
}
