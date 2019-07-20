using Pandaros.API.Extender;
using Pandaros.API.Models;
using System;
using System.Collections.Generic;

namespace Pandaros.API.Jobs.Roaming
{
    public interface IRoamingJobObjective
    {
        float WorkTime { get; }
        ItemId ItemIndex { get; }
        Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; }
        string ObjectiveCategory { get; }
        float WatchArea { get; }
        void DoWork(Colony colony, RoamingJobState state);
    }

    public interface IRoamingJobObjectiveAction : INameable
    {
        float ActionEnergyMinForFix { get; }
        float TimeToPreformAction { get; }
        string AudioKey { get; }
        ItemId ObjectiveLoadEmptyIcon { get; }
        ItemId PreformAction(Colony colony, RoamingJobState state);
    }
}