using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using EPBLib.BlockData;
using EPBLib.Logic;
using ICSharpCode.SharpZipLib.Zip;

namespace EPBLib.Helpers
{
    public static class BinaryReaderExtensions
    {
        #region EpBlueprint
        public static readonly UInt32 EpbIdentifier = 0x78945245;
        public static readonly byte[] ZipDataStartPattern = new byte[] { 0x00, 0x00, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00 };

        [Flags]
        public enum DataTypeFlags : ushort
        {
            Unknown0001 = 0x0001,
            Zipped      = 0x0100
        }

        public static EpBlueprint ReadEpBlueprint(this BinaryReader reader, ref long bytesLeft)
        {

            UInt32 identifier = reader.ReadUInt32();
            bytesLeft -= 4;
            if (identifier != EpbIdentifier)
            {
                throw new Exception($"Unknown file identifier. 0x{identifier:x4}");
            }
            UInt32 version = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"Version:  {version}");

            EpBlueprint.EpbType type = (EpBlueprint.EpbType)reader.ReadByte();
            bytesLeft -= 1;
            Console.WriteLine($"Type:     {type}");

            UInt32 width = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"Width:    {width}");

            UInt32 height = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"Height:   {height}");

            UInt32 depth = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"Depth:    {depth}");

            EpBlueprint epb = new EpBlueprint(type, width, height, depth);

            epb.Unknown01 = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Unknown01: {epb.Unknown01}");

            epb.MetaTags = reader.ReadEpMetaTagDictionary(ref bytesLeft);
            foreach (EpMetaTag tag in epb.MetaTags.Values)
            {
                Console.WriteLine(tag.ToString());
            }


            epb.Unknown02 = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Unknown02: 0x{epb.Unknown02:x4}");

            epb.LightCount = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"LightCount:     {epb.LightCount} (0x{epb.LightCount:x8})");
            epb.UnknownCount01 = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"UnknownCount01: {epb.UnknownCount01} (0x{epb.UnknownCount01:x8})");
            epb.DeviceCount = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"DeviceCount:    {epb.DeviceCount} (0x{epb.DeviceCount:x8})");
            epb.UnknownCount02 = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"UnknownCount02: {epb.UnknownCount02} (0x{epb.UnknownCount02:x8})");
            UInt32 nBlocks = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"nBlocks:        {nBlocks} (0x{nBlocks:x8})");

            if (version >= 13)
            {
                epb.UnknownCount03 = reader.ReadUInt32();
                bytesLeft -= 4;
                Console.WriteLine($"UnknownCount03: {epb.UnknownCount03} (0x{epb.UnknownCount03:x8})");
            }
            if (version >= 18)
            {
                epb.TriangleCount = reader.ReadUInt32();
                bytesLeft -= 4;
                Console.WriteLine($"TriangleCount:  {epb.TriangleCount} (0x{epb.TriangleCount:x8})");
            }

            UInt16 nBlockCounts = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"BlockCounts (0x{nBlockCounts:x4})");
            UInt32 nBlocksTotal = 0;
            for (int i = 0; i < nBlockCounts; i++)
            {
                EpbBlock.EpbBlockType blockType = reader.ReadEpbBlockType(ref bytesLeft);
                if (version <= 12)
                {
                    reader.ReadUInt16(); // Block types were 32 bit in the early versions, but these bytes were probably always zero, so we simply ignore them.
                    bytesLeft -= 2;
                }

                UInt32 blockCount = reader.ReadUInt32();
                bytesLeft -= 4;
                Console.WriteLine($"    BlockType={blockType,-40} Count={blockCount}");

                nBlocksTotal += blockCount;
            }
            Console.WriteLine($"Total number of blocks: {nBlocksTotal}");

            if (version > 8)
            {
                epb.DeviceGroups = reader.ReadEpbDeviceGroups(version, ref bytesLeft);
            }

            UInt32 dataLength = (UInt32)bytesLeft;              //Prior to v23, there was nothing after the zipped data
            DataTypeFlags dataTypeFlags = DataTypeFlags.Zipped; //Prior to v23 all blockdata was zipped
            if (version >= 23)
            {
                dataLength = reader.ReadUInt32();
                bytesLeft -= 4;
                Console.WriteLine($"Data length: {dataLength}(0x{dataLength:x8})");
                dataTypeFlags = (DataTypeFlags)reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"Data type flags: {dataTypeFlags}");
            }
            if ((dataTypeFlags & DataTypeFlags.Zipped) != 0)
            {
                byte[] zippedData = reader.ReadBytes((int)dataLength);
                bytesLeft -= dataLength;

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
                //Console.WriteLine($"{zippedData.ToHexDump()}");
            }

            if (version >= 23)
            {
                long matricesLength = reader.ReadUInt32();
                bytesLeft -= 4;
                Console.WriteLine($"matricesLength: {matricesLength}(0x{matricesLength:x8})");

                UInt16 matrixUnknown01 = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"matrixUnknown01: {matrixUnknown01}");

                if (matricesLength != 0)
                {
                    byte[] matrices = reader.ReadBytes((int)matricesLength);
                    bytesLeft -= matricesLength;

                    BinaryReader matrixReader = new BinaryReader(new MemoryStream(matrices));

                    bool done = false;
                    while (!done && matricesLength > 0)
                    {
                        UInt16 matrixType = matrixReader.ReadUInt16();
                        matricesLength -= 2;
                        Console.WriteLine($"MatrixType: 0x{matrixType:x4}");

                        int count;
                        switch (matrixType) //TODO: Should any of these matrices read actual data?
                        {
                            case 0x0000:
                                Console.WriteLine("Filler Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    // No data, just true/false
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x000c:
                                Console.WriteLine("Unknown12 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x000d:
                                Console.WriteLine("Unknown13 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x000e:
                                Console.WriteLine("Unknown14 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x000f:
                                Console.WriteLine("Unknown15 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0010:
                                Console.WriteLine("Unknown16 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0011:
                                Console.WriteLine("Unknown17 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0012:
                                Console.WriteLine("Unknown18 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0013:
                                Console.WriteLine("Unknown19 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0014:
                                Console.WriteLine("Unknown20 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0015:
                                Console.WriteLine("Unknown21 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadEpbMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            default:
                                Console.WriteLine($"Unknown matrix type: 0x{matrixType:x4}");
                                done = true;
                                break;
                        }
                    }
                }
            }

            int n = Math.Min(0x100, (int)bytesLeft);
            Console.WriteLine($"Remaining data: {bytesLeft} (0x{bytesLeft:x8})");
            byte[] remainingData = reader.ReadBytes(n);
            Console.WriteLine(remainingData.ToHexDump());
            if (bytesLeft != n)
            {
                Console.WriteLine("...");
            }
            return epb;
        }
        #endregion EpBlueprint

        #region EpbBlocks

        public static EpbBlock.EpbBlockType ReadEpbBlockType(this BinaryReader reader, ref long bytesLeft)
        {
            UInt16 id = reader.ReadUInt16();
            bytesLeft -= 2;
            return EpbBlock.GetBlockType(id);
        }

        public static void ReadEpbBlocks(this BinaryReader reader, EpBlueprint epb, UInt32 version, long bytesLeft)
        {
            bytesLeft = reader.ReadBlockTypes(epb, version, bytesLeft);
            bytesLeft = reader.ReadDamageStates(epb, version, bytesLeft);
            bytesLeft = reader.ReadUnknown02(epb, bytesLeft);
            bytesLeft = reader.ReadColourMatrix(epb, version, bytesLeft);
            bytesLeft = reader.ReadTextureMatrix(epb, version, bytesLeft);
            bytesLeft = reader.ReadTextureFlipMatrix(epb, version, bytesLeft);
            bytesLeft = reader.ReadSymbolMatrix(epb, version, bytesLeft);
            bytesLeft = reader.ReadSymbolRotationMatrix(epb, version, bytesLeft);
            bytesLeft = reader.ReadBlockTags(epb, version, bytesLeft);
            bytesLeft = reader.ReadUnknown07(epb, version, bytesLeft);
            bytesLeft = reader.ReadSignalSources(epb, version, bytesLeft);
            bytesLeft = reader.ReadSignalTargets(epb, version, bytesLeft);
            bytesLeft = reader.ReadSignalOperators(epb, version, bytesLeft);
            bytesLeft = reader.ReadCustomNames(epb, version, bytesLeft);
            bytesLeft = reader.ReadUnknown08(epb, version, bytesLeft);
            bytesLeft = reader.ReadCustomPalettes(epb, version, bytesLeft);

            Console.WriteLine($"Remaining block data: {bytesLeft}(0x{bytesLeft:x8})");
            byte[] remainingData = reader.ReadBytes((int)(bytesLeft));
            Console.WriteLine(remainingData.ToHexDump());
        }

        #region BlockTypeMatrix
        public static long ReadBlockTypes(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
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
                            EpbBlock block = new EpbBlock(new EpbBlockPos((byte)x, (byte)y, (byte)z))
                            {
                                BlockType = EpbBlock.GetBlockType((UInt16)(data & 0x7ff)),
                                Rotation = (EpbBlock.EpbBlockRotation)((data >> 11) & 0x1f),
                                Unknown00 = (UInt16)((data >> 16) & 0x3ff),
                                Variant = (byte)((data >> 25) & 0x1f)
                            };
                            if (block.BlockType.Id != 0)
                            {
                                blockCount++;
                                epb.SetBlock(block);
                                Console.WriteLine($"    {blockCount,5} ({x,4}, {y,4}, {z,4}): Rot={block.Rotation} Unknown2=0x{block.Unknown00:x3} Type={block.BlockType,-31} Variant=\"{block.VariantName + "\" (0x" + block.Variant.ToString("x2") + "=" + block.Variant + ")",-31}");
                                //Console.WriteLine($"{BitConverter.GetBytes(data).ToHexString()} | 0x{data:x08} {Convert.ToString(data, 2).PadLeft(32, '0')} |");
                            }
                        }
                    }
                }
            }
            else
            {
                bytesLeft = reader.ReadEpbMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt32 data = reader.ReadUInt32();
                    blockCount++;
                    EpbBlock block = new EpbBlock(new EpbBlockPos(x, y, z))
                    {
                        BlockType = EpbBlock.GetBlockType((UInt16)(data & 0x7ff)),
                        Rotation = (EpbBlock.EpbBlockRotation)((data >> 11) & 0x1f),
                        Unknown00 = (UInt16)((data >> 16) & 0x3ff),
                        Variant = (byte)((data >> 25) & 0x1f)
                    };
                    epb.SetBlock(block);
                    Console.WriteLine($"    {blockCount,5} ({x,4}, {y,4}, {z,4}): Rot={block.Rotation} Unknown2=0x{block.Unknown00:x3} Type={block.BlockType,-31} Variant=\"{block.VariantName + "\" (0x" + block.Variant.ToString("x2") + "=" + block.Variant + ")",-31}");
                    //Console.WriteLine( $"{BitConverter.GetBytes(data).ToHexString()} | 0x{data:x08} {Convert.ToString(data, 2).PadLeft(32, '0')} |");
                    return b - 4;
                });
            }

            return bytesLeft;
        }
        #endregion BlockTypeMatrix

        #region DamageStateMatrix
        public static long ReadDamageStates(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version <= 10)
            {
                return bytesLeft;
            }

            int damageStateCount = 0;
            Console.WriteLine("Damage state matrix");
            bytesLeft = reader.ReadEpbMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
            {
                UInt16 damage = r.ReadUInt16();
                damageStateCount++;
                Console.WriteLine(
                    $"    {damageStateCount,5} ({x,4}, {y,4}, {z,4}): {damage} (0x{damage:x4})");
                return b - 2;
            });
            return bytesLeft;
        }
        #endregion DamageStateMatrix

        #region Unknown02
        public static long ReadUnknown02(this BinaryReader reader, EpBlueprint epb, long bytesLeft)
        {
            int unknown02Count = reader.ReadByte();
            bytesLeft -= 1;
            if (unknown02Count == 0)
            {
                unknown02Count = (int)(epb.Width * epb.Height * epb.Depth); // blockCount;
            }
            byte[] unknown02 = reader.ReadBytes(unknown02Count);
            bytesLeft -= unknown02Count;
            Console.WriteLine($"unknown02: 0x{unknown02.ToHexString()}");
            return bytesLeft;
        }
        #endregion Unknown02

        #region ColourMatrix
        public static long ReadColourMatrix(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version <= 4)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("Colour matrix");
            if (version >= 8)
            {
                bytesLeft = reader.ReadEpbMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt32 bits = r.ReadUInt32();
                    count++;
                    EpbBlock block = epb.Blocks[x, y, z];
                    if (block == null)
                    {
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): WARNING: No block");
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            block.Colours[i] = (EpbColourIndex)(bits & 0x1f);
                            bits = bits >> 5;
                        }
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.Colours)}");
                    }
                    return b - 4;
                });
            }
            else
            {
                Console.WriteLine("TODO: Read but not properly parsed!");
                bytesLeft = reader.ReadEpbRawMatrix(epb, bytesLeft, (b, e, s) =>
                {
                    // TODO: Extract and apply the colour
                    return s + 1;
                });
            }
            return bytesLeft;
        }
        #endregion ColourMatrix

        #region TextureMatrix
        public static long ReadTextureMatrix(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version <= 4)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("Texture matrix");
            if (version >= 8)
            {
                bytesLeft = reader.ReadEpbMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt64 bits = r.ReadUInt64();
                    count++;
                    EpbBlock block = epb.Blocks[x, y, z];
                    if (block == null)
                    {
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): WARNING: No block");
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            block.Textures[i] = (byte) (bits & 0x3f);
                            bits = bits >> 6;
                        }
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.Textures)}");
                    }

                    return b - 8;
                });
            }
            else
            {
                Console.WriteLine("TODO: Read but not properly parsed!");
                bytesLeft = reader.ReadEpbRawMatrix(epb, bytesLeft, (b, e, s) =>
                {
                    // TODO: Extract and apply the texture
                    return s + 1;
                });
            }
            return bytesLeft;
        }
        #endregion TextureMatrix

        #region TextureFlipMatrix
        public static long ReadTextureFlipMatrix(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version < 20)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("TextureFlip matrix");
            bytesLeft = reader.ReadEpbMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
            {
                byte bits = r.ReadByte();
                count++;
                EpbBlock block = epb.Blocks[x, y, z];
                if (block == null)
                {
                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): WARNING: No block");
                }
                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        block.TextureFlips[i] = (bits & 0x01) != 0;
                        bits = (byte) (bits >> 1);
                    }
                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.TextureFlips)}");
                }
                return b - 1;
            });

            return bytesLeft;
        }
        #endregion TextureFlipMatrix

        #region SymbolMatrix
        public static long ReadSymbolMatrix(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version < 8)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("Symbol matrix");
            bytesLeft = reader.ReadEpbMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
            {
                UInt32 bits = r.ReadUInt32();
                count++;
                EpbBlock block = epb.Blocks[x, y, z];
                if (block == null)
                {
                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): WARNING: No block");
                }
                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        block.Symbols[i] = (byte) (bits & 0x1f);
                        bits = bits >> 5;
                    }

                    block.SymbolPage = (byte) bits;
                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): Page={block.SymbolPage} {string.Join(", ", block.Symbols)}");
                }
                return b - 4;
            });
            return bytesLeft;
        }
        #endregion SymbolMatrix

        #region SymbolRotationMatrix
        public static long ReadSymbolRotationMatrix(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version < 8)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("SymbolRotation matrix");
            if (version >= 20)
            {
                bytesLeft = reader.ReadEpbMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt32 bits = r.ReadUInt32();
                    count++;
                    EpbBlock block = epb.Blocks[x, y, z];
                    if (block == null)
                    {
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): WARNING: No block");
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            block.SymbolRotations[i] = (EpbBlock.SymbolRotation) (bits & 0x3);
                            bits = bits >> 2;
                        }
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.SymbolRotations)}");
                    }
                    return b - 4;
                });
            }
            else if (version >= 8)
            {
                bytesLeft = reader.ReadEpbMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt32 bits = r.ReadUInt32();
                    count++;
                    EpbBlock block = epb.Blocks[x, y, z];
                    if (block == null)
                    {
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): WARNING: No block");
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            block.SymbolRotations[i] = (EpbBlock.SymbolRotation) (bits & 0x1f);
                            bits = bits >> 5;
                        }
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.SymbolRotations)}");
                    }
                    return b - 4;
                });
            }
            else
            {
                Console.WriteLine("TODO: Read but not properly parsed!");
                bytesLeft = reader.ReadEpbRawMatrix(epb, bytesLeft, (b, e, s) =>
                {
                    // TODO: Extract and apply the symbol rotation
                    return s + 1;
                });
            }

            return bytesLeft;
        }
        #endregion SymbolRotationMatrix

        #region BlockTags
        public static long ReadBlockTags(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version <= 10)
            {
                return bytesLeft;
            }

            UInt16 nBlockTags = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"BlockTags ({nBlockTags})");
            for (int i = 0; i < nBlockTags; i++)
            {
                EpbBlockPos pos = reader.ReadEpbBlockPos(ref bytesLeft);
                byte unknown06 = reader.ReadByte();
                bytesLeft -= 1;
                UInt16 nTags = reader.ReadUInt16();
                bytesLeft -= 2;

                Console.WriteLine($"{pos}, Unknown06: {unknown06:x2}, Count: {nTags}");

                EpbBlock block = epb.Blocks[pos.X, pos.Y, pos.Z];

                for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                {
                    EpbBlockTag tag = reader.ReadEpbBlockTag(ref bytesLeft);
                    Console.WriteLine($"        {tagIndex}: {tag}");

                    block?.AddTag(tag);
                }
            }
            return bytesLeft;
        }
        #endregion BlockTags

        #region Unknown07
        public static long ReadUnknown07(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version <= 10)
            {
                return bytesLeft;
            }

            UInt16 unknown07Count = reader.ReadUInt16();
            bytesLeft -= 2;
            epb.Unknown07 = reader.ReadBytes(unknown07Count * 6);
            bytesLeft -= unknown07Count * 6;
            Console.WriteLine($"Unknown07: {epb.Unknown07.ToHexString()}");
            return bytesLeft;
        }
        #endregion Unknown07

        #region SignalSources
        public static long ReadSignalSources(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version > 14)
            {
                UInt16 signalCount = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"SignalSources ({signalCount})");
                for (int i = 0; i < signalCount; i++)
                {
                    EpbSignalSource source = new EpbSignalSource();
                    epb.SignalSources.Add(source);

                    byte signalUnknown01 = reader.ReadByte();
                    bytesLeft -= 1;
                    source.Unknown01 = signalUnknown01;
                    Console.WriteLine($"    SignalUnknown01: 0x{signalUnknown01:x2}");

                    UInt16 nTags = reader.ReadUInt16();
                    bytesLeft -= 2;
                    Console.WriteLine($"    Tags:            {nTags}");
                    for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                    {
                        EpbBlockTag tag = reader.ReadEpbBlockTag(ref bytesLeft);
                        source.Tags.Add(tag.Name, tag);
                        Console.WriteLine($"        {tagIndex}: {tag}");
                    }
                }
            }
            else if (version > 13)
            {
                UInt16 signalCount = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"SignalSources ({signalCount})");
                for (int i = 0; i < signalCount; i++)
                {
                    EpbSignalSource source = new EpbSignalSource();
                    epb.SignalSources.Add(source);

                    source.Unknown01 = 0;

                    EpbBlockPos pos = reader.ReadEpbBlockPos(ref bytesLeft);
                    source.Tags.Add("Pos", new EpbBlockTagPos(pos));
                    Console.WriteLine($"    Pos: {pos}");

                    string name = reader.ReadEpString(ref bytesLeft);
                    source.Tags.Add("Name", new EpbBlockTagString(name));
                    Console.WriteLine($"    Name: {name}");
                }
            }
            return bytesLeft;
        }
        #endregion SignalSources

        #region SignalTargets
        public static long ReadSignalTargets(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            // Check CV_Prefab_Tier2.epb.hex for v18
            if (version <= 13)
            {
                return bytesLeft;
            }

            UInt16 signalCount = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"SignalTargets ({signalCount})");
            for (int i = 0; i < signalCount; i++)
            {
                string signalName = reader.ReadEpString(ref bytesLeft);
                Console.WriteLine($"    Signal:         {signalName}");

                UInt16 nTargets = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"    Targets:        {nTargets}");

                for (int targetIndex = 0; targetIndex < nTargets; targetIndex++)
                {
                    EpbSignalTarget target = new EpbSignalTarget();
                    epb.SignalTargets.Add(target);
                    target.SignalName = signalName;

                    byte targetUnknown01 = reader.ReadByte();
                    bytesLeft -= 1;
                    target.Unknown01 = targetUnknown01;
                    Console.WriteLine($"        TargetUnknown01: 0x{targetUnknown01:x2}");

                    UInt16 nTags = reader.ReadUInt16();
                    bytesLeft -= 2;
                    Console.WriteLine($"        Tags:          {nTags}");

                    for (int t = 0; t < nTags; t++)
                    {
                        EpbBlockTag tag = reader.ReadEpbBlockTag(ref bytesLeft);
                        target.Tags.Add(tag.Name, tag);
                        Console.WriteLine($"            {t}: {tag}");
                    }
                }
            }
            return bytesLeft;
        }
        #endregion SignalTargets

        #region SignalOperators
        public static long ReadSignalOperators(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version < 17)
            {
                return bytesLeft;
            }

            UInt16 signalOperatorCount = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"SignalOperators ({signalOperatorCount})");
            for (int i = 0; i < signalOperatorCount; i++)
            {

                byte signalOperatorUnknown01 = reader.ReadByte();
                bytesLeft -= 1;
                Console.WriteLine($"    SignalOperatorUnknown01: 0x{signalOperatorUnknown01:x2}");

                UInt16 nTags = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"    Tags:             {nTags}");

                string opName = "";
                List<EpbBlockTag> tags = new List<EpbBlockTag>();
                for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                {
                    EpbBlockTag tag = reader.ReadEpbBlockTag(ref bytesLeft);
                    tags.Add(tag);
                    if (tag.Name == "OpName" && tag.BlockTagType == EpbBlockTag.TagType.String)
                    {
                        opName = ((EpbBlockTagString) tag).Value;
                    }
                    Console.WriteLine($"        {tagIndex}: {tag}");
                }


                EpbSignalOperator signalOperator;
                switch (opName)
                {
                    case "titleCircuit2xAnd":
                        signalOperator = new EpbSignalOperatorAnd2();
                        break;

                    case "titleCircuit4xAnd":
                        signalOperator = new EpbSignalOperatorAnd4();
                        break;

                    case "titleCircuit2xNand":
                        signalOperator = new EpbSignalOperatorNand2();
                        break;

                    case "titleCircuit4xNand":
                        signalOperator = new EpbSignalOperatorNand4();
                        break;

                    case "titleCircuit2xOr":
                        signalOperator = new EpbSignalOperatorOr2();
                        break;

                    case "titleCircuit4xOr":
                        signalOperator = new EpbSignalOperatorOr4();
                        break;

                    case "titleCircuit2xNor":
                        signalOperator = new EpbSignalOperatorNor2();
                        break;

                    case "titleCircuit4xNor":
                        signalOperator = new EpbSignalOperatorNor4();
                        break;

                    case "titleCircuitXor":
                        signalOperator = new EpbSignalOperatorXor();
                        break;

                    case "titleCircuitXnor":
                        signalOperator = new EpbSignalOperatorXnor();
                        break;

                    case "titleCircuitInverter":
                        signalOperator = new EpbSignalOperatorInverter();
                        break;

                    case "titleCircuitSRLatch":
                        signalOperator = new EpbSignalOperatorSRLatch();
                        break;

                    case "titleCircuitDelay":
                        signalOperator = new EpbSignalOperatorDelay();
                        break;

                    default:
                        signalOperator = new EpbSignalOperator();
                        break;
                }
                epb.SignalOperators.Add(signalOperator);
                signalOperator.Unknown01 = signalOperatorUnknown01;

                foreach (EpbBlockTag tag in tags)
                {
                    signalOperator.Tags.Add(tag.Name, tag);
                }

            }
            return bytesLeft;
        }
        #endregion SignalOperators

        #region CustomNames
        /// <summary>
        /// Names of the custom buttons in the main control panel screen.
        /// </summary>
        public static long ReadCustomNames(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version < 15)
            {
                return bytesLeft;
            }

            UInt16 nCustom = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Custom names ({nCustom})");
            for (int i = 0; i < nCustom; i++)
            {
                string s = ReadEpString(reader, ref bytesLeft);
                Console.WriteLine($"    {i}: \"{s}\"");
                epb.CustomNames.Add(s);
            }
            return bytesLeft;
        }
        #endregion CustomNames

        #region Unknown08
        public static long ReadUnknown08(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version < 19)
            {
                return bytesLeft;
            }

            UInt16 nUnknown08 = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Unknown08 ({nUnknown08})");
            for (int i = 0; i < nUnknown08; i++)
            {
                string name = ReadEpString(reader, ref bytesLeft);
                byte[] unknown = reader.ReadBytes(8);
                bytesLeft -= 8;
                Console.WriteLine($"    {i}: {unknown.ToHexString()} \"{name}\"");
                epb.Unknown08[name] = unknown;
            }
            return bytesLeft;
        }
        #endregion Unknown08

        #region CustomPalettes
        public static long ReadCustomPalettes(this BinaryReader reader, EpBlueprint epb, uint version, long bytesLeft)
        {
            if (version < 21)
            {
                return bytesLeft;
            }

            byte nCustomPalettes = reader.ReadByte();
            bytesLeft -= 1;
            Console.WriteLine($"nCustomPalettes: ({nCustomPalettes})");

            for (int p = 0; p < nCustomPalettes; p++)
            {
                UInt32 nCustomColours = reader.ReadUInt32();
                bytesLeft -= 4;
                Console.WriteLine($"    Palette {p}: ({nCustomColours} - 1)");
                EpbPalette palette = new EpbPalette(nCustomColours);
                palette[0] = new EpbColour(255, 255, 255); // Custom palettes do not contain the "default" unpainted colour.
                for (int i = 1; i < nCustomColours; i++)
                {
                    byte r = reader.ReadByte();
                    bytesLeft -= 1;
                    byte g = reader.ReadByte();
                    bytesLeft -= 1;
                    byte b = reader.ReadByte();
                    bytesLeft -= 1;
                    Console.WriteLine($"        {i}: #{r:X2}{g:X2}{b:X2}");
                    palette[i] = new EpbColour(r, g, b);
                }

                if (p == 0) // TODO: Store all palettes, not just the first.
                {
                    epb.Palette = palette;
                }
            }
            return bytesLeft;
        }
        #endregion CustomPalettes

        public static long ReadEpbRawMatrix(this BinaryReader reader, EpBlueprint epb, long bytesLeft, Func<byte[], EpBlueprint, long, long> func)
        {
            UInt32 matrixSize = reader.ReadUInt32();
            bytesLeft -= 4;
            byte[] data = reader.ReadBytes((int)matrixSize);
            bytesLeft -= matrixSize;
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

        public static long ReadEpbMatrix(this BinaryReader reader, EpBlueprint epb, long bytesLeft, Func<BinaryReader, EpBlueprint, byte, byte, byte, long, long> func)
        {
            UInt32 matrixSize = reader.ReadUInt32();
            bytesLeft -= 4;
            byte[] matrix = reader.ReadBytes((int)matrixSize);
            bytesLeft -= matrixSize;
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
                        UInt32 index = (byte)z * epb.Width * epb.Height + (byte)y * epb.Width + (byte)x;
                        if (index < m.Length && m[index]) //TODO: Why do I have to test against length? v23/BA_FillerTest.epb
                        {
                            bytesLeft = func(reader, epb, (byte)x, (byte)y, (byte)z, bytesLeft);
                        }
                    }
                }
            }
            return bytesLeft;
        }

        public static long ReadEpbMatrix(this BinaryReader matrixReader, BinaryReader dataReader, EpBlueprint epb, ref long dataBytesLeft, Func<BinaryReader, EpBlueprint, UInt32, UInt32, UInt32, long, long> func)
        {
            long matrixBytesRead = 0;
            UInt32 matrixSize = matrixReader.ReadUInt32();
            matrixBytesRead += 4;
            byte[] matrix = matrixReader.ReadBytes((int)matrixSize);
            matrixBytesRead += matrixSize;
            if (func == null)
            {
                return matrixBytesRead;
            }

            bool[] m = matrix.ToBoolArray();
            for (UInt32 z = 0; z < epb.Depth; z++)
            {
                for (UInt32 y = 0; y < epb.Height; y++)
                {
                    for (UInt32 x = 0; x < epb.Width; x++)
                    {
                        uint index = z * epb.Width * epb.Height + y * epb.Width + x;
                        if (index < m.Length && m[index]) //TODO: Why do I have to test against length? v23/BA_FillerTest.epb
                        {
                            dataBytesLeft = func(dataReader, epb, x, y, z, dataBytesLeft);
                        }
                    }
                }
            }
            return matrixBytesRead;
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
                    tagBool.Value = reader.ReadByte() != 0;
                    bytesLeft -= 1;
                    tag = tagBool;
                    break;
                case EpbBlockTag.TagType.Colour:
                    EpbBlockTagColour tagColour = new EpbBlockTagColour(name);
                    tagColour.Value = reader.ReadUInt32();
                    bytesLeft -= 4;
                    tag = tagColour;
                    break;
                case EpbBlockTag.TagType.Float:
                    EpbBlockTagFloat tagFloat = new EpbBlockTagFloat(name);
                    tagFloat.Value = reader.ReadSingle();
                    bytesLeft -= 4;
                    tag = tagFloat;
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
                dictionary[tag.Key] = tag;
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
                    tag02.Value = reader.ReadUInt32();
                    tag02.Unknown = reader.ReadByte();
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
                    tag05.Value = DateTime.FromBinary(reader.ReadInt64());
                    tag05.Unknown = reader.ReadByte();
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

            byte deviceGroupVersion = reader.ReadByte();
            bytesLeft -= 1;

            UInt16 nGroups = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"DeviceGroups (Version: {deviceGroupVersion}, Count: {nGroups}):");
            for (int i = 0; i < nGroups; i++)
            {
                groups.Add(reader.ReadEpbDeviceGroup(version, deviceGroupVersion, ref bytesLeft));
            }
            return groups;
        }
        public static EpbDeviceGroup ReadEpbDeviceGroup(this BinaryReader reader, UInt32 version, byte deviceGroupVersion, ref long bytesLeft)
        {
            EpbDeviceGroup group = new EpbDeviceGroup();
            group.Name = reader.ReadEpString(ref bytesLeft);
            Console.Write($"    {group.Name,-30} (");

            if (deviceGroupVersion >= 5)
            {
                group.DeviceGroupUnknown03 = reader.ReadByte();
                bytesLeft -= 1;
                Console.Write($" u3=0x{group.DeviceGroupUnknown03:x2}");
            }

            group.DeviceGroupUnknown01 = reader.ReadByte();
            bytesLeft -= 1;
            Console.Write($" u1=0x{group.DeviceGroupUnknown01:x2}");

            if (deviceGroupVersion >= 4)
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
                group.Entries.Add(reader.ReadEpbDeviceGroupEntry(version, ref bytesLeft));
            }
            return group;
        }
        public static EpbDeviceGroupEntry ReadEpbDeviceGroupEntry(this BinaryReader reader, UInt32 version, ref long bytesLeft)
        {
            EpbDeviceGroupEntry entry = new EpbDeviceGroupEntry();
            if (version >= 13)
            {
                entry.Pos = reader.ReadEpbBlockPos(ref bytesLeft);
                entry.Name = reader.ReadEpString(ref bytesLeft);
            }
            else
            {
                byte x = (byte)reader.ReadUInt32();
                byte y = (byte)reader.ReadUInt32();
                byte z = (byte)reader.ReadUInt32();
                bytesLeft -= 12;
                entry.Pos = new EpbBlockPos(x, y, z, 8, 8);
                entry.Name = "";
            }
            Console.WriteLine($"        Pos={entry.Pos} Name=\"{entry.Name}\"");
            return entry;
        }

        #endregion EpbDevices

        public static EpbBlockPos ReadEpbBlockPos(this BinaryReader reader, ref long bytesLeft)
        {
            UInt32 data = reader.ReadUInt32();
            bytesLeft -= 4;
            return new EpbBlockPos(
                (byte) ((data >> 20) & 0xff),
                (byte) ((data >> 12) & 0xff),
                (byte) ((data >> 0) & 0xff),
                (byte) ((data >> 28) & 0x0f),
                (byte) ((data >> 8) & 0x0f));
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
