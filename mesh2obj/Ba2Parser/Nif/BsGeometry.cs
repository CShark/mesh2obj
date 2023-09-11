using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mesh2obj.Ba2Parser.Nif {
    internal class BsGeometry : NiAVObject {
        public string MeshReference { get; private set; }

        public override void Parse(BinaryReader reader) {
            base.Parse(reader);

            // Skip unknown properties
            reader.ReadBytes(65);
            var len = reader.ReadInt32();
            MeshReference = new string(reader.ReadChars(len));
        }
    }
}