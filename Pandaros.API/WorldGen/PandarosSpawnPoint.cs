using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pipliz;
using Pipliz.JSON;
using static TerrainGeneration.TerrainGenerator;

namespace Pandaros.API.WorldGen
{
    [ModLoader.ModManager]
    public class PandarosSpawnPoint : ISpawnPointProvider
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameInitializer.NAMESPACE + ".WorldGen.PandarosSpawnPoint.AfterWorldLoad")]
        public void AfterWorldLoad()
        {
            if (ServerManager.TerrainGenerator is TerrainGeneration.TerrainGenerator generator)
            {
                var newSpawnPoint = new PandarosSpawnPoint();
                newSpawnPoint.Default = generator.SpawnPointProvider;
                generator.SpawnPointProvider = newSpawnPoint;
            }
        }

        public ISpawnPointProvider Default { get; set; }

        public Vector3Int GetSpawnPoint()
        {
            var locationJson = APIConfiguration.CSModConfiguration.GetorDefault("SpawnLocation", Vector3Int.invalidPos);

            if (locationJson != Vector3Int.invalidPos)
            {
                return locationJson;
            }
            else
                return Default.GetSpawnPoint();
        }
    }
}
