using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class OnSendAreaHighlightsProvider : IOnSendAreaHighlightsExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        List<IOnSendAreaHighlights> _activatedInstances = new List<IOnSendAreaHighlights>();

        public string InterfaceName => nameof(IOnSendAreaHighlights);
        public Type ClassType => null;

        public void OnSendAreaHighlights(Players.Player player, List<AreaJobTracker.AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            if (_activatedInstances.Count == 0 && LoadedAssembalies.Count != 0)
                foreach (var s in LoadedAssembalies)
                    if (Activator.CreateInstance(s) is IOnSendAreaHighlights cb)
                        _activatedInstances.Add(cb);

            foreach (var cb in _activatedInstances)
                    try
                    {
                    cb.OnSendAreaHighlights(player, list, showWhileHoldingTypes);
                    }
                    catch (Exception ex)
                    {
                        APILogger.LogError(ex);
                    }
        }
    }
}
