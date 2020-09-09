using Pandaros.API.Jobs.Roaming;
using Pandaros.API.Tutorials;
using Pandaros.API.Tutorials.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class TutorialProvider : IAfterItemTypesDefinedExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ITutorial);
        public Type ClassType => null;

        public void AfterItemTypesDefined()
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Tutorials Loaded----------------------");
            var i = 0;

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is ITutorial tutorial &&
                    !string.IsNullOrEmpty(tutorial.Name))
                {
                    sb.Append($"{tutorial.Name}, ");
                    TutorialFactory.Tutorials[tutorial.Name] = tutorial;
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
