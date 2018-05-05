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

        static List<string> inPaths;

        static void Main(string[] args)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            exectutableName = Path.GetFileName(codeBase);

            var optionSet = new OptionSet()
            {
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
