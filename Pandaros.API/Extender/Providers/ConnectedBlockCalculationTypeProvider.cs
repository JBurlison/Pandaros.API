using Pandaros.API.Items;
using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class ConnectedBlockCalculationTypeProvider : IAfterModsLoadedExtention
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IConnectedBlockCalculationType);
        public Type ClassType => null;

        public void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Connected Block CalculationType Loaded----------------------");
            var i = 0;

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IConnectedBlockCalculationType connectedBlockCalcType &&
                    !string.IsNullOrEmpty(connectedBlockCalcType.name))
                {
                    sb.Append($"{connectedBlockCalcType.name}, ");
                    ConnectedBlockCalculator.CalculationTypes[connectedBlockCalcType.name] = connectedBlockCalcType;
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            APILogger.LogToFile(sb.ToString());
            APILogger.LogToFile("---------------------------------------------------------");
        }
    }
}
