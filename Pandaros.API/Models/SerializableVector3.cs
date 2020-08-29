using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.API.Models
{
    public class SerializableVector3 : IEquatable<SerializableVector3>
    {
        public SerializableVector3() { }

        public SerializableVector3(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public SerializableVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public SerializableVector3(Vector3? vector3)
        {
            if (vector3 == null || !vector3.HasValue)
                return;

            x = vector3.Value.x;
            y = vector3.Value.y;
            z = vector3.Value.z;
        }

        public SerializableVector3(Pipliz.Vector3Int vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public SerializableVector3(Quaternion quant)
        {
            x = quant.x;
            y = quant.y;
            z = quant.z;
        }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public static implicit operator Vector3(SerializableVector3 serializableVector3)
        {
            return new Vector3(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public static implicit operator Pipliz.Vector3Int(SerializableVector3 serializableVector3)
        {
            return new Pipliz.Vector3Int(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public static implicit operator Quaternion(SerializableVector3 serializableVector3)
        {
            return Quaternion.Euler(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public bool Equals(SerializableVector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public bool Equals(SerializableVector3 x, SerializableVector3 other)
        {
            return x.x == other.x && x.y == other.y && x.z == other.z;
        }

    }
}
