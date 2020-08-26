using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender
{
    public interface IOnTimedUpdate
    {
        int NextUpdateTimeMinMs { get; }
        int NextUpdateTimeMaxMs { get; }
        ServerTimeStamp NextUpdateTime { get; set; }
        void OnTimedUpdate();
    }
}
