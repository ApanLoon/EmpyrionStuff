using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using EPBLib.BlockData;
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


            UInt16 unknown02 = reader.ReadUInt16();
            Console.WriteLine($"Unknown02: 0x{unknown02:x4}");

            UInt32 nLights = reader.ReadUInt32();
            Console.WriteLine($"nLights:        {nLights} (0x{nLights:x8})");
            UInt32 unknownCount01 = reader.ReadUInt32();
            Console.WriteLine($"unknownCount01: {unknownCount01} (0x{unknownCount01:x8})");
            UInt32 nDevices = reader.ReadUInt32();
            Console.WriteLine($"nDevices:       {nDevices} (0x{nDevices:x8})");
            UInt32 unknownCount02 = reader.ReadUInt32();
            Console.WriteLine($"unknownCount02: {unknownCount02} (0x{unknownCount02:x8})");
            UInt32 nBlocks = reader.ReadUInt32();
            Console.WriteLine($"nBlocks:        {nBlocks} (0x{nBlocks:x8})");

            if (version >= 14)
            {
                UInt32 unknownCount03 = reader.ReadUInt32();
                Console.WriteLine($"unknownCount03: {unknownCount03} (0x{unknownCount03:x8})");
            }
            if (version >= 18)
            {
                UInt32 nTriangles = reader.ReadUInt32();
                Console.WriteLine($"nTriangles:     {nTriangles} (0x{nTriangles:x8})");
            }

            UInt16 nBlockCounts = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"BlockCounts (0x{nBlockCounts:x4})");
            UInt32 nBlocksTotal = 0;
            for (int i = 0; i < nBlockCounts; i++)
            {
                EpbBlock.EpbBlockType blockType = reader.ReadEpbBlockType();
                if (version <= 12)
                {
                    reader.ReadUInt16(); // Block types were 32 bit in the early versions, but these bytes were probably always zero, so we simply ignore them.
                }

                UInt32 blockCount = reader.ReadUInt32();
                bytesLeft -= 6;
                Console.WriteLine($"    BlockType={EpbBlock.GetBlockTypeName(blockType)} Count={blockCount}");

                nBlocksTotal += blockCount;
            }
            Console.WriteLine($"Total number of blocks: {nBlocksTotal}");

            if (version > 4)
            {
                byte[] unknown05 = reader.ReadBytes(1);
                bytesLeft -= 1;
                Console.WriteLine($"Unknown05: {unknown05.ToHexString()}");
            }

            if (version > 4)
            {
                epb.DeviceGroups = reader.ReadEpbDeviceGroups(version, ref bytesLeft);
            }

            /* TODO: We now assume that we reas all the bytes up to the zipped section correctly and don't skip any bytes.
            byte[] buf = reader.ReadBytes((int)bytesLeft);
            // There might be a number of unparsed bytes remaining at this point, so read the rest and search for the PKZip header:
            int dataStart = buf.IndexOf(ZipDataStartPattern);
            if (dataStart == -1)
            {
                throw new Exception("ReadHeader: Unable to locate ZipDataStart.");
            }
            byte[] unknown8 = buf.Take(dataStart).ToArray();
            bytesLeft -= dataStart;
            Console.WriteLine($"BeforeZIP: {unknown8.ToHexString()}");
            byte[] zippedData = buf.Skip(dataStart).Take((int)bytesLeft).ToArray();
            */
            byte[] zippedData = reader.ReadBytes((int)bytesLeft);

            zippedData[0] = 0x50; // Prior to version 22 this byte was zero, set to 'P'
            zippedData[1] = 0x4b; // Prior to version 22 this byte was zero, set to 'K'
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

        public static EpbBlock.EpbBlockType ReadEpbBlockType(this BinaryReader reader)
        {
            return(EpbBlock.EpbBlockType)reader.ReadUInt16();
        }

        public static void ReadEpbBlocks(this BinaryReader reader, EpBlueprint epb, UInt32 version, long length)
        {
            long bytesLeft = length;

            #region BlockTypes
            Console.WriteLine("Block type matrix");
            int blockCount = 0;
            if (version <= 4)
            {
                for (UInt32 z = 0; z < epb.Depth; z++)
                {
                    for (UInt32 y = 0; y < epb.Height; y++)
                    {
                        for (UInt32 x = 0; x < epb.Width; x++)
                        {
                            UInt32 data = reader.ReadUInt32();
                            bytesLeft -= 4;
                            EpbBlock block = new EpbBlock()
                            {

                                BlockType = (EpbBlock.EpbBlockType)(data & 0x7ff),
                                Rotation = (EpbBlock.EpbBlockRotation)((data >> 11) & 0x1f),
                                Unknown00 = (UInt16)((data >> 16) & 0x3ff),
                                Variant = (byte)((data >> 25) & 0x1f)
                            };
                            if (block.BlockType != 0)
                            {
                                blockCount++;
                                epb.SetBlock(block, x, y, z);
                                Console.Write($"{BitConverter.GetBytes(data).ToHexString()} | 0x{data:x08} {Convert.ToString(data, 2).PadLeft(32, '0')} |");
                                Console.WriteLine($"    {blockCount,5} ({x,4}, {y,4}, {z,4}): Rot={block.Rotation} Unknown2=0x{block.Unknown00:x3} Type={EpbBlock.GetBlockTypeName(block.BlockType)} Variant={block.VariantName}");
                            }
                        }
                    }
                }
            }
            else
            {
                bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                {
                    UInt32 data = reader.ReadUInt32();
                    blockCount++;
                    EpbBlock block = new EpbBlock()
                    {

                        BlockType = (EpbBlock.EpbBlockType)(data & 0x7ff),
                        Rotation = (EpbBlock.EpbBlockRotation)((data >> 11) & 0x1f),
                        Unknown00 = (UInt16)((data >> 16) & 0x3ff),
                        Variant = (byte)((data >> 25) & 0x1f)
                    };
                    epb.SetBlock(block, x, y, z);
                    Console.Write($"{BitConverter.GetBytes(data).ToHexString()} | 0x{data:x08} {Convert.ToString(data, 2).PadLeft(32, '0')} |");
                    Console.WriteLine($"    {blockCount,5} ({x,4}, {y,4}, {z,4}): Rot={block.Rotation} Unknown2=0x{block.Unknown00:x3} Type={EpbBlock.GetBlockTypeName(block.BlockType)} Variant={block.VariantName}");
                    return b - 4;
                });
            }
            #endregion BlockTypes

            #region DamageState
            if (version > 4) // TODO: Verify version, maybe this is newer than this
            {
                int damageStateCount = 0;
                Console.WriteLine("Damage state matrix");
                bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                {
                    UInt16 damage = r.ReadUInt16();
                    damageStateCount++;
                    Console.WriteLine(
                        $"    {damageStateCount,5} ({x,4}, {y,4}, {z,4}): {damage} (0x{damage:x4})");
                    return b - 2;
                });
            }
            #endregion DamageState

            #region Unknown02
            int unknown02Count = reader.ReadByte();
            bytesLeft -= 1;
            if (unknown02Count == 0)
            {
                unknown02Count = (int) (epb.Width * epb.Height * epb.Depth); // blockCount;
            }
            byte[] unknown02 = reader.ReadBytes(unknown02Count);
            bytesLeft -= unknown02Count;
            Console.WriteLine($"unknown02: 0x{unknown02.ToHexString()}");
            #endregion Unknown02

            #region Colours
            if (version > 4)
            {
                int colourCount = 0;
                Console.WriteLine("Colour matrix");
                if (version <= 12)
                {
                    Console.WriteLine("TODO: Read but not properly parsed!");
                    bytesLeft = reader.ReadEpbRawMatrix(epb, bytesLeft, (b, e, s) =>
                    {
                        // TODO: Extract and apply the colour
                        return s + 1;
                    });
                }
                else
                {
                    bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                    {
                        EpbBlock block = epb.Blocks[x, y, z];
                        UInt32 bits = r.ReadUInt32();
                        for (int i = 0; i < 6; i++)
                        {
                            block.Colours[i] = (EpbColour)(bits & 0x1f);
                            bits = bits >> 5;
                        }

                        colourCount++;
                        Console.WriteLine($"    {colourCount,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.Colours)}");
                        return b - 4;
                    });
                }
            }
            #endregion Colours

            #region Textures
            if (version > 4)
            {
                int textureCount = 0;
                Console.WriteLine("Texture matrix");
                if (version <= 12)
                {
                    Console.WriteLine("TODO: Read but not properly parsed!");
                    bytesLeft = reader.ReadEpbRawMatrix(epb, bytesLeft, (b, e, s) =>
                    {
                        // TODO: Extract and apply the texture
                        return s + 1;
                    });
                }
                else
                {
                    bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                    {
                        EpbBlock block = epb.Blocks[x, y, z];
                        UInt64 bits = r.ReadUInt64();
                        for (int i = 0; i < 6; i++)
                        {
                            block.Textures[i] = (byte) (bits & 0x3f);
                            bits = bits >> 6;
                        }

                        textureCount++;
                        Console.WriteLine(
                            $"    {textureCount,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.Textures)}");
                        return b - 8;
                    });
                }
            }
            #endregion Textures

            #region TextureFlips
            if (version >= 20)
            {
                int textureFlipCount = 0;
                Console.WriteLine("TextureFlip matrix");
                bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                {
                    EpbBlock block = epb.Blocks[x, y, z];
                    byte bits = r.ReadByte();
                    for (int i = 0; i < 6; i++)
                    {
                        block.TextureFlips[i] = (bits & 0x01) != 0;
                        bits = (byte)(bits >> 1);
                    }

                    textureFlipCount++;
                    Console.WriteLine($"    {textureFlipCount,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.TextureFlips)}");
                    return b - 1;
                });
            }
            #endregion TextureFlips

            #region Symbols
            if (version > 4) // TODO: Verify version, maybe this is newer than this
            {
                int symbolCount = 0;
                Console.WriteLine("Symbol matrix");
                if (version <= 12)
                {
                    Console.WriteLine("TODO: Read but not properly parsed!");
                    bytesLeft = reader.ReadEpbRawMatrix(epb, bytesLeft, (b, e, s) =>
                    {
                        // TODO: Extract and apply the symbol
                        return s + 1;
                    });
                }
                else
                {
                    bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                    {
                        EpbBlock block = epb.Blocks[x, y, z];
                        UInt32 bits = r.ReadUInt32();
                        for (int i = 0; i < 6; i++)
                        {
                            block.Symbols[i] = (byte) (bits & 0x1f);
                            bits = bits >> 5;
                        }

                        block.SymbolPage = (byte) bits;
                        symbolCount++;
                        Console.WriteLine(
                            $"    {symbolCount,5} ({x,4}, {y,4}, {z,4}): Page={block.SymbolPage} {string.Join(", ", block.Symbols)}");
                        return b - 4;
                    });
                }
            }
            #endregion Symbols

            #region SymbolRotations
            if (version > 4) //version >= 20)
            {
                int symbolRotationCount = 0;
                Console.WriteLine("SymbolRotation matrix");
                if (version < 20)
                {
                    //TODO: "BAO_AntarisSpacefarm.epd" proves that this is NOT correct.
                    Console.WriteLine("TODO: Read but not properly parsed!");
                    bytesLeft = reader.ReadEpbRawMatrix(epb, bytesLeft, (b, e, s) =>
                    {
                        // TODO: Extract and apply the symbol rotation
                        return s + 1;
                    });
                }
                else
                {
                    bytesLeft = reader.ReadEpbMatrix(epb, length, (r, e, x, y, z, b) =>
                    {
                        EpbBlock block = epb.Blocks[x, y, z];
                        UInt32 bits = r.ReadUInt32();
                        for (int i = 0; i < 6; i++)
                        {
                            block.SymbolRotations[i] = (EpbBlock.SymbolRotation) (bits & 0x3);
                            bits = bits >> 2;
                        }

                        symbolRotationCount++;
                        Console.WriteLine(
                            $"    {symbolRotationCount,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.SymbolRotations)}");
                        return b - 4;
                    });
                }
            }
            #endregion SymbolRotations

            #region "Unknown06"
            if (version > 4) // TODO: Verify version, maybe this is newer than this
            {
                UInt16 nUnknown06 = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"Unknown06 ({nUnknown06})");
                for (int i = 0; i < nUnknown06; i++)
                {
                    byte[] unknown06a = reader.ReadBytes(5); //TODO: This is possibly a EpbBlockPos, hinting on a flexible length construct to accomodate for larger coordinates
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
            }
            #endregion "Unknown06"

            #region Unknown07
            if (version > 4) // TODO: Verify version, maybe this is newer than this
            {

                UInt16 unknown07Count = reader.ReadUInt16();
                bytesLeft -= 2;
                byte[] unknown07 = reader.ReadBytes(unknown07Count * 6);
                bytesLeft -= unknown07Count * 6;
                Console.WriteLine($"Unknown07: {unknown07.ToHexString()}");
            }
            #endregion Unknown07

            #region Signals
            if (version > 12) // TODO: Verify version, maybe this is newer than this
            {
                UInt16 signalCount = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"Signals ({signalCount})");
                for (int i = 0; i < signalCount; i++)
                {
                    byte signalUnknown01 = reader.ReadByte();
                    bytesLeft -= 1;
                    Console.WriteLine($"    SignalUnknown01: 0x{signalUnknown01:x2}");
                    UInt16 nTags = reader.ReadUInt16();
                    bytesLeft -= 2;
                    Console.WriteLine($"    BlockTags: {nTags}");
                    for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                    {
                        EpbBlockTag tag = reader.ReadEpbBlockTag(ref bytesLeft);
                        Console.WriteLine($"        {tagIndex}: {tag}");
                    }
                }
            }
            #endregion Signals

            #region Logic
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

                byte unknown09 = reader.ReadByte(); //In "/PrefabsStock/BA_Prefab_Tier5a/BA_Prefab_Tier5a.epb", this does not exist.
                bytesLeft -= 1;
                Console.WriteLine($"Unknown09: 0x{unknown09:x2}");

                UInt16 nCustom = reader.ReadUInt16();
                Console.WriteLine($"Custom ({nCustom})");
                for (int i = 0; i < nCustom; i++)
                {
                    string s = ReadEpString(reader, ref bytesLeft);
                    //UInt16 customUnknown01 = reader.ReadUInt16();
                    //Console.WriteLine($"    {i}: 0x{customUnknown01:x4} \"{s}\"");
                    Console.WriteLine($"    {i}: \"{s}\"");
                }
            }
            #endregion Logic

            byte[] remainingData = reader.ReadBytes((int)(bytesLeft));
            Console.WriteLine($"Remaining data:\n\r{remainingData.ToHexDump()}");
        }

        public static long ReadEpbRawMatrix(this BinaryReader reader, EpBlueprint epb, long bytesLeft, Func<byte[], EpBlueprint, long, long> func)
        {
            UInt32 matrixSize = reader.ReadUInt32();
            byte[] data = reader.ReadBytes((int)matrixSize);
            bytesLeft -= 4;
            if (func == null)
            {
                return bytesLeft;
            }

            long index = 0;
            while (index < matrixSize)
            {
                index = func(data, epb, index);
            }
            return bytesLeft;
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
                    if (name == "Pos")
                    {
                        EpbBlockTagPos tagPos = new EpbBlockTagPos();
                        tagPos.Value = reader.ReadEpbBlockPos(ref bytesLeft);
                        tag = tagPos;
                    }
                    else
                    {
                        EpbBlockTagUInt32 tagUInt32 = new EpbBlockTagUInt32(name);
                        tagUInt32.Value = reader.ReadUInt32();
                        bytesLeft -= 4;
                        tag = tagUInt32;
                    }
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
                case EpMetaTagType.UInt16:
                    EpMetaTagUInt16 tagUInt16 = new EpMetaTagUInt16(key);
                    tagUInt16.Value = reader.ReadUInt16();
                    bytesLeft -= 2;
                    tag = tagUInt16;
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

        public static List<EpbDeviceGroup> ReadEpbDeviceGroups(this BinaryReader reader, UInt32 version, ref long bytesLeft)
        {
            List<EpbDeviceGroup> groups = new List<EpbDeviceGroup>();
            UInt16 nGroups = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"DeviceGroups ({nGroups}):");
            for (int i = 0; i < nGroups; i++)
            {
                groups.Add(reader.ReadEpbDeviceGroup(version, ref bytesLeft));
            }
            return groups;
        }
        public static EpbDeviceGroup ReadEpbDeviceGroup(this BinaryReader reader, UInt32 version, ref long bytesLeft)
        {
            EpbDeviceGroup group = new EpbDeviceGroup();
            group.Name = reader.ReadEpString(ref bytesLeft);
            Console.Write($"    {group.Name,-30} (");

            if (version >= 20) //TODO: Some v20 files omit this byte. I don't know how to filter that.
            {
                group.DeviceGroupUnknown03 = reader.ReadByte();
                bytesLeft -= 1;
                Console.Write($" u3=0x{group.DeviceGroupUnknown03:x2}");
            }

            group.DeviceGroupUnknown01 = reader.ReadByte();
            bytesLeft -= 1;
            Console.Write($" u1=0x{group.DeviceGroupUnknown01:x2}");

            if (version > 14)
            {
                group.Shortcut = reader.ReadByte();
                bytesLeft -= 1;
                Console.Write($" Shortcut=" + (group.Shortcut != 0xff ?  $"{group.Shortcut + 1,-4}" : "None"));
            }
            UInt16 nDevices = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($" n=0x{nDevices:x4})");

            for (int i = 0; i < nDevices; i++)
            {
                group.Entries.Add(reader.ReadEpbDeviceGroupEntry(ref bytesLeft));
            }
            return group;
        }
        public static EpbDeviceGroupEntry ReadEpbDeviceGroupEntry(this BinaryReader reader, ref long bytesLeft)
        {
            EpbDeviceGroupEntry entry = new EpbDeviceGroupEntry();
            entry.Pos = reader.ReadEpbBlockPos(ref bytesLeft);
            Console.Write($"        Pos={entry.Pos}");

            entry.Name = reader.ReadEpString(ref bytesLeft);
            Console.WriteLine($" Name=\"{entry.Name}\"");

            return entry;
        }

        #endregion EpbDevices

        public static EpbBlockPos ReadEpbBlockPos(this BinaryReader reader, ref long bytesLeft)
        {
            UInt32 data = reader.ReadUInt32();
            bytesLeft -= 4;
            return new EpbBlockPos()
            {
                U1 = (byte)((data >> 28) & 0x0f),
                X  = (byte)((data >> 20) & 0xff),
                Y  = (byte)((data >> 12) & 0xff),
                U2 = (byte)((data >>  8) & 0x0f),
                Z  = (byte)((data >>  0) & 0xff)
            };
        }

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
