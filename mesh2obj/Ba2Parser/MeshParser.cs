﻿using System.Globalization;

/*
 * Format source: https://github.com/fo76utils/ce2utils/blob/6676ac74e90f07c1f246538ea33635c100b961ca/src/meshfile.cpp
 */

namespace mesh2obj.Ba2Parser {
    internal class MeshParser : IParser {
        public void Parse(string inputFile, string outputFile) {
            using (var file = new FileStream(inputFile, FileMode.Open, FileAccess.Read)) {
                using (var reader = new BinaryReader(file)) {
                    reader.ReadUInt32();

                    var triangleCount = reader.ReadUInt32() / 3;
                    var triangleOffset = file.Position;

                    reader.ReadBytes(2 * (int)triangleCount * 3);

                    var coordinateScale = reader.ReadSingle();
                    reader.ReadUInt32();

                    var vertexCount = reader.ReadUInt32();
                    var vertexOffset = file.Position;

                    reader.ReadBytes(2 * (int)vertexCount * 3);

                    var uvCount = reader.ReadUInt32();
                    var uvOffset = file.Position;

                    reader.ReadBytes(2 * (int)uvCount * 2);

                    var count = reader.ReadUInt32();
                    reader.ReadBytes((int)count * 4);
                    count = reader.ReadUInt32();
                    reader.ReadBytes((int)count * 4);

                    var normalCount = reader.ReadUInt32();
                    var normalOffset = file.Position;

                    file.Seek(0, SeekOrigin.Begin);

                    if (normalCount != vertexCount || normalCount != uvCount) {
                        Console.WriteLine("Normal, Vertex & UV points don't match");
                        return;
                    }

                    Console.WriteLine(
                        $"Converting mesh {Path.GetFileNameWithoutExtension(inputFile)} with {vertexCount} vertices and {triangleCount} faces...");

                    using (var output = new FileStream(outputFile, FileMode.Create, FileAccess.Write)) {
                        using (var writer = new StreamWriter(output)) {
                            NumberFormatInfo nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };

                            file.Seek(vertexOffset, SeekOrigin.Begin);

                            for (int i = 0; i < vertexCount; i++) {
                                var x = reader.ReadInt16() * coordinateScale / 32767;
                                var y = reader.ReadInt16() * coordinateScale / 32767;
                                var z = reader.ReadInt16() * coordinateScale / 32767;
                                writer.WriteLine("v {0} {1} {2}", x.ToString(nfi), y.ToString(nfi), z.ToString(nfi));
                            }

                            writer.WriteLine();

                            file.Seek(uvOffset, SeekOrigin.Begin);

                            for (int i = 0; i < uvCount; i++) {
                                var u = reader.ReadHalf();
                                var w = reader.ReadHalf();
                                writer.WriteLine("vt {0} {1}", u.ToString(nfi), w.ToString(nfi));
                            }

                            writer.WriteLine();

                            writer.WriteLine("g Object");

                            file.Seek(triangleOffset, SeekOrigin.Begin);

                            for (int i = 0; i < triangleCount; i++) {
                                var i0 = reader.ReadUInt16();
                                var i1 = reader.ReadUInt16();
                                var i2 = reader.ReadUInt16();

                                writer.WriteLine("f {0}/{0} {1}/{1} {2}/{2}", i0 + 1, i1 + 1, i2 + 1);
                            }
                        }
                    }
                }
            }
        }
    }
}