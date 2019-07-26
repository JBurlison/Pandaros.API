using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class SuccessResponse
    {
        public bool Success { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}
