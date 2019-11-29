using Happiness;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender.Providers
{
    class HappinessCausesProvider : IOnColonyCreatedExtender, IOnLoadingColonyExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName { get; } = nameof(IHappinessCause);

        public Type ClassType => null;

        public void ColonyCreated(Colony c)
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Happiness Cause Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IHappinessCause cause)
                {
                    c.HappinessData.HappinessCauses.Add(cause);
                    sb.Append($"{cause.GetType().Name}, ");
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

        public void OnLoadingColony(Colony c, JSONNode n)
        {
            ColonyCreated(c);
        }
    }
}