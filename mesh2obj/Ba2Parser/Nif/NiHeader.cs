using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace mesh2obj.Ba2Parser.Nif {
    internal struct NiBlockMetadata {
        public string Type { get; }
        public uint Size { get; }
        public long Offset { get; }

        public NiBlockMetadata(string type, uint size, long offset) {
            Type = type;
            Size = size;
            Offset = offset;
        }
    }

    internal class NiHeader : INiParser {
        private List<string> _blockTypes = new();
        private List<NiBlockMetadata> _blocks = new();

        public IReadOnlyList<string> BlockTypes => _blockTypes.AsReadOnly();
        public IReadOnlyList<NiBlockMetadata> Blocks => _blocks.AsReadOnly();

        public uint BlockCount { get; private set; }

        public void Parse(BinaryReader reader) {
            List<short> blockTypeIndex = new();
            List<uint> blockSize = new();

            // skip header
            while (reader.ReadByte() != 0x0A) {
            }

            reader.ReadUInt32(); // Version
            reader.ReadByte(); // EndianType
            reader.ReadUInt32(); // User Version
            var numBlocks = reader.ReadUInt32();

            // Some bethesda stream stuff
            reader.ReadUInt32(); // BS Version
            int count = reader.ReadByte(); // Author
            reader.ReadBytes(count);
            reader.ReadUInt32(); // Int?
            count = reader.ReadByte();
            reader.ReadBytes(count);
            count = reader.ReadByte();
            reader.ReadBytes(count);

            var numBlockTypes = reader.ReadUInt16();
            for (int i = 0; i < numBlockTypes; i++) {
                count = reader.ReadInt32();
                _blockTypes.Add(new string(reader.ReadChars(count)));
            }

            for (int i = 0; i < numBlocks; i++) {
                blockTypeIndex.Add(reader.ReadInt16());
            }

            for (int i = 0; i < numBlocks; i++) {
                blockSize.Add(reader.ReadUInt32());
            }

            var strings = reader.ReadUInt32();
            reader.ReadUInt32(); // Max String length
            for (int i = 0; i < strings; i++) {
                count = reader.ReadInt32();
                reader.ReadBytes(count);
            }

            count = reader.ReadInt32(); // Num Groups
            for (int i = 0; i < count; i++) {
                reader.ReadUInt32();
            }

            long offset = reader.BaseStream.Position;

            for (int i = 0; i < blockTypeIndex.Count; i++) {
                _blocks.Add(new NiBlockMetadata(BlockTypes[blockTypeIndex[i]], blockSize[i], offset));
                offset += blockSize[i];
            }

            BlockCount = numBlocks;
        }
    }
}