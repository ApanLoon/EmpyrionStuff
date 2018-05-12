using EPBLib;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using EPBLib.Helpers;

namespace epb
{
    class Program
    {
        public enum CreateTemplate
        {
            None,
            BaseBox,
            BaseBoxFrame,
            BaseSingleBlock,
            BasePyramid,
            BaseBlockTypes
        }

        public static string exectutableName;
        public static bool showHelp = false;

        public static CreateTemplate        createTemplate = CreateTemplate.None;
        public static bool                  hollow = false;
        public static string                writePath      = "NewBlueprint.epb";
        public static EpBlueprint.EpbType   type           = EpBlueprint.EpbType.Base;
        public static EpbBlock.EpbBlockType blockType      = EpbBlock.EpbBlockType.SteelBlockL_A;
        public static byte                  blockVariant   = 0x00;
        public static UInt32                width          = 1;
        public static UInt32                height         = 1;
        public static UInt32                depth          = 1;
        public static string                creatorId      = "Gronk";
        public static string                creatorName    = "Apan Loon";
        public static string                ownerId        = "Gronkers";
        public static string                ownerName      = "Apan Loony";

        public static List<string> inPaths;

        public static void Main(string[] args)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            exectutableName = Path.GetFileName(codeBase);

            var optionSet = new OptionSet()
            {
                {
                    "c|create=",
                    $"Create a new blueprint. Options are: {string.Join(", ", Enum.GetNames(typeof(CreateTemplate)))}",
                    v =>
                    {
                        if (v != null)
                        {
                            createTemplate = (CreateTemplate)Enum.Parse(typeof(CreateTemplate), v);
                        }
                    }
                },
                {
                    "hollow",
                    $"Make the created blueprint hollow.",
                    v => hollow = v != null
                },
                {
                    "o|outpath=",
                    $"Path and name of the created blueprint.",
                    v =>
                    {
                        if (v != null)
                        {
                            writePath = v;
                        }
                    }
                },
                {
                    "s|size=",
                    "Size of new blueprint (w,h,d) No spaces",
                    v =>
                    {
                        if (v != null)
                        {
                            string[] a = v.Split(',');
                            width  = UInt32.Parse(a[0]);
                            height = UInt32.Parse(a[1]);
                            depth  = UInt32.Parse(a[2]);
                        }
                    }
                },
                {
                    "b|blocktype=",
                    "Block type to use for shapes.",
                    v =>
                    {
                        if (v != null)
                        {
                            try
                            {
                                blockType = (EpbBlock.EpbBlockType)(byte)new System.ComponentModel.ByteConverter().ConvertFromString(v);
                            }
                            catch (Exception e)
                            {
                                blockType = (EpbBlock.EpbBlockType)Enum.Parse(typeof(EpbBlock.EpbBlockType), v);
                            }
                        }
                    }
                },
                {
                    "v|blockvariant=",
                    "Block variant to use for shapes.",
                    v =>
                    {
                        if (v != null)
                        {
                            try
                            {
                                blockVariant = (byte)new System.ComponentModel.ByteConverter().ConvertFromString(v);
                            }
                            catch (Exception e)
                            {
                                blockVariant = EpbBlock.GetVariant(blockType, v);
                            }
                        }
                    }
                },
                {
                    "h|help",
                    "Show this message and exit",
                    v => showHelp = v != null
                }
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

            switch (createTemplate)
            {
                case CreateTemplate.None:
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
                    break;
                case CreateTemplate.BaseBox:
                    CreateBox(writePath, hollow);
                    break;
                case CreateTemplate.BaseBoxFrame:
                    CreateBoxFrame(writePath);
                    break;
                case CreateTemplate.BasePyramid:
                    CreatePyramid(writePath, hollow);
                    break;
                case CreateTemplate.BaseBlockTypes:
                    width = 16;
                    height = 1;
                    depth = 16;
                    CreateBlockTypes(writePath);
                    break;
            }
        }



        static void CreateBox(string path, bool hollow)
        {
            EpBlueprint epb = CreateCommon();

            for (UInt32 z = 0; z < depth; z++)
            {
                for (UInt32 y = 0; y < height; y++)
                {
                    for (UInt32 x = 0; x < width; x++)
                    {
                        bool isInterior =    (x > 0 && x < (width  - 1))
                                          && (y > 0 && y < (height - 1))
                                          && (z > 0 && z < (depth  - 1));
                        byte[] c = isInterior ? new byte[] { 3, 3, 3, 3, 3, 3 } : new byte[] { 0, 0, 0, 0, 0, 0 };
                        byte[] t = new byte[]  {14, 14, 14, 14, 14, 14};
                        bool[] tf = ((x % 2) == 0) ? new bool[] {false, false, false, false, false, false} : new bool[] { true, true, true, true, true, true };

                        if (!isInterior || !hollow)
                        {
                            epb.SetBlock(new EpbBlock() { BlockType = blockType, Variant = blockVariant, FaceColours = c, Textures = t, TextureFlips = tf}, x, y, z);
                        }
                    }
                }
            }

            //// Create Device list:
            //EpbDeviceGroup group = new EpbDeviceGroup
            //{
            //    Name = "Core",
            //    Flags = 0xff01
            //};
            //EpbDeviceGroupEntry core = new EpbDeviceGroupEntry
            //{
            //    Unknown = new byte[] {0x00, 0x08, 0x00, 0x80},
            //    Name = ""
            //};
            //group.Entries.Add(core);
            //epb.DeviceGroups.Add(group);

            //group = new EpbDeviceGroup
            //{
            //    Name = "Ungrouped",
            //    Flags = 0xff00
            //};
            //epb.DeviceGroups.Add(group);


            // Write the file:
            using (FileStream stream = File.Create(path))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(epb);
                }
            }
        }

        static void CreateBoxFrame(string path)
        {
            EpBlueprint epb = CreateCommon();

            for (UInt32 z = 0; z < depth; z++)
            {
                for (UInt32 y = 0; y < height; y++)
                {
                    for (UInt32 x = 0; x < width; x++)
                    {
                        bool a = x % (width  - 1) == 0;
                        bool b = y % (height - 1) == 0;
                        bool c = z % (depth  - 1) == 0;

                        if (!(
                               (!a && !b &&  c)
                            || (!a &&  b && !c)
                            || ( a && !b && !c)
                            || (!a && !b && !c)
                           ))
                        {
                            epb.SetBlock(new EpbBlock() { BlockType = blockType, Variant = blockVariant }, x, y, z);
                        }
                    }
                }
            }

            // Write the file:
            using (FileStream stream = File.Create(path))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(epb);
                }
            }
        }

        static void CreatePyramid(string path, bool hollow)
        {
            EpBlueprint epb = CreateCommon();

            UInt32 w = width;
            UInt32 h = height;
            UInt32 d = depth;

            for (UInt32 y = 0; y < h; y++)
            {
                for (UInt32 z = y; z < d; z++)
                {
                    for (UInt32 x = y; x < w; x++)
                    {
                        bool isBackEdge  = (z == y);
                        bool isFrontEdge = (z == (d - 1));
                        bool isLeftEdge  = (x == y);
                        bool isRightEdge = (x == (w - 1));
                        bool isInterior  = !isBackEdge && ! isFrontEdge && !isRightEdge && !isLeftEdge && y > 0 && y < (h - 1);

                        EpbBlock.EpbBlockType     t = blockType;
                        EpbBlock.EpbBlockRotation r = EpbBlock.EpbBlockRotation.PzPy;
                        byte                      v = blockVariant;
                        byte[]                    c = new byte[] { 0, 0, 0, 0, 0, 0 };

                        if (isBackEdge && isLeftEdge)
                        {
                            t = EpbBlock.EpbBlockType.SteelBlockL_A;
                            v = EpbBlock.GetVariant(t, "Corner Large A");
                            r = EpbBlock.EpbBlockRotation.PxPy;
                        }
                        else if (isBackEdge && isRightEdge)
                        {
                            t = EpbBlock.EpbBlockType.SteelBlockL_A;
                            v = EpbBlock.GetVariant(t, "Corner Large A");
                            r = EpbBlock.EpbBlockRotation.PzPy;
                        }
                        else if (isFrontEdge && isLeftEdge)
                        {
                            t = EpbBlock.EpbBlockType.SteelBlockL_A;
                            v = EpbBlock.GetVariant(t, "Corner Large A");
                            r = EpbBlock.EpbBlockRotation.NzPy;
                        }
                        else if (isFrontEdge && isRightEdge)
                        {
                            t = EpbBlock.EpbBlockType.SteelBlockL_A;
                            v = EpbBlock.GetVariant(t, "Corner Large A");
                            r = EpbBlock.EpbBlockRotation.NxPy;
                        }
                        else if (isBackEdge)
                        {
                            t = EpbBlock.EpbBlockType.SteelBlockL_A;
                            v = EpbBlock.GetVariant(t, "Slope");
                            r = EpbBlock.EpbBlockRotation.NzPy;
                        }
                        else if (isFrontEdge)
                        {
                            t = EpbBlock.EpbBlockType.SteelBlockL_A;
                            v = EpbBlock.GetVariant(t, "Slope");
                            r = EpbBlock.EpbBlockRotation.PzPy;
                        }
                        else if (isLeftEdge)
                        {
                            t = EpbBlock.EpbBlockType.SteelBlockL_A;
                            v = EpbBlock.GetVariant(t, "Slope");
                            r = EpbBlock.EpbBlockRotation.NxPy;
                        }
                        else if (isRightEdge)
                        {
                            t = EpbBlock.EpbBlockType.SteelBlockL_A;
                            v = EpbBlock.GetVariant(t, "Slope");
                            r = EpbBlock.EpbBlockRotation.PxPy;
                        }
                        else if (isInterior)
                        {
                            c = new byte[] { 3, 3, 3, 3, 3, 3 };
                        }

                        if (!isInterior || !hollow)
                        {
                            epb.SetBlock(new EpbBlock() { BlockType = t, Rotation = r, Variant = v, FaceColours = c }, x, y, z);
                        }
                    }
                }

                w -= 1;
                d -= 1;
            }

            // Write the file:
            using (FileStream stream = File.Create(path))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(epb);
                }
            }
        }



        //TODO: This creates a blueprint that can't be spawned in game
        static void CreateBlockTypes(string path)
        {
            EpBlueprint epb = CreateCommon();

            for (UInt32 i = 0; i < 256; i++)
            {
                {
                    UInt32 x = i % width;
                    UInt32 z = i / width;
                    epb.SetBlock(new EpbBlock() { BlockType = (EpbBlock.EpbBlockType)i, Variant = blockVariant }, x, 0, z);
                }
            }

            // Write the file:
            using (FileStream stream = File.Create(path))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(epb);
                }
            }
        }

        static EpBlueprint CreateCommon()
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

            EpMetaTagString metaTag10 = new EpMetaTagString(EpMetaTagKey.UnknownMetax10) { Value = "" };
            epb.MetaTags.Add(metaTag10.Key, metaTag10);

            EpMetaTag05 metaTag12 = new EpMetaTag05(EpMetaTagKey.UnknownMetax12)
            {
                Value = new byte[] { 0x00, 0x80, 0x80, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00 }
            };
            epb.MetaTags.Add(metaTag12.Key, metaTag12);

            return epb;
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
