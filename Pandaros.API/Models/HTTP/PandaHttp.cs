using Microsoft.OpenApi.Models;
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
        public OperationType RestVerb { get; set; }
        public string Description { get; set; }

        public PandaHttp(OperationType verb, string route, string description)
        {
            RestVerb = verb;
            Route = route;
            Description = description;
        }
    }
}
