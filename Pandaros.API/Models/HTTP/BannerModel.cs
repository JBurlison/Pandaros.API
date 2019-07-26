using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class BannerModel
    {
        public SerializableVector3 Position { get; set; }
        public int ColonyId { get; set; }
        public int SafeRadius { get; set; }
    }
}
