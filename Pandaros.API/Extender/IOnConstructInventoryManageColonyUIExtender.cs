using NetworkUI;
using NetworkUI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Extender
{
    public interface IOnConstructInventoryManageColonyUIExtender  : IPandarosExtention
    {
        void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu, (Table, Table) table);
    }
}
