using Pandaros.API.Models;

namespace Pandaros.API.StatusIcons
{
    public class Poisoned : CSType
    {
        public override string icon { get; set; } = GameInitializer.ICON_PATH + "Poisoned.png";
        public override string name { get; set; } = GameInitializer.NAMESPACE + ".Poisoned";
    }
}
