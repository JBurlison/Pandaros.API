using Pandaros.API.Models;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API
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

        public static Vector3Int GetClosestPosition(this Vector3Int pos, List<SerializableVector3Int> locations)
        {
            var retVal = Vector3Int.invalidPos;
            var currentMin = float.MaxValue;

            foreach (var loc in locations)
            {
                var dis = UnityEngine.Vector3Int.Distance(pos, loc);

                if (dis < currentMin)
                {
                    currentMin = dis;
                    retVal = loc;
                }
            }

            return retVal;
        }


        public static Vector3Int GetClosestPosition(this Vector3Int pos, List<Vector3Int> locations)
        {
            var retVal = Vector3Int.invalidPos;
            var currentMin = float.MaxValue;

            foreach (var loc in locations)
            {
                var dis = UnityEngine.Vector3Int.Distance(pos, loc);

                if (dis < currentMin)
                {
                    currentMin = dis;
                    retVal = loc;
                }
            }

            return retVal;
        }

        public static List<Vector3Int> SortClosestPositions(this Vector3Int pos, IEnumerable<SerializableVector3Int> locations)
        {
            var retVal = new Dictionary<Vector3Int, float>();

            foreach (var loc in locations)
            {
                var dis = UnityEngine.Vector3Int.Distance(pos, loc);

                retVal[loc] = dis;
            }

            return retVal.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        }

        public static List<Vector3Int> SortClosestPositions(this Vector3Int pos, IEnumerable<Vector3Int> locations)
        {
            var retVal = new Dictionary<Vector3Int, float>();

            foreach (var loc in locations)
            {
                var dis = UnityEngine.Vector3Int.Distance(pos, loc);

                retVal[loc] = dis;
            }

            return retVal.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        }
    }
}
