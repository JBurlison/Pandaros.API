using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models
{

    public class CSGuardJobSettings : ICSGuardJobSettings
    {
        public virtual string blockType { get; set; }
        public virtual int cooldownShot { get; set; }
        public virtual int damage { get; set; }
        public virtual string jobType { get; set; }
        public virtual string npcType { get; set; }
        public virtual string onHitAudio { get; set; }
        public virtual string onShootAudio { get; set; }
        public virtual int range { get; set; }
        public virtual IRecruitmentitem recruitmentItem { get; set; }
        public virtual IShootrequirement[] shootRequirements { get; set; }
        public virtual string sleepType { get; set; }
    }

    public class Recruitmentitem : IRecruitmentitem
    {
        public virtual string type { get; set; }
    }

    public class Shootrequirement : IShootrequirement
    {
        public virtual string type { get; set; }
    }
}
