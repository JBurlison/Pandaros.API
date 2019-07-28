using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class RoamingJobObjectiveProvider : IAfterItemTypesDefinedExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IRoamingJobObjective);
        public Type ClassType => null;

        public void AfterItemTypesDefined()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Roaming Job Objective Loaded----------------------");
            var i = 0;

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IRoamingJobObjective roamingJobObjective &&
                    !string.IsNullOrEmpty(roamingJobObjective.ItemIndex))
                {
                    sb.Append($"{roamingJobObjective.ItemIndex}, ");
                    RoamingJobManager.RegisterObjectiveType(roamingJobObjective);
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
