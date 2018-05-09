using EPBLib;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EPBLib.Helpers;

namespace epb
{
    class Program
    {
        static string exectutableName;
        static bool showHelp = false;

        static bool write = false;
        static string writePath = "NewBlueprint.epb";
        static EpBlueprint.EpbType type = EpBlueprint.EpbType.Base;
        static UInt32 width       = 1;
        static UInt32 height      = 1;
        static UInt32 depth       = 1;
        static string creatorId   = "Gronk";
        static string creatorName = "Apan Loon";
        static string ownerId     = "Gronkers";
        static string ownerName   = "Apan Loony";

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
                CreateEpb(writePath);
                return;
            }

            foreach (string inPath in inPaths)
            {
                try
                {
                    OpenEpb(inPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error on file {inPath}: {ex.Message}\r\n{ex.InnerException}\r\n{ex.StackTrace}");
                }
            }
        }

        static void CreateEpb(string path)
        {
            EpBlueprint epb = new EpBlueprint(type, width, height, depth);

            EpMetaTag03 metaTag11 = new EpMetaTag03(EpMetaTagKey.UnknownMetax11)
            {
                Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }
            };
            epb.MetaTags.Add(metaTag11.Key, metaTag11);

            EpMetaTag01 metaTag01 = new EpMetaTag01(EpMetaTagKey.UnknownMetax01)
            {
                Value = 0x0000
            };
            epb.MetaTags.Add(metaTag01.Key, metaTag01);

            EpMetaTag03 metaTag0E = new EpMetaTag03(EpMetaTagKey.UnknownMetax0E)
            {
                Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }
            };
            epb.MetaTags.Add(metaTag0E.Key, metaTag0E);

            EpMetaTag03 metaTag0F = new EpMetaTag03(EpMetaTagKey.UnknownMetax0F)
            {
                Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }
            };
            epb.MetaTags.Add(metaTag0F.Key, metaTag0F);

            EpMetaTag01 metaTag05 = new EpMetaTag01(EpMetaTagKey.UnknownMetax05)
            {
                Value = 0x0000
            };
            epb.MetaTags.Add(metaTag05.Key, metaTag05);

            EpMetaTag02 metaTag04 = new EpMetaTag02(EpMetaTagKey.UnknownMetax04)
            {
                Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }
            };
            epb.MetaTags.Add(metaTag04.Key, metaTag04);

            EpMetaTag04 metaTag06 = new EpMetaTag04(EpMetaTagKey.UnknownMetax06)
            {
                Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
            };
            epb.MetaTags.Add(metaTag06.Key, metaTag06);

            EpMetaTagString metaTag07 = new EpMetaTagString(EpMetaTagKey.UnknownMetax07)
            {
                Value = ""
            };
            epb.MetaTags.Add(metaTag07.Key, metaTag07);

            EpMetaTag05 metaTag09 = new EpMetaTag05(EpMetaTagKey.UnknownMetax09)
            {
                Value = new byte[] { 0x94, 0x90, 0x35, 0xdf, 0x0a, 0xb2, 0xd5, 0x88, 0x00 }
            };
            epb.MetaTags.Add(metaTag09.Key, metaTag09);

            EpMetaTag02 metaTag08 = new EpMetaTag02(EpMetaTagKey.UnknownMetax08)
            {
                Value = new byte[] { 0x4a, 0x06, 0x00, 0x00, 0x00 }
            };
            epb.MetaTags.Add(metaTag08.Key, metaTag08);

            EpMetaTagString creatorIdTag = new EpMetaTagString(EpMetaTagKey.CreatorId)
            {
                Value = creatorId
            };
            epb.MetaTags.Add(creatorIdTag.Key, creatorIdTag);

            EpMetaTagString creatorNameTag = new EpMetaTagString(EpMetaTagKey.CreatorName)
            {
                Value = creatorName
            };
            epb.MetaTags.Add(creatorNameTag.Key, creatorNameTag);

            EpMetaTagString ownerIdTag = new EpMetaTagString(EpMetaTagKey.OwnerId)
            {
                Value = ownerId
            };
            epb.MetaTags.Add(ownerIdTag.Key, ownerIdTag);

            EpMetaTagString ownerNameTag = new EpMetaTagString(EpMetaTagKey.OwnerName)
            {
                Value = ownerName
            };
            epb.MetaTags.Add(ownerNameTag.Key, ownerNameTag);

            EpMetaTagString metaTag10 = new EpMetaTagString(EpMetaTagKey.UnknownMetax10) {Value = ""};
            epb.MetaTags.Add(metaTag10.Key, metaTag10);

            EpMetaTag05 metaTag12 = new EpMetaTag05(EpMetaTagKey.UnknownMetax12)
            {
                Value = new byte[] {0x00, 0x80, 0x80, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00}
            };
            epb.MetaTags.Add(metaTag12.Key, metaTag12);

            //Core:
            EpbDeviceGroup group = new EpbDeviceGroup
            {
                Name = "Core",
                Flags = 0xff01
            };
            EpbDeviceGroupEntry core = new EpbDeviceGroupEntry
            {
                Unknown = new byte[] {0x00, 0x08, 0x00, 0x80},
                Name = ""
            };
            group.Entries.Add(core);
            epb.DeviceGroups.Add(group);

            group = new EpbDeviceGroup
            {
                Name = "Ungrouped",
                Flags = 0xff00
            };
            epb.DeviceGroups.Add(group);

            using (FileStream stream = File.Create(path))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(epb);
                }
            }
        }

        static void OpenEpb(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                long bytesLeft = stream.Length;

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    try
                    {
                        EpBlueprint epb = reader.ReadEpBlueprint(ref bytesLeft);
                    }
                    catch (System.Exception ex)
                    {
                        throw new Exception("Failed reading EPB file", ex);
                    }
                }
            }
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
