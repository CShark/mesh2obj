using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mesh2obj.Ba2Parser.Nif {
    internal class NiObjectNet : INiParser {
        public void Parse(BinaryReader reader) {
            reader.ReadUInt32(); // Name String ID
            int count = reader.ReadInt32(); // Extra Data Count
            for (int i = 0; i < count; i++) {
                reader.ReadInt32(); // Extra Data Ref
            }

            reader.ReadUInt32(); // Controller Obj ID
        }
    }
}