using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace EPBLib.Helpers
{
    public static class BinaryReaderExtensions
    {
        #region EpBlueprint
        public static readonly UInt32 EpbIdentifier = 0x78945245;
        public static readonly byte[] ZipDataStartPattern = new byte[] { 0x00, 0x00, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00 };


        public static EpBlueprint ReadEpBlueprint(this BinaryReader reader, ref long bytesLeft)
        {

            UInt32 identifier = reader.ReadUInt32();
            if (identifier != EpbIdentifier)
            {
                throw new Exception($"Unknown file identifier. 0x{identifier:x4}");
            }
            UInt32 version = reader.ReadUInt32();
            EpBlueprint.EpbType type = (EpBlueprint.EpbType)reader.ReadByte();
            UInt32 width = reader.ReadUInt32();
            UInt32 height = reader.ReadUInt32();
            UInt32 depth = reader.ReadUInt32();
            bytesLeft -= 4 + 4 + 1 + 4 + 4 + 4;
            Console.WriteLine($"Version:  {version}");
            Console.WriteLine($"Type:     {type}");
            Console.WriteLine($"Width:    {width}");
            Console.WriteLine($"Height:   {height}");
            Console.WriteLine($"Depth:    {depth}");

            EpBlueprint epb = new EpBlueprint(type, width, height, depth);

            UInt16 unknown01 = reader.ReadUInt16();
            bytesLeft -= 2;

            Console.WriteLine($"Unkown01: {unknown01}");

            epb.MetaTags = reader.ReadEpMetaTagDictionary(ref bytesLeft);
            foreach (EpMetaTag tag in epb.MetaTags.Values)
            {
                Console.WriteLine(tag.ToString());
            }


            // TODO: Funky
            int nUnknown02;
            if (version <= 4)
            {
                nUnknown02 = 37;
            }
            else if (version <= 12)
            {
                nUnknown02 = 40;
            }
            else if (version <= 17)
            {
                nUnknown02 = 26;
            }
            else
            {
                nUnknown02 = 30;
            }
            byte[] unknown02 = reader.ReadBytes(nUnknown02);
            bytesLeft -= nUnknown02;
            Console.WriteLine($"Unknown02: {unknown02.ToHexString()}");

            if (version > 12)
            {
                UInt16 nBlockCounts = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"BlockCounts ({nBlockCounts:x4})");
                UInt32 nBlocks = 0;
                for (int i = 0; i < nBlockCounts; i++)
                {
                    byte blockType = reader.ReadByte();
                    byte unknown = reader.ReadByte();
                    UInt32 blockCount = reader.ReadUInt32();
                    bytesLeft -= 6;
                    Console.WriteLine($"    BlockType=0x{blockType:x2} Unknown=0x{unknown:x2} Count={blockCount}");

                    nBlocks += blockCount;
                }
                Console.WriteLine($"Total number of blocks: {nBlocks}");
            }

            byte[] unknown05 = reader.ReadBytes(1);
            bytesLeft -= 1;
            Console.WriteLine($"Unknown05: {unknown05.ToHexString()}");

            epb.DeviceGroups = reader.ReadEpbDeviceGroups(ref bytesLeft);
            Console.WriteLine($"DeviceGroups ({epb.DeviceGroups.Count}):");
            foreach (EpbDeviceGroup group in epb.DeviceGroups)
            {
                Console.WriteLine($"    {group.Name} (Flags=0x{group.Flags:x4})");
                foreach (EpbDeviceGroupEntry entry in group.Entries)
                {
                    Console.WriteLine($"        Unknown={entry.Unknown.ToHexString()} \"{entry.Name}\"");
                }
            }

            // There might be a number of unparsed bytes remaining at this point, so read the rest and search for the PKZip header:
            byte[] buf = reader.ReadBytes((int)bytesLeft);
            int dataStart = buf.IndexOf(ZipDataStartPattern);
            if (dataStart == -1)
            {
                throw new Exception("ReadHeader: Unable to locate ZipDataStart.");
            }
            byte[] unknown8 = buf.Take(dataStart).ToArray();
            bytesLeft -= dataStart;
            Console.WriteLine($"BeforeZIP: {unknown8.ToHexString()}");

            byte[] zippedData = buf.Skip(dataStart).Take((int)bytesLeft).ToArray();
            zippedData[0] = 0x50;
            zippedData[1] = 0x4b;
            using (ZipFile zf = new ZipFile(new MemoryStream(zippedData)))
            {
                zf.IsStreamOwner = true;
                foreach (ZipEntry entry in zf)
                {
                    if (!entry.IsFile || entry.Name != "0")
                    {
                        Console.WriteLine($"Skipping ZIP entry: {entry.Name} ({entry.Size} bytes)");
                        continue;
                    }

                    byte[] zipBuffer = new byte[4096];
                    Stream zipStream = zf.GetInputStream(entry);

                    using (BinaryReader zipReader = new BinaryReader(zipStream))
                    {
                        zipReader.ReadEpbBlocks(epb, version, entry.Size);
                    }
                }
            }

            return epb;
        }
        #endregion EpBlueprint

        #region EpbBlocks

        public static void ReadEpbBlocks(this BinaryReader reader, EpBlueprint epb, UInt32 version, long length)
        {
            long bytesLeft = length;
            int blockCount = 0;
            Console.WriteLine("Block matrix");
            bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
            {
                UInt32 data = reader.ReadUInt32();
                blockCount++;
                EpbBlock block = new EpbBlock()
                {
                    BlockType = (EpbBlock.EpbBlockType)(data & 0x7ff),
                    Rotation  = (byte)((data >> 11) & 0x1f),
                    Unknown00 = (UInt16)((data >> 16) & 0x3ff),
                    Variant   = (byte)((data >> 26) & 0x1f)
                };
                epb.SetBlock(block, x, y, z);
                Console.WriteLine($"    {blockCount} ({x}, {y}, {z}): {data:x08} Type={block.BlockType} Rot=0x{block.Rotation:x2} Unknown2=0x{block.Unknown00:x3} Variant={block.VariantName}");
                return b - 4;
            });

            int unknown01Count = 0;
            Console.WriteLine("Unknown01 matrix");
            bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
            {
                byte unknown01a = r.ReadByte();
                byte unknown01b = r.ReadByte();
                byte unknown01c = r.ReadByte();
                byte unknown01d = r.ReadByte();
                unknown01Count++;
                Console.WriteLine(
                    $"    {unknown01Count} ({x}, {y}, {z}): 0x{unknown01a:x2} 0x{unknown01b:x2} 0x{unknown01c:x2} 0x{unknown01d:x2}");
                return b - 4;
            });

            int unknown02Count = reader.ReadByte();
            bytesLeft -= 1;
            if (unknown02Count == 0)
            {
                unknown02Count = (int) (epb.Width * epb.Height * epb.Depth); // blockCount;
            }

            byte[] unknown02 = reader.ReadBytes(unknown02Count);
            bytesLeft -= unknown02Count;
            Console.WriteLine($"unknown02: {unknown02.ToHexString()}");

            int colourCount = 0;
            Console.WriteLine("Colour matrix");
            bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
                UInt32 bits = r.ReadUInt32();
                for (int i = 0; i < 6; i++)
                {
                    block.FaceColours[i] = (byte) (bits & 0x1f);
                    bits = bits >> 5;
                }

                colourCount++;
                Console.WriteLine($"    {colourCount} ({x}, {y}, {z}): {string.Join(", ", block.FaceColours)}");
                return b - 4;
            });

            int textureCount = 0;
            Console.WriteLine("Texture matrix");
            bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
                UInt64 bits = r.ReadUInt64();
                for (int i = 0; i < 6; i++)
                {
                    block.FaceTextures[i] = (byte) (bits & 0x3f);
                    bits = bits >> 6;
                }

                textureCount++;
                Console.WriteLine($"    {textureCount} ({x}, {y}, {z}): {string.Join(", ", block.FaceTextures)}");
                return b - 8;
            });

            if (version >= 20)
            {
                int unknown03Count = 0;
                Console.WriteLine("Unknown03 matrix");
                bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                {
                    byte unknown03a = r.ReadByte();
                    unknown03Count++;
                    Console.WriteLine($"    {unknown03Count} ({x}, {y}, {z}): 0x{unknown03a:x2}");
                    return b - 1;
                });
            }
            else
            {
                int unknown04Count = 0;
                Console.WriteLine("Unknown04 matrix");
                bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                {
                    UInt32 unknown04a = r.ReadUInt32();
                    unknown04Count++;
                    Console.WriteLine($"    {unknown04Count} ({x}, {y}, {z}): 0x{unknown04a:x8}");
                    return b - 4;
                });
            }

            int symbolCount = 0;
            Console.WriteLine("Symbol matrix");
            bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
                UInt32 bits = r.ReadUInt32();
                for (int i = 0; i < 6; i++)
                {
                    block.FaceSymbols[i] = (byte) (bits & 0x1f);
                    bits = bits >> 5;
                }

                block.SymbolPage = (byte) bits;
                symbolCount++;
                Console.WriteLine(
                    $"    {symbolCount} ({x}, {y}, {z}): Page={block.SymbolPage} {string.Join(", ", block.FaceSymbols)}");
                return b - 4;
            });

            if (version >= 20) // TODO: I have no idea when this appeared
            {
                int unknown05Count = 0;
                Console.WriteLine("Unknown05 matrix");
                bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                {
                    byte unknown05a = r.ReadByte();
                    byte unknown05b = r.ReadByte();
                    byte unknown05c = r.ReadByte();
                    byte unknown05d = r.ReadByte();
                    unknown05Count++;
                    Console.WriteLine(
                        $"    {unknown05Count} ({x}, {y}, {z}): 0x{unknown05a:x2} 0x{unknown05b:x2} 0x{unknown05c:x2} 0x{unknown05d:x2}");
                    return b - 4;
                });
            }

            UInt16 unknown06Count = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Unknown06 ({unknown06Count})");
            for (int i = 0; i < unknown06Count; i++)
            {
                byte[] unknown06a = reader.ReadBytes(5);
                bytesLeft -= 5;
                Console.WriteLine($"    unknown06a: {unknown06a.ToHexString()}");
                UInt16 nTags = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"    BlockTags: {nTags}");
                for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                {
                    EpbBlockTag tag = reader.ReadEpbBlockTag(ref bytesLeft);
                    Console.WriteLine($"        {tagIndex}: {tag}");
                }
            }

            UInt16 unknown07Count = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Unknown07 ({unknown07Count})");


            UInt16 signalCount = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Signals ({signalCount})");
            for (int i = 0; i < signalCount; i++)
            {
                byte[] signalUnknown = reader.ReadBytes(1);
                bytesLeft -= 1;
                Console.WriteLine($"    Unknown: {signalUnknown.ToHexString()}");
                UInt16 nTags = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"    BlockTags: {nTags}");
                for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                {
                    EpbBlockTag tag = reader.ReadEpbBlockTag(ref bytesLeft);
                    Console.WriteLine($"        {tagIndex}: {tag}");
                }
            }

            if (version < 20)
            {
                // Check CV_Prefab_Tier2.epb.hex for v18
            }
            else
            {
                UInt16 logicCount = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"Logic ({logicCount})");
                for (int i = 0; i < logicCount; i++)
                {
                    if (i > 0)
                    {
                        byte logicUnknown01 = reader.ReadByte();
                        bytesLeft -= 1;
                        Console.WriteLine($"    LogicUnknown01: 0x{logicUnknown01:x2}");
                    }

                    string name = reader.ReadEpString(ref bytesLeft);
                    UInt16 nRules = reader.ReadUInt16();
                    bytesLeft -= 2;
                    byte logicUnknown02 = reader.ReadByte();
                    bytesLeft -= 1;
                    UInt16 nTags = reader.ReadByte();
                    bytesLeft -= 1;
                    Console.WriteLine($"    Name:           {name}");
                    Console.WriteLine($"    nRules:         {nRules}");
                    Console.WriteLine($"    LogicUnknown02: 0x{logicUnknown02:x2}");
                    Console.WriteLine($"    nTags:          {nTags}");
                    for (int j = 0; j < nRules; j++)
                    {
                        byte ruleUnknown01 = reader.ReadByte();
                        bytesLeft -= 1;
                        Console.WriteLine($"        RuleUnknown01: 0x{ruleUnknown01:x2}");
                        if (j > 0)
                        {
                            byte ruleUnknown02 = reader.ReadByte();
                            bytesLeft -= 1;
                            nTags = reader.ReadUInt16();
                            bytesLeft -= 2;
                            Console.WriteLine($"        RuleUnknown02: 0x{ruleUnknown02:x2}");
                            Console.WriteLine($"        nTags:         {nTags}");
                        }

                        for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                        {
                            EpbBlockTag tag = reader.ReadEpbBlockTag(ref bytesLeft);
                            Console.WriteLine($"            {tagIndex}: {tag}");
                        }

                    }
                }

                byte unknown08 = reader.ReadByte();
                bytesLeft -= 1;
                Console.WriteLine($"Unknown08: 0x{unknown08:x2}");

                UInt16 logicOpsCount = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"LogicOps ({logicOpsCount})");
                for (int i = 0; i < logicOpsCount; i++)
                {
                    byte logicOpUnknown01 = reader.ReadByte();
                    bytesLeft -= 1;
                    UInt16 nTags = reader.ReadUInt16();
                    bytesLeft -= 2;
                    Console.WriteLine($"    LogicOpUnknown01: 0x{logicOpUnknown01:x2}");
                    Console.WriteLine($"    nTags:            {nTags}");
                    for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                    {
                        EpbBlockTag tag = reader.ReadEpbBlockTag(ref bytesLeft);
                        Console.WriteLine($"        {tagIndex}: {tag}");
                    }
                }
            }




            byte[] remainingData = reader.ReadBytes((int)(bytesLeft));
            Console.WriteLine($"Remaining data: {remainingData.ToHexString()}");
        }

        public static long ReadEpbMatrix(this BinaryReader reader, EpBlueprint epb, long bytesLeft, Func<BinaryReader, EpBlueprint, UInt32, UInt32, UInt32, long, long> func)
        {
            UInt32 matrixSize = reader.ReadUInt32();
            byte[] matrix = reader.ReadBytes((int)matrixSize);
            bytesLeft -= 4;
            if (func == null)
            {
                return bytesLeft;
            }

            bool[] m = matrix.ToBoolArray();
            for (UInt32 z = 0; z < epb.Depth; z++)
            {
                for (UInt32 y = 0; y < epb.Height; y++)
                {
                    for (UInt32 x = 0; x < epb.Width; x++)
                    {
                        if (m[z * epb.Width * epb.Height + y * epb.Width + x])
                        {
                            bytesLeft = func(reader, epb, x, y, z, bytesLeft);
                        }
                    }
                }
            }
            return bytesLeft;
        }

        public static EpbBlockTag ReadEpbBlockTag(this BinaryReader reader, ref long bytesLeft)
        {
            EpbBlockTag.TagType type = (EpbBlockTag.TagType)reader.ReadByte();
            bytesLeft -= 1;
            string name = reader.ReadEpString(ref bytesLeft);

            EpbBlockTag tag;
            switch (type)
            {
                case EpbBlockTag.TagType.UInt32:
                    EpbBlockTagUInt32 tagUInt32 = new EpbBlockTagUInt32(name);
                    tagUInt32.Value = reader.ReadUInt32();
                    bytesLeft -= 4;
                    tag = tagUInt32;
                    break;
                case EpbBlockTag.TagType.String:
                    EpbBlockTagString tagString = new EpbBlockTagString(name);
                    tagString.Value = reader.ReadEpString(ref bytesLeft);
                    tag = tagString;
                    break;
                case EpbBlockTag.TagType.Bool:
                    EpbBlockTagBool tagBool = new EpbBlockTagBool(name);
                    tag = tagBool;
                    break;
                case EpbBlockTag.TagType.Colour:
                    EpbBlockTagColour tagColour = new EpbBlockTagColour(name);
                    tagColour.Value = reader.ReadUInt32();
                    bytesLeft -= 4;
                    tag = tagColour;
                    break;
                case EpbBlockTag.TagType.x03:
                    EpbBlockTagx03 tagx03 = new EpbBlockTagx03(name);
                    tagx03.Value = reader.ReadBytes(4);
                    bytesLeft -= 4;
                    tag = tagx03;
                    break;
                default:
                    tag = null;
                    break;
            }
            return tag;
        }
        #endregion EpbBlocks

        #region EpMetaTags

        public static Dictionary<EpMetaTagKey, EpMetaTag> ReadEpMetaTagDictionary(this BinaryReader reader, ref long bytesLeft)
        {
            Dictionary<EpMetaTagKey, EpMetaTag> dictionary = new Dictionary<EpMetaTagKey, EpMetaTag>();
            UInt16 nTags = reader.ReadUInt16();
            bytesLeft -= 2;
            for (int i = 0; i < nTags; i++)
            {
                EpMetaTag tag = reader.ReadEpMetaTag(ref bytesLeft);
                dictionary.Add(tag.Key, tag);
            }
            return dictionary;
        }


        public static EpMetaTag ReadEpMetaTag(this BinaryReader reader, ref long bytesLeft)
        {
            EpMetaTagKey key   = (EpMetaTagKey)reader.ReadInt32();
            EpMetaTagType type = (EpMetaTagType)reader.ReadInt32();
            bytesLeft -= 8;

            EpMetaTag tag;
            switch (type)
            {
                case EpMetaTagType.String:
                    EpMetaTagString tagString = new EpMetaTagString(key);
                    tagString.Value = reader.ReadEpString(ref bytesLeft);
                    tag = tagString;
                    break;
                case EpMetaTagType.Unknownx01:
                    EpMetaTag01 tag01 = new EpMetaTag01(key);
                    tag01.Value = reader.ReadUInt16();
                    bytesLeft -= 2;
                    tag = tag01;
                    break;
                case EpMetaTagType.Unknownx02:
                    EpMetaTag02 tag02 = new EpMetaTag02(key);
                    tag02.Value = reader.ReadBytes(5);
                    bytesLeft -= 5;
                    tag = tag02;
                    break;
                case EpMetaTagType.Unknownx03:
                    EpMetaTag03 tag03 = new EpMetaTag03(key);
                    tag03.Value = reader.ReadBytes(5);
                    bytesLeft -= 5;
                    tag = tag03;
                    break;
                case EpMetaTagType.Unknownx04:
                    EpMetaTag04 tag04 = new EpMetaTag04(key);
                    tag04.Value = reader.ReadBytes(13);
                    bytesLeft -= 13;
                    tag = tag04;
                    break;
                case EpMetaTagType.Unknownx05:
                    EpMetaTag05 tag05 = new EpMetaTag05(key);
                    tag05.Value = reader.ReadBytes(9);
                    bytesLeft -= 9;
                    tag = tag05;
                    break;
                default:
                    tag = null;
                    break;
            }
            return tag;
        }
        #endregion EpMetaTags

        #region EpbDevices

        public static List<EpbDeviceGroup> ReadEpbDeviceGroups(this BinaryReader reader, ref long bytesLeft)
        {
            List<EpbDeviceGroup> groups = new List<EpbDeviceGroup>();
            UInt16 nGroups = reader.ReadUInt16();
            bytesLeft -= 2;
            for (int i = 0; i < nGroups; i++)
            {
                groups.Add(reader.ReadEpbDeviceGroup(ref bytesLeft));
            }
            return groups;
        }
        public static EpbDeviceGroup ReadEpbDeviceGroup(this BinaryReader reader, ref long bytesLeft)
        {
            EpbDeviceGroup group = new EpbDeviceGroup();
            group.Name = reader.ReadEpString(ref bytesLeft);
            group.Flags = reader.ReadUInt16();
            bytesLeft -= 2;
            UInt16 nDevices = reader.ReadUInt16();
            bytesLeft -= 2;
            for (int i = 0; i < nDevices; i++)
            {
                group.Entries.Add(reader.ReadEpbDeviceGroupEntry(ref bytesLeft));
            }
            return group;
        }
        public static EpbDeviceGroupEntry ReadEpbDeviceGroupEntry(this BinaryReader reader, ref long bytesLeft)
        {
            EpbDeviceGroupEntry entry = new EpbDeviceGroupEntry();
            entry.Unknown = reader.ReadBytes(4);
            bytesLeft -= 4;
            entry.Name = reader.ReadEpString(ref bytesLeft);
            return entry;
        }

        #endregion EpbDevices

        public static string ReadEpString(this BinaryReader reader, ref long bytesLeft)
        {
            int len = 0;

            bool more;
            int shift = 0;
            do
            {
                byte b = reader.ReadByte();
                bytesLeft -= 1;
                more = (b & 0x80) != 0;
                len += (b & 0x7f) << shift;
                shift += 7;
            } while (more);

            string s = (len == 0) ? "" : System.Text.Encoding.UTF8.GetString(reader.ReadBytes(len));
            bytesLeft -= len;
            return s;
        }


    }
}
