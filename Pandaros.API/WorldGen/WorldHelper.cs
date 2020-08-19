using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.WorldGen
{
    public static class WorldHelper
    {
        /// <summary>
        ///     Gets all non air blocks in an area
        /// </summary>
        /// <param name="w"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Dictionary<Vector3Int, ItemTypes.ItemType> GetBlocksInArea(Vector3Int min, Vector3Int max)
        {
            Dictionary<Vector3Int, ItemTypes.ItemType> retval = new Dictionary<Vector3Int, ItemTypes.ItemType>();
            int xMin = min.x > max.x ? max.x : min.x;
            int xMax = min.x < max.x ? max.x : min.x;
            int yMin = min.y > max.y ? max.y : min.y;
            int yMax = min.y < max.y ? max.y : min.y;
            int zMin = min.z > max.z ? max.z : min.z;
            int zMax = min.z < max.z ? max.z : min.z;


            for (int Y = yMin; Y < yMax; Y++)
            {
                for (int Z = zMin; Z < zMax; Z++)
                {
                    for (int X = xMin; X < xMax; X++)
                    {
                        var pos = new Vector3Int(X, Y, Z);
                        if (World.TryGetTypeAt(pos, out ItemTypes.ItemType worldType) && worldType != ColonyBuiltIn.ItemTypes.AIR)
                            retval[pos] = worldType;
                    }
                }
            }

            return retval;
        }
    }
}
