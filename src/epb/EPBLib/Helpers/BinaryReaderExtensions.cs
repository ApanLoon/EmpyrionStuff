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
            Console.WriteLine($"Unknown02: {BitConverter.ToString(unknown02).Replace("-", "")}");

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
            Console.WriteLine($"Unknown05: {BitConverter.ToString(unknown05).Replace("-", "")}");

            epb.DeviceGroups = reader.ReadEpbDeviceGroups(ref bytesLeft);
            Console.WriteLine($"DeviceGroups ({epb.DeviceGroups.Count}):");
            foreach (EpbDeviceGroup group in epb.DeviceGroups)
            {
                Console.WriteLine($"    {group.Name} (Flags=0x{group.Flags:x4})");
                foreach (EpbDeviceGroupEntry entry in group.Entries)
                {
                    Console.WriteLine($"        Unknown={BitConverter.ToString(entry.Unknown).Replace("-", "")} \"{entry.Name}\"");
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
            Console.WriteLine($"BeforeZIP: {BitConverter.ToString(unknown8).Replace("-", "")}");

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
            bytesLeft = reader.ReadEpbMatrix(epb, "Blocks", length, (r, e, x, y, z, b) =>
            {
                EpbBlock.EpbBlockType type = (EpbBlock.EpbBlockType)r.ReadByte();
                byte rotation = r.ReadByte();
                byte unknown2 = r.ReadByte();
                byte variant = r.ReadByte();
                blockCount++;
                EpbBlock block = new EpbBlock()
                {
                    BlockType = type,
                    Rotation = rotation,
                    Unknown00 = unknown2,
                    Variant = variant
                };
                epb.SetBlock(block, x, y, z);
                Console.WriteLine($"    {blockCount} ({x}, {y}, {z}): Type={type} Rot=0x{rotation:x2} Unknown2=0x{unknown2:x2} Variant=0x{variant:x2}");
                return b - 4;
            });

            int unknown01Count = 0;
            bytesLeft = reader.ReadEpbMatrix(epb, "unknown01", length, (r, e, x, y, z, b) =>
            {
                byte unknown01a = r.ReadByte();
                byte unknown01b = r.ReadByte();
                byte unknown01c = r.ReadByte();
                byte unknown01d = r.ReadByte();
                unknown01Count++;
                Console.WriteLine($"    {unknown01Count} ({x}, {y}, {z}): 0x{unknown01a:x2} 0x{unknown01b:x2} 0x{unknown01c:x2} 0x{unknown01d:x2}");
                return b - 4;
            });

            int unknown02Count = reader.ReadByte();
            bytesLeft -= 1;
            if (unknown02Count == 0)
            {
                unknown02Count = (int)(epb.Width * epb.Height * epb.Depth); // blockCount;
            }
            byte[] unknown02 = reader.ReadBytes(unknown02Count);
            bytesLeft -= unknown02Count;
            Console.WriteLine($"unknown02: {BitConverter.ToString(unknown02).Replace("-", "")}");

            int colourCount = 0;
            bytesLeft = reader.ReadEpbMatrix(epb, "Colour", length, (r, e, x, y, z, b) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
                UInt32 bits = r.ReadUInt32();
                for (int i = 0; i < 6; i++)
                {
                    block.FaceColours[i] = (byte)(bits & 0x1f);
                    bits = bits >> 5;
                }
                colourCount++;
                Console.WriteLine($"    {colourCount} ({x}, {y}, {z}): {string.Join(", ", block.FaceColours)}");
                return b - 4;
            });

            int textureCount = 0;
            bytesLeft = reader.ReadEpbMatrix(epb, "Texture", length, (r, e, x, y, z, b) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
                UInt64 bits = r.ReadUInt64();
                for (int i = 0; i < 6; i++)
                {
                    block.FaceTextures[i] = (byte)(bits & 0x3f);
                    bits = bits >> 6;
                }
                textureCount++;
                Console.WriteLine($"    {textureCount} ({x}, {y}, {z}): {string.Join(", ", block.FaceTextures)}");
                return b - 8;
            });

            if (version >= 20)
            {
                int unknown03Count = 0;
                bytesLeft = reader.ReadEpbMatrix(epb, "Unknown03", length, (r, e, x, y, z, b) =>
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
                bytesLeft = reader.ReadEpbMatrix(epb, "Unknown04", length, (r, e, x, y, z, b) =>
                {
                    UInt32 unknown04a = r.ReadUInt32();
                    unknown04Count++;
                    Console.WriteLine($"    {unknown04Count} ({x}, {y}, {z}): 0x{unknown04a:x8}");
                    return b - 4;
                });
            }

            int symbolCount = 0;
            bytesLeft = reader.ReadEpbMatrix(epb, "Symbol", length, (r, e, x, y, z, b) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
                UInt32 bits = r.ReadUInt32();
                for (int i = 0; i < 6; i++)
                {
                    block.FaceSymbols[i] = (byte)(bits & 0x1f);
                    bits = bits >> 5;
                }
                block.SymbolPage = (byte)bits;
                symbolCount++;
                Console.WriteLine($"    {symbolCount} ({x}, {y}, {z}): Page={block.SymbolPage} {string.Join(", ", block.FaceSymbols)}");
                return b - 4;
            });

            if (version >= 20) // TODO: I have no idea when this appeared
            {
                int unknown05Count = 0;
                bytesLeft = reader.ReadEpbMatrix(epb, "Unknown05", length, (r, e, x, y, z, b) =>
                {
                    byte unknown05a = r.ReadByte();
                    byte unknown05b = r.ReadByte();
                    byte unknown05c = r.ReadByte();
                    byte unknown05d = r.ReadByte();
                    unknown05Count++;
                    Console.WriteLine($"    {unknown05Count} ({x}, {y}, {z}): 0x{unknown05a:x2} 0x{unknown05b:x2} 0x{unknown05c:x2} 0x{unknown05d:x2}");
                    return b - 4;
                });
            }

            int unknown06Count = 0;
            if (version <= 12)
            {
                unknown06Count = 4;
            }
            else if (version <= 17)
            {
                unknown06Count = 12;
            }
            else
            {
                unknown06Count = 14;
            }
            byte[] unknown06 = reader.ReadBytes(unknown06Count);
            Console.WriteLine($"Unknown06: {unknown06Count:x8} {BitConverter.ToString(unknown06).Replace("-", "")}");

            byte[] remainingData = reader.ReadBytes((int)(bytesLeft));
            Console.WriteLine($"Remaining data: {BitConverter.ToString(remainingData).Replace("-", "")}");
        }

        public static long ReadEpbMatrix(this BinaryReader reader, EpBlueprint epb, string name, long bytesLeft, Func<BinaryReader, EpBlueprint, UInt32, UInt32, UInt32, long, long> func)
        {
            UInt32 matrixSize = reader.ReadUInt32();
            byte[] matrix = reader.ReadBytes((int)matrixSize);
            bytesLeft -= 4;
            Console.WriteLine($"{name} Matrix: MatrixSize=0x{matrixSize:x8} Matrix={BitConverter.ToString(matrix).Replace("-", "")}");
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
            byte len = reader.ReadByte();
            string s = (len == 0) ? "" : System.Text.Encoding.ASCII.GetString(reader.ReadBytes(len));
            bytesLeft -= 1 + len;
            return s;
        }


    }
}
