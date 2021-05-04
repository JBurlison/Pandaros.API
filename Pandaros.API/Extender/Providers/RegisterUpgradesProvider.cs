using Assets.ColonyPointUpgrades;
using Pandaros.API.Upgrades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender.Providers
{
    public class RegisterUpgradesProvider : IOnRegisterUpgradesExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IPandaUpgrade);

        public Type ClassType => throw new NotImplementedException();

        public void OnRegisterUpgrades(UpgradesManager upgrades)
        {
            StringBuilder sb = new StringBuilder();
            APILogger.LogToFile("-------------------Colony Upgrades Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IPandaUpgrade upgrade)
                {
                    upgrades.RegisterUpgrade(upgrade);

                    sb.Append($"{upgrade.UniqueKey}, ");
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
