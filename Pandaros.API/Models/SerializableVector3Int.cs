using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models
{
    public class SerializableVector3Int : IEquatable<SerializableVector3Int>, IComparable<SerializableVector3Int>, IEqualityComparer<SerializableVector3Int>
    {
        public SerializableVector3Int() { }

        public SerializableVector3Int(int X, int Y, int Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public SerializableVector3Int(Vector3Int vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public SerializableVector3Int(Vector3Int? vector3)
        {
            if (vector3 == null || !vector3.HasValue)
                return;

            x = vector3.Value.x;
            y = vector3.Value.y;
            z = vector3.Value.z;
        }


        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }

        public static implicit operator Vector3Int(SerializableVector3Int serializableVector3)
        {
            return new Vector3Int(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public static implicit operator UnityEngine.Vector3Int(SerializableVector3Int serializableVector3)
        {
            return new Vector3Int(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public static implicit operator SerializableVector3Int(Vector3Int serializableVector3)
        {
            return new SerializableVector3Int(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public static implicit operator SerializableVector3Int(UnityEngine.Vector3Int serializableVector3)
        {
            return new SerializableVector3Int(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public bool Equals(SerializableVector3Int other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public int CompareTo(SerializableVector3Int other)
        {
            int num = x - other.x;
            if (num != 0)
            {
                return num;
            }

            num = y - other.y;
            if (num != 0)
            {
                return num;
            }

            return z - other.z;
        }

        public bool Equals(SerializableVector3Int x, SerializableVector3Int other)
        {
            return x.x == other.x && x.y == other.y && x.z == other.z;
        }

        public int GetHashCode(SerializableVector3Int obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return ((x + 18) * 31 + y) * 13 + z;
        }
    }
}
