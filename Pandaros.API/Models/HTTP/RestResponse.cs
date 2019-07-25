using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class RestResponse
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; } = "application/json";
    }
}
