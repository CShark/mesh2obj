using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mesh2obj.Ba2Parser.Nif {
    internal interface INiParser {
        public void Parse(BinaryReader reader);
    }
}