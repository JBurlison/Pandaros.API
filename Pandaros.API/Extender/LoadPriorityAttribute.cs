using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender
{
    public class LoadPriorityAttribute : Attribute
    {
        public double Priority { get; set; }

        public LoadPriorityAttribute(double priority)
        {
            Priority = priority;
        }
    }
}
