using System;
using System.Collections.Generic;

namespace Pandaros.API.Extender
{
    public interface IPandarosExtention
    {
        List<Type> LoadedAssembalies { get; }

        string InterfaceName { get; }

        Type ClassType { get; }
    }
}
