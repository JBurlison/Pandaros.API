using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models
{
    public interface ICSBlockJobSettings
    {
        string blockType { get; set; }
        int cooldown { get; set; }
        string jobType { get; set; }
        int maxCraftsPerHaul { get; set; }
        string npcType { get; set; }
    }
}
