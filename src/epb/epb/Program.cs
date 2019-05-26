using EPBLib;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using EPBLib.BlockData;
using EPBLib.Helpers;

namespace epb
{
    class Program
    {
        public enum CommandType
        {
            Open,
            Create,
            ShowHelp
        }
 
        public enum CreateTemplateType
        {
            BaseRotations,
            BaseBox,
            BaseBoxFrame,
            BasePyramid,
            BaseBlockTypes,
            SVHullVariants
        }

        public static CommandType Command = CommandType.Open;

        public static CreateTemplateType    CreateTemplate  = CreateTemplateType.BaseBox;
        public static bool                  Hollow          = false;
        public static string                OutputPath      = "NewBlueprint.epb";
        public static EpBlueprint.EpbType   BlueprintType   = EpBlueprint.EpbType.Base;
        public static EpbBlock.EpbBlockType BlockType       = EpbBlock.GetBlockType("HullFullLarge", "Cube");
        public static byte                  BlockVariant    = 0x00;
        public static UInt32                Width           = 1;
        public static UInt32                Height          = 1;
        public static UInt32                Depth           = 1;
        public static string                CreatorId       = "Gronk";
        public static string                CreatorName     = "Apan Loon";
        public static string                OwnerId         = "Gronkers";
        public static string                OwnerName       = "Apan Loony";

        public static List<string> InputPaths;

        public static string ExectutableName;

        public static void Main(string[] args)
        {
            Console.Out.NewLine = "\n";

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            ExectutableName = Path.GetFileName(codeBase);

            var optionSet = new OptionSet()
            {
                {
                    "c|create=",
                    $"Create a new blueprint. Options are: {string.Join(", ", Enum.GetNames(typeof(CreateTemplateType)))}",
                    v =>
                    {
                        if (v != null)
                        {
                            Command = CommandType.Create;
                            CreateTemplate = (CreateTemplateType)Enum.Parse(typeof(CreateTemplateType), v);
                        }
                    }
                },
                {
                    "hollow",
                    $"Make the created blueprint hollow.",
                    v => Hollow = v != null
                },
                {
                    "o|outpath=",
                    $"Path and name of the created blueprint.",
                    v =>
                    {
                        if (v != null)
                        {
                            OutputPath = v;
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
                            Width  = UInt32.Parse(a[0]);
                            Height = UInt32.Parse(a[1]);
                            Depth  = UInt32.Parse(a[2]);
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
                                BlockType = EpbBlock.BlockTypes[(UInt16)new System.ComponentModel.ByteConverter().ConvertFromString(v)];
                            }
                            catch 
                            {
                                BlockType = EpbBlock.GetBlockType(v, "");
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
                                BlockVariant = (byte)new System.ComponentModel.ByteConverter().ConvertFromString(v);
                            }
                            catch
                            {
                                BlockVariant = EpbBlock.GetVariant(BlockType.Id, v);
                            }
                        }
                    }
                },
                {
                    "h|help",
                    "Show this message and exit",
                    v =>
                    {
                        if (v != null)
                        {
                            Command = CommandType.ShowHelp;
                        }
                    }
                }
            };

            try
            {
                InputPaths = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", ExectutableName);
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", ExectutableName);
                PauseOnExit();
                return;
            }

            switch (Command)
            {
            case CommandType.ShowHelp:
                ShowHelp(optionSet);
                PauseOnExit();
                break;

            case CommandType.Create:
                switch (CreateTemplate)
                {
                case CreateTemplateType.BaseRotations:
                    CreateRotations(OutputPath, Hollow);
                    break;
                case CreateTemplateType.BaseBox:
                    CreateBox(OutputPath, Hollow);
                    break;
                case CreateTemplateType.BaseBoxFrame:
                    CreateBoxFrame(OutputPath);
                    break;
                case CreateTemplateType.BasePyramid:
                    CreatePyramid(OutputPath, Hollow);
                    break;
                case CreateTemplateType.BaseBlockTypes:
                    CreateBlockTypes(OutputPath);
                    break;
                case CreateTemplateType.SVHullVariants:
                    CreateHullVariants(OutputPath);
                    break;
                }

                    break;
            case CommandType.Open:
                foreach (string inPath in InputPaths)
                {
                    try
                    {
                        OpenEpb(inPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error on file {inPath}: {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
                    }
                }
                break;
            }

        }

        #region Create
        static EpBlueprint CreateCommon()
        {
            EpBlueprint epb = new EpBlueprint(BlueprintType, Width, Height, Depth);

            EpMetaTag03 metaTag11 = new EpMetaTag03(EpMetaTagKey.UnknownMetax11)
            {
                Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }
            };
            epb.MetaTags.Add(metaTag11.Key, metaTag11);

            EpMetaTagUInt16 metaTagUInt16 = new EpMetaTagUInt16(EpMetaTagKey.UnknownMetax01)
            {
                Value = 0x0000
            };
            epb.MetaTags.Add(metaTagUInt16.Key, metaTagUInt16);

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

            EpMetaTagUInt16 metaTag05 = new EpMetaTagUInt16(EpMetaTagKey.UnknownMetax05)
            {
                Value = 0x0000
            };
            epb.MetaTags.Add(metaTag05.Key, metaTag05);

            EpMetaTag02 metaTag04 = new EpMetaTag02(EpMetaTagKey.UnknownMetax04)
            {
                Value = 0,
                Unknown = 0
            };
            epb.MetaTags.Add(metaTag04.Key, metaTag04);

            EpMetaTag04 metaTag06 = new EpMetaTag04(EpMetaTagKey.UnknownMetax06)
            {
                Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
            };
            epb.MetaTags.Add(metaTag06.Key, metaTag06);

            EpMetaTagString metaTag07 = new EpMetaTagString(EpMetaTagKey.BlueprintName)
            {
                Value = ""
            };
            epb.MetaTags.Add(metaTag07.Key, metaTag07);

            EpMetaTag05 metaTag09 = new EpMetaTag05(EpMetaTagKey.CreationTime)
            {
                Value = DateTime.Now,
                Unknown = 0
            };
            epb.MetaTags.Add(metaTag09.Key, metaTag09);

            EpMetaTag02 metaTag08 = new EpMetaTag02(EpMetaTagKey.BuildVersion)
            {
                Value = 1838,
                Unknown = 0
            };
            epb.MetaTags.Add(metaTag08.Key, metaTag08);

            EpMetaTagString creatorIdTag = new EpMetaTagString(EpMetaTagKey.CreatorId)
            {
                Value = CreatorId
            };
            epb.MetaTags.Add(creatorIdTag.Key, creatorIdTag);

            EpMetaTagString creatorNameTag = new EpMetaTagString(EpMetaTagKey.CreatorName)
            {
                Value = CreatorName
            };
            epb.MetaTags.Add(creatorNameTag.Key, creatorNameTag);

            EpMetaTagString ownerIdTag = new EpMetaTagString(EpMetaTagKey.OwnerId)
            {
                Value = OwnerId
            };
            epb.MetaTags.Add(ownerIdTag.Key, ownerIdTag);

            EpMetaTagString ownerNameTag = new EpMetaTagString(EpMetaTagKey.OwnerName)
            {
                Value = OwnerName
            };
            epb.MetaTags.Add(ownerNameTag.Key, ownerNameTag);

            EpMetaTagString metaTag10 = new EpMetaTagString(EpMetaTagKey.DisplayName) { Value = "" };
            epb.MetaTags.Add(metaTag10.Key, metaTag10);

            EpMetaTag05 metaTag12 = new EpMetaTag05(EpMetaTagKey.UnknownMetax12)
            {
                Value = DateTime.MinValue,
                Unknown = 0
            };
            epb.MetaTags.Add(metaTag12.Key, metaTag12);

            return epb;
        }

        static void CreateRotations(string path, bool hollow)
        {
            Width = 3;
            Height = 3;
            Depth = (uint)Enum.GetValues(typeof(EpbBlock.EpbBlockRotation)).Length * 2;
            EpBlueprint epb = CreateCommon();

            EpbBlock block;

            byte x = 0;
            foreach (EpbBlock.EpbBlockRotation rot in Enum.GetValues(typeof(EpbBlock.EpbBlockRotation)))
            {
                block = new EpbBlock(new EpbBlockPos(x, 0, 0 )) { BlockType = BlockType, Variant = BlockVariant };
                block.SetColour(EpbColourIndex.Red, EpbBlock.FaceIndex.Right);
                block.SetColour(EpbColourIndex.BrightGreen, EpbBlock.FaceIndex.Top);
                block.SetColour(EpbColourIndex.Blue, EpbBlock.FaceIndex.Front);
                block.SetColour(EpbColourIndex.Cyan, EpbBlock.FaceIndex.Left);
                block.SetColour(EpbColourIndex.Pink, EpbBlock.FaceIndex.Bottom);
                block.SetColour(EpbColourIndex.Yellow, EpbBlock.FaceIndex.Back);
                block.Rotation = rot;
                epb.SetBlock(block);
                x += 2;
            }

            block = new EpbBlock(new EpbBlockPos(2, 0, 0 )) { BlockType = BlockType, Variant = BlockVariant };
            block.SetColour(EpbColourIndex.Red);
            epb.SetBlock(block);
            block = new EpbBlock(new EpbBlockPos(0, 2, 0)) { BlockType = BlockType, Variant = BlockVariant };
            block.SetColour(EpbColourIndex.BrightGreen);
            epb.SetBlock(block);

            // Write the file:
            using (FileStream stream = File.Create(path))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(epb);
                }
            }
        }

        static void CreateBox(string path, bool hollow)
        {
            EpBlueprint epb = CreateCommon();

            for (int z = 0; z < Depth; z++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        bool isInterior = (x > 0 && x < (Width - 1))
                                          && (y > 0 && y < (Height - 1))
                                          && (z > 0 && z < (Depth - 1));

                        if (!isInterior || !hollow)
                        {
                            EpbBlock block = new EpbBlock(new EpbBlockPos((byte)x, (byte)y, (byte)z)) { BlockType = BlockType, Variant = BlockVariant };
                            block.SetColour(isInterior ? EpbColourIndex.Pink : EpbColourIndex.None);
                            block.SetTexture(14, (x % 2) == 1);
                            block.SetSymbol(1, (EpbBlock.SymbolRotation)(x % 4), EpbBlock.FaceIndex.Back);
                            block.SetSymbol(2, face: EpbBlock.FaceIndex.Right);
                            block.SetSymbol(3, face: EpbBlock.FaceIndex.Front);
                            block.SetSymbol(4, face: EpbBlock.FaceIndex.Left);
                            block.SetSymbol(5, face: EpbBlock.FaceIndex.Top);
                            block.SetSymbol(6, face: EpbBlock.FaceIndex.Bottom);
                            epb.SetBlock(block);
                        }
                    }
                }
            }

            //// Create Device list:
            //EpbDeviceGroup group = new EpbDeviceGroup
            //{
            //    Name = "Core",
            //    DeviceGroupUnknown03 = 0x01,
            //    DeviceGroupUnknown01 = 0x01,
            //    Shortcut = 0xff
            //};
            //EpbDeviceGroupEntry core = new EpbDeviceGroupEntry
            //{
            //    Pos = 0x80000800, //new byte[] {0x00, 0x08, 0x00, 0x80},
            //    Name = ""
            //};
            //group.Entries.Add(core);
            //epb.DeviceGroups.Add(group);

            //group = new EpbDeviceGroup
            //{
            //    Name = "Ungrouped",
            //    DeviceGroupUnknown03 = 0x01,
            //    DeviceGroupUnknown01 = 0x00,
            //    Shortcut = 0xff
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
            for (int z = 0; z < Depth; z++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        bool a = x % (Width  - 1) == 0;
                        bool b = y % (Height - 1) == 0;
                        bool c = z % (Depth  - 1) == 0;
                        int d = (a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0);
                        if (d >= 2)
                        {
                            epb.SetBlock(new EpbBlock(new EpbBlockPos((byte)x, (byte)y, (byte)z)) { BlockType = BlockType, Variant = BlockVariant });
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

            UInt32 w = Width;
            UInt32 h = Height;
            UInt32 d = Depth;

            for (int y = 0; y < h; y++)
            {
                for (int z = y; z < d; z++)
                {
                    for (int x = y; x < w; x++)
                    {
                        bool isBackEdge  = (z == y);
                        bool isFrontEdge = (z == (d - 1));
                        bool isLeftEdge  = (x == y);
                        bool isRightEdge = (x == (w - 1));
                        bool isInterior  = !isBackEdge && ! isFrontEdge && !isRightEdge && !isLeftEdge && y > 0 && y < (h - 1);

                        EpbBlock.EpbBlockType     t = BlockType;
                        EpbBlock.EpbBlockRotation r = EpbBlock.EpbBlockRotation.PzPy;
                        byte                      v = BlockVariant;
                        byte[]                    c = new byte[] { 0, 0, 0, 0, 0, 0 };

                        if (isBackEdge && isLeftEdge)
                        {
                            t = EpbBlock.GetBlockType("HullFullLarge", "Corner Large A");
                            v = EpbBlock.GetVariant(t.Id, "Corner Large A");
                            r = EpbBlock.EpbBlockRotation.PxPy;
                        }
                        else if (isBackEdge && isRightEdge)
                        {
                            t = EpbBlock.GetBlockType("HullFullLarge", "Corner Large A"); 
                            v = EpbBlock.GetVariant(t.Id, "Corner Large A");
                            r = EpbBlock.EpbBlockRotation.PzPy;
                        }
                        else if (isFrontEdge && isLeftEdge)
                        {
                            t = EpbBlock.GetBlockType("HullFullLarge", "Corner Large A");
                            v = EpbBlock.GetVariant(t.Id, "Corner Large A");
                            r = EpbBlock.EpbBlockRotation.NzPy;
                        }
                        else if (isFrontEdge && isRightEdge)
                        {
                            t = EpbBlock.GetBlockType("HullFullLarge", "Corner Large A");
                            v = EpbBlock.GetVariant(t.Id, "Corner Large A");
                            r = EpbBlock.EpbBlockRotation.NxPy;
                        }
                        else if (isBackEdge)
                        {
                            t = EpbBlock.GetBlockType("HullFullLarge", "Slope");
                            v = EpbBlock.GetVariant(t.Id, "Slope");
                            r = EpbBlock.EpbBlockRotation.NzPy;
                        }
                        else if (isFrontEdge)
                        {
                            t = EpbBlock.GetBlockType("HullFullLarge", "Slope");
                            v = EpbBlock.GetVariant(t.Id, "Slope");
                            r = EpbBlock.EpbBlockRotation.PzPy;
                        }
                        else if (isLeftEdge)
                        {
                            t = EpbBlock.GetBlockType("HullFullLarge", "Slope");
                            v = EpbBlock.GetVariant(t.Id, "Slope");
                            r = EpbBlock.EpbBlockRotation.NxPy;
                        }
                        else if (isRightEdge)
                        {
                            t = EpbBlock.GetBlockType("HullFullLarge", "Slope");
                            v = EpbBlock.GetVariant(t.Id, "Slope");
                            r = EpbBlock.EpbBlockRotation.PxPy;
                        }

                        if (!isInterior || !hollow)
                        {
                            EpbBlock block = new EpbBlock(new EpbBlockPos((byte)x, (byte)y, (byte)z)) {BlockType = t, Rotation = r, Variant = v};
                            block.SetColour(isInterior ? EpbColourIndex.Pink : EpbColourIndex.None);
                            epb.SetBlock(block);
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

        static void CreateBlockTypes(string path)
        {
            UInt16[] allBlockTypes = new UInt16[] { 1, 1003, 1004, 1005, 1006, 1007, 1011, 1012, 1013, 1027, 1028, 1035, 1073, 1077, 1078, 1080, 1081, 1082, 1083, 1084, 1085, 1086, 1087, 1088, 1096, 1097, 1099, 1101, 1113, 1114, 1115, 1121, 1122, 1125, 114, 1143, 1185, 1189, 1204, 1206, 1232, 1260, 1262, 1264, 1273, 1274, 1275, 1276, 1277, 1279, 1282, 1283, 1284, 1285, 1288, 1289, 1290, 1291, 1292, 1293, 1296, 1298, 1299, 1320, 1335, 1362, 1364, 1365, 1366, 1387, 1388, 1393, 1394, 1405, 1406, 1407, 1408, 1410, 1411, 1412, 1413, 1414, 1415, 1416, 1423, 1425, 1426, 1491, 1493, 1495, 1496, 1501, 1502, 1503, 1515, 256, 260, 261, 262, 263, 265, 270, 273, 274, 275, 278, 280, 281, 285, 286, 289, 291, 333, 334, 388, 389, 390, 397, 398, 400, 401, 406, 407, 409, 410, 411, 412, 413, 416, 437, 442, 443, 444, 446, 461, 462, 468, 492, 498, 520, 541, 543, 544, 555, 560, 564, 565, 566, 57, 583, 584, 612, 613, 615, 617, 620, 621, 623, 635, 636, 637, 638, 651, 653, 658, 672, 673, 674, 676, 677, 679, 681, 682, 685, 686, 691, 692, 704, 705, 714, 717, 727, 732, 770, 771, 79, 795, 796, 797, 798, 80, 801, 802, 805, 807, 81, 816, 82, 83, 84, 85, 884, 885, 90, 91, 95, 950, 951, 952, 953, 954, 960, 962, 965, 966, 967, 968, 969, 977, 983, 984, 988, 989, 992, 994, 0x101, 0x193, 0x194 };
            int start = 0;
            int length = 15;
            UInt32[] blockTypes = new UInt32[length];
            Array.Copy(allBlockTypes, start, blockTypes, 0, length);
            Width = (UInt32)blockTypes.Length * 14;
            Height = 1;
            Depth = 1;

            EpBlueprint epb = CreateCommon();
            byte i = 0;
            foreach (UInt16 bt in blockTypes )
            {
                epb.SetBlock(new EpbBlock(new EpbBlockPos(i, 0, 0)) { BlockType = EpbBlock.BlockTypes[bt], Variant = BlockVariant });
                i += 14;
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

        static void CreateHullVariants(string path)
        {
            Width = 16;
            Height = 1;
            Depth = 20;
            EpBlueprint epb = CreateCommon();
            epb.Type = EpBlueprint.EpbType.SmallVessel;

            UInt16[] types = new UInt16[] { 381, 382, 1791}; // HullFullSmall, HullThinSmall, HullExtendedSmall
            int i = 0;
            foreach (UInt16 t in types)
            {
                foreach (string variantName in EpbBlock.BlockVariants[t])
                {
                    byte x = (byte)((i % 8) * 2);
                    byte z = (byte)((i / 8) * 2);
                    EpbBlock.EpbBlockType bt = EpbBlock.BlockTypes[t];
                    byte v = EpbBlock.GetVariant(t, variantName);
                    EpbBlock block =
                        new EpbBlock(new EpbBlockPos(x, 0, z))
                        {
                            BlockType = bt,
                            Variant = v,
                            Colours =
                            {
                                [0] = EpbColourIndex.Red,
                                [1] = EpbColourIndex.BrightGreen,
                                [2] = EpbColourIndex.Blue,
                                [3] = EpbColourIndex.Cyan,
                                [4] = EpbColourIndex.Purple,
                                [5] = EpbColourIndex.Yellow
                            }
                        };
                    epb.SetBlock(block);
                    i++;
                }
            }

            // Write the file:
            FileStream stream = null;
            try
            {
                stream = File.Create(path);
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    stream = null; // The stream will be closed when the writer is closed so set to null so that we don't try to close it twice.
                    writer.Write(epb);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        #endregion Create

        #region Open
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
        #endregion Open

        static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine("Usage: {0} [OPTIONS]+ [FILE]+", ExectutableName);
            Console.WriteLine();
            Console.WriteLine("Operate on files in the Empyrion blueprint (epb) TPC format.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("    epb -c BaseBox -b 412 -v 0 -s 10,10,10 -o Box.epb");
            Console.WriteLine("    epb -c BaseBox --hollow -b 412 -v 0 -s 10,10,10 -o BoxHollow.epb");
            Console.WriteLine("    epb -c BaseFrame -b 412 -v 0 -s 10,10,10 -o Frame.epb");
            Console.WriteLine("    epb -c BasePyramid -s 8,4,8 -o Pyramid.epb");
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
