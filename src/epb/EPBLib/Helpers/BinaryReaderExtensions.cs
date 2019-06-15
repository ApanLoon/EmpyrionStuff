using EPBLib.BlockData;
using EPBLib.Logic;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;

namespace EPBLib.Helpers
{
    public static class BinaryReaderExtensions
    {
        #region Blueprint
        public static readonly UInt32 EpbIdentifier = 0x78945245;
        public static readonly byte[] ZipDataStartPattern = new byte[] { 0x00, 0x00, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00 };

        [Flags]
        public enum DataTypeFlags : ushort
        {
            Unknown0001 = 0x0001,
            Zipped      = 0x0100
        }

        public static Blueprint ReadBlueprint(this BinaryReader reader, ref long bytesLeft)
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

            BlueprintType type = (BlueprintType)reader.ReadByte();
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

            Blueprint epb = new Blueprint(type, width, height, depth);
            epb.Version = version;

            epb.Unknown01 = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Unknown01: {epb.Unknown01}");

            epb.MetaTags = reader.ReadMetaTagDictionary(ref bytesLeft);
            foreach (MetaTag tag in epb.MetaTags.Values)
            {
                Console.WriteLine(tag.ToString());
            }


            epb.Unknown02 = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Unknown02: 0x{epb.Unknown02:x4}");

            epb.LightCount = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"LightCount:     {epb.LightCount} (0x{epb.LightCount:x8})");
            epb.DoorCount = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"DoorCount:      {epb.DoorCount} (0x{epb.DoorCount:x8})");
            epb.DeviceCount = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"DeviceCount:    {epb.DeviceCount} (0x{epb.DeviceCount:x8})");
            epb.UnknownCount02 = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"UnknownCount02: {epb.UnknownCount02} (0x{epb.UnknownCount02:x8})");
            epb.SolidCount = reader.ReadUInt32();
            bytesLeft -= 4;
            Console.WriteLine($"SolidCount:     {epb.SolidCount} (0x{epb.SolidCount:x8})");

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
                BlockType blockType = reader.ReadBlockType(ref bytesLeft);
                if (version <= 12)
                {
                    reader.ReadUInt16(); // Block types were 32 bit in the early versions, but these bytes were probably always zero, so we simply ignore them.
                    bytesLeft -= 2;
                }

                UInt32 blockCount = reader.ReadUInt32();
                bytesLeft -= 4;
                Console.WriteLine($"    BlockType={blockType,-40} Count={blockCount}");
                epb.BlockCounts[blockType] = blockCount;

                nBlocksTotal += blockCount;
            }
            Console.WriteLine($"Total number of blocks: {nBlocksTotal}");

            if (version > 8)
            {
                epb.DeviceGroups = reader.ReadDeviceGroups(version, ref bytesLeft);
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
                            zipReader.ReadBlocks(epb, version, entry.Size);
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
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
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
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x000d:
                                Console.WriteLine("Unknown13 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x000e:
                                Console.WriteLine("Unknown14 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x000f:
                                Console.WriteLine("Unknown15 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0010:
                                Console.WriteLine("Unknown16 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0011:
                                Console.WriteLine("Unknown17 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0012:
                                Console.WriteLine("Unknown18 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0013:
                                Console.WriteLine("Unknown19 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0014:
                                Console.WriteLine("Unknown20 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
                                {
                                    Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): true");
                                    count++;
                                    return b;
                                });
                                break;

                            case 0x0015:
                                Console.WriteLine("Unknown21 Matrix");
                                count = 0;
                                matricesLength -= matrixReader.ReadMatrix(reader, epb, ref bytesLeft, (dataReader, e, x, y, z, b) =>
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
        #endregion Blueprint

        #region Blocks

        public static BlockType ReadBlockType(this BinaryReader reader, ref long bytesLeft)
        {
            UInt16 id = reader.ReadUInt16();
            bytesLeft -= 2;
            return BlockType.GetBlockType(id);
        }

        public static void ReadBlocks(this BinaryReader reader, Blueprint epb, UInt32 version, long bytesLeft)
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
        public static long ReadBlockTypes(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
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
                            Block block = new Block(new BlockPos((byte)x, (byte)y, (byte)z))
                            {
                                BlockType = BlockType.GetBlockType((UInt16)(data & 0x7ff)),
                                Rotation = (Block.BlockRotation)((data >> 11) & 0x1f),
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
                bytesLeft = reader.ReadMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt32 data = reader.ReadUInt32();
                    blockCount++;
                    Block block = new Block(new BlockPos(x, y, z))
                    {
                        BlockType = BlockType.GetBlockType((UInt16)(data & 0x7ff)),
                        Rotation = (Block.BlockRotation)((data >> 11) & 0x1f),
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
        public static long ReadDamageStates(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
        {
            if (version <= 10)
            {
                return bytesLeft;
            }

            int damageStateCount = 0;
            Console.WriteLine("Damage state matrix");
            bytesLeft = reader.ReadMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
            {
                UInt16 damage = r.ReadUInt16();
                damageStateCount++;
                Block block = epb.Blocks[x, y, z];
                if (block != null)
                {
                    block.DamageState = damage;
                }
                Console.WriteLine(
                    $"    {damageStateCount,5} ({x,4}, {y,4}, {z,4}): {damage} (0x{damage:x4})");
                return b - 2;
            });
            return bytesLeft;
        }
        #endregion DamageStateMatrix

        #region Unknown02
        public static long ReadUnknown02(this BinaryReader reader, Blueprint epb, long bytesLeft)
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
        public static long ReadColourMatrix(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
        {
            if (version <= 4)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("Colour matrix");
            if (version >= 8)
            {
                bytesLeft = reader.ReadMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt32 bits = r.ReadUInt32();
                    count++;
                    Block block = epb.Blocks[x, y, z];
                    if (block == null)
                    {
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): WARNING: No block");
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            block.Colours[i] = (ColourIndex)(bits & 0x1f);
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
                bytesLeft = reader.ReadRawMatrix(epb, bytesLeft, (b, e, s) =>
                {
                    // TODO: Extract and apply the colour
                    return s + 1;
                });
            }
            return bytesLeft;
        }
        #endregion ColourMatrix

        #region TextureMatrix
        public static long ReadTextureMatrix(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
        {
            if (version <= 4)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("Texture matrix");
            if (version >= 8)
            {
                bytesLeft = reader.ReadMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt64 bits = r.ReadUInt64();
                    count++;
                    Block block = epb.Blocks[x, y, z];
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
                bytesLeft = reader.ReadRawMatrix(epb, bytesLeft, (b, e, s) =>
                {
                    // TODO: Extract and apply the texture
                    return s + 1;
                });
            }
            return bytesLeft;
        }
        #endregion TextureMatrix

        #region TextureFlipMatrix
        public static long ReadTextureFlipMatrix(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
        {
            if (version < 20)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("TextureFlip matrix");
            bytesLeft = reader.ReadMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
            {
                byte bits = r.ReadByte();
                count++;
                Block block = epb.Blocks[x, y, z];
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
        public static long ReadSymbolMatrix(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
        {
            if (version < 8)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("Symbol matrix");
            bytesLeft = reader.ReadMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
            {
                UInt32 bits = r.ReadUInt32();
                count++;
                Block block = epb.Blocks[x, y, z];
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
        public static long ReadSymbolRotationMatrix(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
        {
            if (version < 8)
            {
                return bytesLeft;
            }

            int count = 0;
            Console.WriteLine("SymbolRotation matrix");
            if (version >= 20)
            {
                bytesLeft = reader.ReadMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt32 bits = r.ReadUInt32();
                    count++;
                    Block block = epb.Blocks[x, y, z];
                    if (block == null)
                    {
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): WARNING: No block");
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            block.SymbolRotations[i] = (Block.SymbolRotation) (bits & 0x3);
                            bits = bits >> 2;
                        }
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): {string.Join(", ", block.SymbolRotations)}");
                    }
                    return b - 4;
                });
            }
            else if (version >= 8)
            {
                bytesLeft = reader.ReadMatrix(epb, bytesLeft, (r, e, x, y, z, b) =>
                {
                    UInt32 bits = r.ReadUInt32();
                    count++;
                    Block block = epb.Blocks[x, y, z];
                    if (block == null)
                    {
                        Console.WriteLine($"    {count,5} ({x,4}, {y,4}, {z,4}): WARNING: No block");
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            block.SymbolRotations[i] = (Block.SymbolRotation) (bits & 0x1f);
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
                bytesLeft = reader.ReadRawMatrix(epb, bytesLeft, (b, e, s) =>
                {
                    // TODO: Extract and apply the symbol rotation
                    return s + 1;
                });
            }

            return bytesLeft;
        }
        #endregion SymbolRotationMatrix

        #region BlockTags
        public static long ReadBlockTags(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
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
                BlockPos pos = reader.ReadBlockPos(ref bytesLeft);
                byte unknown06 = reader.ReadByte();
                bytesLeft -= 1;
                UInt16 nTags = reader.ReadUInt16();
                bytesLeft -= 2;

                Console.WriteLine($"{pos}, Unknown06: {unknown06:x2}, Count: {nTags}");

                Block block = epb.Blocks[pos.X, pos.Y, pos.Z];

                for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                {
                    BlockTag tag = reader.ReadBlockTag(ref bytesLeft);
                    Console.WriteLine($"        {tagIndex}: {tag}");

                    block?.AddTag(tag);
                }
            }
            return bytesLeft;
        }
        #endregion BlockTags

        #region Unknown07
        public static long ReadUnknown07(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
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
        public static long ReadSignalSources(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
        {
            if (version > 14)
            {
                UInt16 signalCount = reader.ReadUInt16();
                bytesLeft -= 2;
                Console.WriteLine($"SignalSources ({signalCount})");
                for (int i = 0; i < signalCount; i++)
                {
                    SignalSource source = new SignalSource();
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
                        BlockTag tag = reader.ReadBlockTag(ref bytesLeft);
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
                    SignalSource source = new SignalSource();
                    epb.SignalSources.Add(source);

                    source.Unknown01 = 0;

                    BlockPos pos = reader.ReadBlockPos(ref bytesLeft);
                    source.Tags.Add("Pos", new BlockTagPos(pos));
                    Console.WriteLine($"    Pos: {pos}");

                    string name = reader.ReadEpString(ref bytesLeft);
                    source.Tags.Add("Name", new BlockTagString(name));
                    Console.WriteLine($"    Name: {name}");
                }
            }
            return bytesLeft;
        }
        #endregion SignalSources

        #region SignalTargets
        public static long ReadSignalTargets(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
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
                    SignalTarget target = new SignalTarget();
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
                        BlockTag tag = reader.ReadBlockTag(ref bytesLeft);
                        target.Tags.Add(tag.Name, tag);
                        Console.WriteLine($"            {t}: {tag}");
                    }
                }
            }
            return bytesLeft;
        }
        #endregion SignalTargets

        #region SignalOperators
        public static long ReadSignalOperators(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
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
                List<BlockTag> tags = new List<BlockTag>();
                for (int tagIndex = 0; tagIndex < nTags; tagIndex++)
                {
                    BlockTag tag = reader.ReadBlockTag(ref bytesLeft);
                    tags.Add(tag);
                    if (tag.Name == "OpName" && tag.BlockTagType == BlockTag.TagType.String)
                    {
                        opName = ((BlockTagString) tag).Value;
                    }
                    Console.WriteLine($"        {tagIndex}: {tag}");
                }


                SignalOperator signalOperator;
                switch (opName)
                {
                    case "titleCircuit2xAnd":
                        signalOperator = new SignalOperatorAnd2();
                        break;

                    case "titleCircuit4xAnd":
                        signalOperator = new SignalOperatorAnd4();
                        break;

                    case "titleCircuit2xNand":
                        signalOperator = new SignalOperatorNand2();
                        break;

                    case "titleCircuit4xNand":
                        signalOperator = new SignalOperatorNand4();
                        break;

                    case "titleCircuit2xOr":
                        signalOperator = new SignalOperatorOr2();
                        break;

                    case "titleCircuit4xOr":
                        signalOperator = new SignalOperatorOr4();
                        break;

                    case "titleCircuit2xNor":
                        signalOperator = new SignalOperatorNor2();
                        break;

                    case "titleCircuit4xNor":
                        signalOperator = new SignalOperatorNor4();
                        break;

                    case "titleCircuitXor":
                        signalOperator = new SignalOperatorXor();
                        break;

                    case "titleCircuitXnor":
                        signalOperator = new SignalOperatorXnor();
                        break;

                    case "titleCircuitInverter":
                        signalOperator = new SignalOperatorInverter();
                        break;

                    case "titleCircuitSRLatch":
                        signalOperator = new SignalOperatorSrLatch();
                        break;

                    case "titleCircuitDelay":
                        signalOperator = new SignalOperatorDelay();
                        break;

                    default:
                        signalOperator = new SignalOperator();
                        break;
                }
                epb.SignalOperators.Add(signalOperator);
                signalOperator.Unknown01 = signalOperatorUnknown01;

                foreach (BlockTag tag in tags)
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
        public static long ReadCustomNames(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
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
        public static long ReadUnknown08(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
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
        public static long ReadCustomPalettes(this BinaryReader reader, Blueprint epb, uint version, long bytesLeft)
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
                Palette palette = new Palette(nCustomColours);
                palette[0] = new Colour(255, 255, 255); // Custom palettes do not contain the "default" unpainted colour.
                for (int i = 1; i < nCustomColours; i++)
                {
                    byte r = reader.ReadByte();
                    bytesLeft -= 1;
                    byte g = reader.ReadByte();
                    bytesLeft -= 1;
                    byte b = reader.ReadByte();
                    bytesLeft -= 1;
                    Console.WriteLine($"        {i}: #{r:X2}{g:X2}{b:X2}");
                    palette[i] = new Colour(r, g, b);
                }

                if (p == 0) // TODO: Store all palettes, not just the first.
                {
                    epb.Palette = palette;
                }
            }
            return bytesLeft;
        }
        #endregion CustomPalettes

        public static long ReadRawMatrix(this BinaryReader reader, Blueprint epb, long bytesLeft, Func<byte[], Blueprint, long, long> func)
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

        public static long ReadMatrix(this BinaryReader reader, Blueprint epb, long bytesLeft, Func<BinaryReader, Blueprint, byte, byte, byte, long, long> func)
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

        public static long ReadMatrix(this BinaryReader matrixReader, BinaryReader dataReader, Blueprint epb, ref long dataBytesLeft, Func<BinaryReader, Blueprint, UInt32, UInt32, UInt32, long, long> func)
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

        public static BlockTag ReadBlockTag(this BinaryReader reader, ref long bytesLeft)
        {
            BlockTag.TagType type = (BlockTag.TagType)reader.ReadByte();
            bytesLeft -= 1;
            string name = reader.ReadEpString(ref bytesLeft);

            BlockTag tag;
            switch (type)
            {
                case BlockTag.TagType.UInt32:
                    if (name == "Pos")
                    {
                        BlockTagPos tagPos = new BlockTagPos();
                        tagPos.Value = reader.ReadBlockPos(ref bytesLeft);
                        tag = tagPos;
                    }
                    else
                    {
                        BlockTagUInt32 tagUInt32 = new BlockTagUInt32(name);
                        tagUInt32.Value = reader.ReadUInt32();
                        bytesLeft -= 4;
                        tag = tagUInt32;
                    }
                    break;
                case BlockTag.TagType.String:
                    BlockTagString tagString = new BlockTagString(name);
                    tagString.Value = reader.ReadEpString(ref bytesLeft);
                    tag = tagString;
                    break;
                case BlockTag.TagType.Bool:
                    BlockTagBool tagBool = new BlockTagBool(name);
                    tagBool.Value = reader.ReadByte() != 0;
                    bytesLeft -= 1;
                    tag = tagBool;
                    break;
                case BlockTag.TagType.Colour:
                    BlockTagColour tagColour = new BlockTagColour(name);
                    tagColour.Value = reader.ReadUInt32();
                    bytesLeft -= 4;
                    tag = tagColour;
                    break;
                case BlockTag.TagType.Float:
                    BlockTagFloat tagFloat = new BlockTagFloat(name);
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
        #endregion Blocks

        #region MetaTags

        public static Dictionary<MetaTagKey, MetaTag> ReadMetaTagDictionary(this BinaryReader reader, ref long bytesLeft)
        {
            Dictionary<MetaTagKey, MetaTag> dictionary = new Dictionary<MetaTagKey, MetaTag>();
            UInt16 nTags = reader.ReadUInt16();
            bytesLeft -= 2;
            for (int i = 0; i < nTags; i++)
            {
                MetaTag tag = reader.ReadMetaTag(ref bytesLeft);
                dictionary[tag.Key] = tag;
            }
            return dictionary;
        }


        public static MetaTag ReadMetaTag(this BinaryReader reader, ref long bytesLeft)
        {
            MetaTagKey key   = (MetaTagKey)reader.ReadInt32();
            MetaTagType type = (MetaTagType)reader.ReadInt32();
            bytesLeft -= 8;

            MetaTag tag;
            switch (type)
            {
                case MetaTagType.String:
                    MetaTagString tagString = new MetaTagString(key);
                    tagString.Value = reader.ReadEpString(ref bytesLeft);
                    tag = tagString;
                    break;
                case MetaTagType.UInt16:
                    MetaTagUInt16 tagUInt16 = new MetaTagUInt16(key);
                    tagUInt16.Value = reader.ReadUInt16();
                    bytesLeft -= 2;
                    tag = tagUInt16;
                    break;
                case MetaTagType.Unknownx02:
                    MetaTag02 tag02 = new MetaTag02(key);
                    tag02.Value = reader.ReadUInt32();
                    tag02.Unknown = reader.ReadByte();
                    bytesLeft -= 5;
                    tag = tag02;
                    break;
                case MetaTagType.Unknownx03:
                    MetaTag03 tag03 = new MetaTag03(key);
                    tag03.Value = reader.ReadBytes(5);
                    bytesLeft -= 5;
                    tag = tag03;
                    break;
                case MetaTagType.Unknownx04:
                    MetaTag04 tag04 = new MetaTag04(key);
                    tag04.Value = reader.ReadBytes(13);
                    bytesLeft -= 13;
                    tag = tag04;
                    break;
                case MetaTagType.Unknownx05:
                    MetaTag05 tag05 = new MetaTag05(key);
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
        #endregion MetaTags

        #region Devices

        public static List<DeviceGroup> ReadDeviceGroups(this BinaryReader reader, UInt32 version, ref long bytesLeft)
        {
            List<DeviceGroup> groups = new List<DeviceGroup>();

            byte deviceGroupVersion = reader.ReadByte();
            bytesLeft -= 1;

            UInt16 nGroups = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"DeviceGroups (Version: {deviceGroupVersion}, Count: {nGroups}):");
            for (int i = 0; i < nGroups; i++)
            {
                groups.Add(reader.ReadDeviceGroup(version, deviceGroupVersion, ref bytesLeft));
            }
            return groups;
        }
        public static DeviceGroup ReadDeviceGroup(this BinaryReader reader, UInt32 version, byte deviceGroupVersion, ref long bytesLeft)
        {
            DeviceGroup group = new DeviceGroup();
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
                group.Entries.Add(reader.ReadDeviceGroupEntry(version, ref bytesLeft));
            }
            return group;
        }
        public static DeviceGroupEntry ReadDeviceGroupEntry(this BinaryReader reader, UInt32 version, ref long bytesLeft)
        {
            DeviceGroupEntry entry = new DeviceGroupEntry();
            if (version >= 13)
            {
                entry.Pos = reader.ReadBlockPos(ref bytesLeft);
                entry.Name = reader.ReadEpString(ref bytesLeft);
            }
            else
            {
                byte x = (byte)reader.ReadUInt32();
                byte y = (byte)reader.ReadUInt32();
                byte z = (byte)reader.ReadUInt32();
                bytesLeft -= 12;
                entry.Pos = new BlockPos(x, y, z, 8, 8);
                entry.Name = "";
            }
            Console.WriteLine($"        Pos={entry.Pos} Name=\"{entry.Name}\"");
            return entry;
        }

        #endregion Devices

        public static BlockPos ReadBlockPos(this BinaryReader reader, ref long bytesLeft)
        {
            UInt32 data = reader.ReadUInt32();
            bytesLeft -= 4;
            return new BlockPos(
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
