using System.Globalization;
using CommandLine;
using mesh2obj.Ba2Parser;

namespace mesh2obj {
    internal class Program {
        internal class Options {
            [Option('i', "input", Required = true, HelpText = "The .mesh or .nif file to convert")]
            public string Input { get; set; }

            [Option('o', "output", Required = false, Default = null, HelpText = "Path to the .obj file to generate")]
            public string Output { get; set; }

            [Option("starfield", Required = false,
                HelpText = "Path to the Starfield data files to extract dependencies on the fly")]
            public string StarfieldDataPath { get; set; }
        }

        static void Main(string[] args) {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o => {
                if (File.Exists(o.Input)) {
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(o.Input));

                    if (o.Output == null) {
                        o.Output = Path.ChangeExtension(o.Input, "obj");
                    }

                    IParser? parser = null;

                    if (o.Input.ToLower().EndsWith(".mesh")) {
                        parser = new MeshParser();
                    }else if (o.Input.ToLower().EndsWith(".nif")) {
                        parser = new NifParser(o.StarfieldDataPath);
                    }

                    if (parser != null) {
                        parser.Parse(o.Input, o.Output);
                    } else {
                        Console.WriteLine("Unknown file format");
                    }
                }
            });
        }
    }
}