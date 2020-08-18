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

            for (int Y = min.y; Y < max.y; Y++)
            {
                for (int Z = min.z; Z < max.z; Z++)
                {
                    for (int X = min.x; X < max.x; X++)
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
