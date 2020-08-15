using Pandaros.API.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.API.Extender.Providers
{
    public class AfterItemTypesDefinedProvider : IAfterItemTypesDefinedExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IAfterItemTypesDefined);
        public Type ClassType => null;

        public void AfterItemTypesDefined()
        {
            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IAfterItemTypesDefined afterItemTypes)
                {
                    try
                    {
                        afterItemTypes.AfterItemTypesDefined();
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
