using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Elskom.Generic.Libs;

namespace mesh2obj.Ba2Parser.Ba2 {
    internal class Ba2Archive {
        [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Ansi)]
        public struct Header {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string magic;

            public UInt32 version;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string type;

            public UInt32 fileCount;
            public UInt64 nameTableOffset;
            public UInt64 unknown;

            public string Magic => magic;
            public UInt32 Version => version;
            public UInt32 FileCount => fileCount;
            public string Type => type;
            public UInt64 NameTableOffset => nameTableOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Ansi)]
        public struct FileHeader {
            public UInt32 nameHash;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string extension;

            public UInt32 dirHash;
            public UInt32 flags;
            public UInt64 offset;
            public UInt32 packedSize;
            public UInt32 fullSize;
            public UInt32 align;

            public uint NameHash => nameHash;
            public string Extension => extension;
            public uint DirHash => dirHash;
            public uint Flags => flags;
            public ulong Offset => offset;
            public uint PackedSize => packedSize;
            public uint FullSize => fullSize;
            public uint Align => align;
        }

        private string _filePath;
        private Header _header;
        private Dictionary<string, FileHeader> _files = new();

        public IReadOnlyDictionary<string, FileHeader> Files => _files;

        public Ba2Archive(string filePath) {
            _filePath = filePath;

            List<FileHeader> fileMetadata = new();

            using (var file = new FileStream(_filePath, FileMode.Open, FileAccess.Read)) {
                var headerSize = Marshal.SizeOf(typeof(Header));
                var buffer = new byte[headerSize];
                file.Read(buffer, 0, headerSize);

                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                _header = Marshal.PtrToStructure<Header>(handle.AddrOfPinnedObject());
                handle.Free();

                // Read file metadata
                for (int i = 0; i < _header.FileCount; i++) {
                    headerSize = Marshal.SizeOf(typeof(FileHeader));
                    buffer = new byte[headerSize];
                    file.Read(buffer, 0, headerSize);

                    handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    var header = Marshal.PtrToStructure<FileHeader>(handle.AddrOfPinnedObject());
                    handle.Free();

                    fileMetadata.Add(header);
                }

                // Read file name table
                var idx = 0;
                file.Seek((long)_header.NameTableOffset, SeekOrigin.Begin);
                using (var reader = new BinaryReader(file)) {
                    while (reader.PeekChar() > -1) {
                        try {
                            var len = reader.ReadUInt16();
                            var str = new string(reader.ReadChars(len));
                            _files.Add(str, fileMetadata[idx]);
                        } catch (Exception ex) {
                        }

                        idx++;
                    }
                }
            }
        }

        public void ExportFile(string path, string target) {
            if (_files.ContainsKey(path)) {
                var header = _files[path];

                using (var file = new FileStream(_filePath, FileMode.Open, FileAccess.Read)) {
                    file.Seek((long)header.Offset, SeekOrigin.Begin);

                    using (var compressed = new MemoryStream()) {
                        CopyTo(file, compressed, (int)header.PackedSize);
                        compressed.Flush();
                        compressed.Seek(0, SeekOrigin.Begin);

                        using (var output = new FileStream($"{target}", FileMode.Create, FileAccess.ReadWrite)) {
                            using (var zOut = new ZOutputStream(output)) {
                                compressed.CopyTo(zOut);
                                zOut.Flush();
                            }
                        }
                    }
                }
            }
        }

        static void CopyTo(Stream fromStream, Stream destination, int count, int bufferSize = 81920) {
            var buffer = new byte[bufferSize];
            int read;
            while ((read = fromStream.Read(buffer, 0, Math.Min(count, bufferSize))) != 0) {
                count -= read;
                destination.Write(buffer, 0, read);
            }
        }
    }
}