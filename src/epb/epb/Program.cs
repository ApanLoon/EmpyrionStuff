using EPBLib;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace epb
{
    class Program
    {
        static string exectutableName;
        static bool showHelp = false;

        static bool write = false;
        static string writePath = "NewBlueprint.epb";
        static EPB.BluePrintType type = EPB.BluePrintType.Base;
        static UInt32 width       = 1;
        static UInt32 height      = 1;
        static UInt32 depth       = 1;
        static string creatorId   = "76561198111970779";
        static string creatorName = "Apan Loon";
        static string ownerId     = "76561198111970779";
        static string ownerName   = "Apan Loon";

        static List<string> inPaths;

        static void Main(string[] args)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            exectutableName = Path.GetFileName(codeBase);

            var optionSet = new OptionSet()
            {
                { "c|create=",      "Create a new blueprint",
                                v => { write = v != null; writePath = v; } },
                { "h|help",      "Show this message and exit",
                                v => showHelp       = v != null }
            };

            try
            {
                inPaths = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", exectutableName);
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", exectutableName);
                PauseOnExit();
                return;
            }

            if (showHelp)
            {
                ShowHelp(optionSet);
                PauseOnExit();
                return;
            }

            if (write)
            {
                CreateEPB(writePath);
                return;
            }

            foreach (string inPath in inPaths)
            {
                try
                {
                    OpenEPB(inPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error on file {0}: {1}\n{2}\n{3}", inPath, ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
        }

        static void CreateEPB(string path)
        {
            EPB epb = new EPB(type, width, height, depth, creatorId, creatorName, ownerId, ownerName);
            epb.Write(path);
        }

        static void OpenEPB(string inPath)
        {
            EPB epb = new EPB(inPath);
        }


        static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine("Usage: {0} [OPTIONS]+ [FILE]+", exectutableName);
            Console.WriteLine();
            Console.WriteLine("Operate on files in the Empyrion blueprint (epb) TPC format.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
        }

        static void PauseOnExit()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Enter to exit.");
                Console.ReadLine();
            }
        }

    }
}
