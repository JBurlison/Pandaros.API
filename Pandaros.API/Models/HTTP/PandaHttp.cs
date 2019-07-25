using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class PandaHttp : Attribute
    {
        public string Route { get; set; }
        public RestVerb RestVerb { get; set; }

        public PandaHttp(RestVerb verb, string route)
        {
            RestVerb = verb;
            Route = route;
        }
    }
}
