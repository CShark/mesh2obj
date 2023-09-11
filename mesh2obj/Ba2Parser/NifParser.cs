using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using mesh2obj.Ba2Parser.Ba2;
using mesh2obj.Ba2Parser.Nif;

/*
 * Format source: https://github.com/niftools/nifxml/blob/master/nif.xml
 */

namespace mesh2obj.Ba2Parser {
    internal class NifParser : IParser {
        private string _starfieldPath;

        public NifParser(string starfieldPath) {
            _starfieldPath = starfieldPath;
        }

        public void Parse(string inputFile, string outputFile) {
            var header = new NiHeader();

            List<BsGeometry> geometries = new();

            using (var file = new FileStream(inputFile, FileMode.Open, FileAccess.Read)) {
                using (var reader = new BinaryReader(file)) {
                    header.Parse(reader);

                    foreach (var block in header.Blocks.Where(x => x.Type == "BSGeometry")) {
                        reader.BaseStream.Seek(block.Offset, SeekOrigin.Begin);

                        var bsGeo = new BsGeometry();
                        bsGeo.Parse(reader);
                        geometries.Add(bsGeo);
                    }
                }
            }

            // find geometries
            var dir = Path.GetDirectoryName(inputFile);
            while (!string.IsNullOrEmpty(dir)) {
                if (Path.GetFileName(dir) == "meshes")
                    break;

                dir = Path.GetDirectoryName(dir);
            }

            if (string.IsNullOrEmpty(dir)) {
                Console.WriteLine(
                    "Could not find geometries directory relative to meshes directory. Please place your .nif file in the appropriate meshes subfolder.");
                return;
            }

            var geometryPath = Path.Combine(Path.GetDirectoryName(dir), "geometries");

            List<string> pendingFiles = new List<string>();
            foreach (var geo in geometries) {
                if (!File.Exists(Path.Combine(geometryPath, $"{geo.MeshReference}.mesh"))) {
                    pendingFiles.Add(geo.MeshReference);
                }
            }

            if (pendingFiles.Count > 0) {
                if (!Directory.Exists(_starfieldPath)) {
                    Console.WriteLine(
                        "Missing geometries, but no starfield data path given to automatically fetch the missing geometries");
                } else {
                    var meshes01 = new Ba2Archive(Path.Combine(_starfieldPath, "Starfield - Meshes01.ba2"));
                    var meshes02 = new Ba2Archive(Path.Combine(_starfieldPath, "Starfield - Meshes02.ba2"));
                    var meshesPatch = new Ba2Archive(Path.Combine(_starfieldPath, "Starfield - MeshesPatch.ba2"));

                    // extract missing files from starfield source
                    foreach (var file in pendingFiles) {
                        var targetPath = Path.Combine(geometryPath, file + ".mesh");
                        var ba2Path = $"geometries/{file.Replace("\\", "/")}.mesh";

                        if (!Directory.Exists(Path.GetDirectoryName(targetPath))) {
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                        }

                        if (meshes01.Files.ContainsKey(ba2Path)) {
                            meshes01.ExportFile(ba2Path, targetPath);
                        } else if (meshes02.Files.ContainsKey(ba2Path)) {
                            meshes02.ExportFile(ba2Path, targetPath);
                        } else if (meshesPatch.Files.ContainsKey(ba2Path)) {
                            meshesPatch.ExportFile(ba2Path, targetPath);
                        } else {
                            Console.WriteLine("Could not find ba2-Archive for mesh " + file);
                        }
                    }
                }
            }

            var meshParser = new MeshParser();
            foreach (var geo in geometries) {
                var geoFile = Path.Combine(geometryPath, $"{geo.MeshReference}.mesh");

                if (File.Exists(geoFile)) {
                    meshParser.Parse(geoFile, Path.ChangeExtension(geoFile, ".obj"));
                }
            }

            // Combine .obj into single one
            long indexOffset = 0;
            Dictionary<BsGeometry, long> offsets = new();
            NumberFormatInfo nif = new NumberFormatInfo { NumberDecimalSeparator = "." };
            using (var file = new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite)) {
                using (var writer = new StreamWriter(file)) {
                    // Add vertices
                    foreach (var geo in geometries) {
                        var geoFile = Path.Combine(geometryPath, $"{geo.MeshReference}.obj");

                        if (File.Exists(geoFile)) {
                            offsets.Add(geo, indexOffset);
                            writer.WriteLine();
                            writer.WriteLine("# Vertices for " + geo.MeshReference);

                            using (var geoStream = new FileStream(geoFile, FileMode.Open, FileAccess.Read)) {
                                using (var geoReader = new StreamReader(geoFile)) {
                                    string? data;
                                    while ((data = geoReader.ReadLine()) != null) {
                                        if (data.StartsWith("v ")) {
                                            indexOffset++;

                                            // Transpose position
                                            var parts = data.Split(' ');
                                            var position = new Vector3(float.Parse(parts[1], nif),
                                                float.Parse(parts[2], nif), float.Parse(parts[3], nif));

                                            position *= geo.Scale;
                                            position = Vector3.Transform(position, geo.Rotation);
                                            position += geo.Translation;

                                            writer.WriteLine("v {0} {1} {2}", position.X.ToString(nif),
                                                position.Y.ToString(nif), position.Z.ToString(nif));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Add UV maps
                    foreach (var geo in geometries) {
                        var geoFile = Path.Combine(geometryPath, $"{geo.MeshReference}.obj");

                        if (File.Exists(geoFile)) {
                            writer.WriteLine();
                            writer.WriteLine("# UV Coordinates for " + geo.MeshReference);

                            using (var geoStream = new FileStream(geoFile, FileMode.Open, FileAccess.Read)) {
                                using (var geoReader = new StreamReader(geoFile)) {
                                    string? data;
                                    while ((data = geoReader.ReadLine()) != null) {
                                        if (data.StartsWith("vt ")) {
                                            writer.WriteLine(data);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    // Rebuild objects & faces
                    foreach (var geo in geometries) {
                        var geoFile = Path.Combine(geometryPath, $"{geo.MeshReference}.obj");

                        if (File.Exists(geoFile)) {
                            writer.WriteLine();
                            writer.WriteLine("# Faces for " + geo.MeshReference);
                            writer.WriteLine("g " + geo.MeshReference);
                            var offset = offsets[geo];

                            using (var geoStream = new FileStream(geoFile, FileMode.Open, FileAccess.Read)) {
                                using (var geoReader = new StreamReader(geoFile)) {
                                    string? data;
                                    while ((data = geoReader.ReadLine()) != null) {
                                        if (data.StartsWith("f ")) {
                                            var parts = data.Split(' ');

                                            var f1 = long.Parse(parts[1].Split('/')[0]);
                                            var f2 = long.Parse(parts[2].Split('/')[0]);
                                            var f3 = long.Parse(parts[3].Split('/')[0]);

                                            writer.WriteLine("f {0}/{0} {1}/{1} {2}/{2}", f1 + offset, f2 + offset,
                                                f3 + offset);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}