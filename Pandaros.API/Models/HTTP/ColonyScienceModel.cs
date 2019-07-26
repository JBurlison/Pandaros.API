using Science;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class ColonyScienceModel
    {
        public List<uint> CompletedScience { get; set; } = new List<uint>();
        public Dictionary<uint, float> CompletedCycles { get; set; } = new Dictionary<uint, float>();
    }

    
}
