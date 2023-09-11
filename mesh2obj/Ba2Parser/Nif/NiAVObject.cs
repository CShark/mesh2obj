using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace mesh2obj.Ba2Parser.Nif {
    internal class NiAVObject : NiObjectNet {
        public Vector3 Translation { get; private set; }
        public Matrix4x4 Rotation { get; private set; }
        public float Scale { get; private set; }

        public virtual void Parse(BinaryReader reader) {
            base.Parse(reader);

            reader.ReadUInt32(); // Flags

            Translation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            {
                var m11 = reader.ReadSingle();
                var m21 = reader.ReadSingle();
                var m31 = reader.ReadSingle();
                var m12 = reader.ReadSingle();
                var m22 = reader.ReadSingle();
                var m32 = reader.ReadSingle();
                var m13 = reader.ReadSingle();
                var m23 = reader.ReadSingle();
                var m33 = reader.ReadSingle();

                Rotation = new Matrix4x4(
                    m11, m12, m13, 0,
                    m21, m22, m23, 0,
                    m31, m32, m33, 0,
                    0, 0, 0, 1);
            }
            Scale = reader.ReadSingle();

            reader.ReadInt32(); // Collision Object
        }
    }
}