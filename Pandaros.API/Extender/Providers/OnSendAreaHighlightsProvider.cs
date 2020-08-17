using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class OnSendAreaHighlightsProvider : IOnSendAreaHighlightsExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IOnSendAreaHighlights);
        public Type ClassType => null;

        public void OnSendAreaHighlights(Players.Player player, List<AreaJobTracker.AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IOnSendAreaHighlights afterItemTypes)
                {
                    try
                    {
                        afterItemTypes.OnSendAreaHighlights(player, list, showWhileHoldingTypes);
                    }
                    catch (Exception ex)
                    {
                        APILogger.LogError(ex);
                    }
                }
            }
        }
    }
}
