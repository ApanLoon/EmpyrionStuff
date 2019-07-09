using System;
using System.IO;
using ECFLib.IO;
using ECFLib;
using Fclp;

namespace ecf
{
    class Program
    {
        private static string EcfPath { get; set; }
        private static string OutPath { get; set; }
        private static bool OutputCode { get; set; }
        private static bool CmdListBlocks { get; set; }
        private static bool CmdShowHelp { get; set; }


        private static void Main(string[] args)
        {
            var p = new FluentCommandLineParser(); // http://fclp.github.io/fluent-command-line-parser/
            p.Setup<string>('f', "ecf")
                .SetDefault("G:/Steam/steamapps/common/Empyrion - Galactic Survival/Content/Configuration/Config_Example.ecf")
                .Callback(path => EcfPath = path)
                .WithDescription("Path to the ecf file to read.");

            p.Setup<string>('o', "out")
                .SetDefault("Config.ecf")
                .Callback(path => OutPath = path)
                .WithDescription("Path to the ecf file to write.");

            p.Setup<bool>('b', "blocks")
                .Callback(v => CmdListBlocks = v)
                .SetDefault(false)
                .WithDescription("List the blocks extracted from the ecf file.");

            p.Setup<bool>('c', "code")
                .Callback(v => OutputCode = v)
                .SetDefault(false)
                .WithDescription("When appropriate, use code formatting.");


            p.SetupHelp("h", "help")
                .Callback(ShowHelp);

            var result = p.Parse(args);

            if (result.HasErrors)
            {
                ShowErrors(result);
                PauseOnExit();
                return;
            }

            Config config = OpenECF(EcfPath);

            //if (CmdListBlocks)
            //{
            //    ListBlocks(config);
            //}

            WriteECF(config, OutPath);

            PauseOnExit();
        }

        private static Config OpenECF(string path)
        {
            using (StreamReader reader = new StreamReader(File.OpenRead(path)))
            {
                try
                {
                    return reader.ReadEcf();
                }
                catch (System.Exception ex)
                {
                    throw new Exception("Failed reading ECF file", ex);
                }
            }
        }

        private static void WriteECF(Config config, string path)
        {
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(path)))
            {
                try
                {
                    writer.EcfWrite(config);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed writing ECF file", ex);
                }
            }
        }


        private static void ListBlocks(Config config)
        {
            if (OutputCode)
            {
                //Console.WriteLine($"        public static readonly Dictionary<UInt16, BlockType> BlockTypes = new Dictionary<UInt16, BlockType>()");
                //Console.WriteLine( "        {");

                //foreach (BlockType block in config.BlockTypes)
                //{
                //    Console.WriteLine($"            {{ {block.Id,5}, new BlockType(){{Id = {block.Id,5}, Name = {"\"" + block.Name + "\"",-31}, Category = {"\"" + block.Category + "\"",-31}, Ref = {"\"" + block.RefName + "\"",-31}}}}},");
                //}
                //Console.WriteLine("        };");
            }
            else
            {
                //Console.WriteLine($"Blocks in {config.Path}:");
                //foreach (Entity block in config.BlockEntities)
                //{
                //    Console.WriteLine(block);
                //}
            }
        }


        private static void ShowErrors(ICommandLineParserResult result)
        {
            Console.Write("ecf: Error parsing arguments.");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"   {error.ToString()}");
            }
            Console.WriteLine();
            Console.WriteLine($"Try `ecf --help' for more information.");
        }

        private static void ShowHelp(string s)
        {
            Console.WriteLine($"Usage: ecf [OPTIONS]+");
            Console.WriteLine();
            Console.WriteLine("Operate on files in the Empyrion configuration (ecf) format.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine(s);
            //Console.WriteLine();
            //Console.WriteLine("Examples:");
            //Console.WriteLine("    epb -c BaseBox -b 412 -v 0 -s 10,10,10 -o Box.epb");
            //Console.WriteLine("    epb -c BaseBox --hollow -b 412 -v 0 -s 10,10,10 -o BoxHollow.epb");
            //Console.WriteLine("    epb -c BaseFrame -b 412 -v 0 -s 10,10,10 -o Frame.epb");
            //Console.WriteLine("    epb -c BasePyramid -s 8,4,8 -o Pyramid.epb");
        }

        private static void PauseOnExit()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
