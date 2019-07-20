using Pandaros.API.Models;

namespace Pandaros.API.StatusIcons
{
    public class Waiting : CSType
    {
        public override string icon { get; set; } = GameInitializer.ICON_PATH + "Waiting.png";
        public override string name { get; set; } = GameInitializer.NAMESPACE + ".Waiting";
    }
}
