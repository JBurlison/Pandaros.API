using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkUI;
using NetworkUI.Items;

namespace Pandaros.API.Extender.Providers
{
    public class OnConstructInventoryManageColonyUIProvider : IOnConstructInventoryManageColonyUIExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IOnConstructInventoryManageColonyUI);

        public Type ClassType => null;

        List<IOnConstructInventoryManageColonyUI> _onConstructInventoryManageColonyUIs = new List<IOnConstructInventoryManageColonyUI>();

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu, (Table, Table) table)
        {
            if (_onConstructInventoryManageColonyUIs.Count == 0)
                foreach(var type in LoadedAssembalies)
                    _onConstructInventoryManageColonyUIs.Add((IOnConstructInventoryManageColonyUI)Activator.CreateInstance(type));

            foreach (var ui in _onConstructInventoryManageColonyUIs)
                try
                {
                    ui.OnConstructInventoryManageColonyUI(player, networkMenu, table);
                }
                catch  (Exception ex)
                {
                    APILogger.LogError(ex);
                }
        }
    }
}
