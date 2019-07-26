using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class ColonyJobs
    {
        public Dictionary<int, int> OpenJobsByTypeId { get; set; }
        public Dictionary<int, int> TakenJobsByTypeId { get; set; }
    }
}
