using EPBLib.Logic;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EPBLib.BlockData;
using static EPBLib.Helpers.BinaryReaderExtensions;

namespace EPBLib.Helpers
{
    public static class BinaryWriterExtensions
    {
        #region Blueprint
        public static readonly UInt32 EpbIdentifier = 0x78945245;
        public static UInt32 EpbVersion = 25;

        public static void Write(this BinaryWriter writer, Blueprint epb)
        {
            writer.Write(EpbIdentifier);
            writer.Write(EpbVersion);
            writer.Write((byte)epb.Type);
            writer.Write(epb.Width);
            writer.Write(epb.Height);
            writer.Write(epb.Depth);
            writer.Write(epb.Unknown01);
            writer.Write(epb.MetaTags);

            writer.Write(epb.Unknown02);
            writer.Write(epb.LightCount); // TODO: Count the number of light blocks in the model
            writer.Write(epb.DoorCount);
            writer.Write(epb.DeviceCount); // TODO: Count the number of devices in the model
            writer.Write(epb.UnknownCount02);
            writer.Write((UInt32)epb.Blocks.Count);
            writer.Write(epb.UnknownCount03);
            writer.Write(epb.TriangleCount);
            writer.Write(epb.UnknownCount04); //v24
            writer.Write(epb.UnknownCount05); //v24
            writer.Write(epb.UnknownCount06); //v24

            writer.Write((UInt16)epb.BlockCounts.Count);
            foreach (BlockType type in epb.BlockCounts.Keys)
            {
                writer.Write(type);
                writer.Write(epb.BlockCounts[type]);
            }

            writer.Write(epb.DeviceGroups);
            writer.WriteEpbDataSection(epb, GenerateSection0);
            writer.WriteEpbDataSection(epb, GenerateSection1, DataTypeFlags.Unknown0001);
        }

        public static void Write(this BinaryWriter writer, BlockType type)
        {
            writer.Write((UInt16)type.Id);
        }

        #region MetaTags
        public static void Write(this BinaryWriter writer, Dictionary<MetaTagKey, MetaTag> dictionary)
        {
            writer.Write((UInt16)dictionary.Count);
            foreach (MetaTag tag in dictionary.Values)
            {
                switch (tag.TagType)
                {
                    case MetaTagType.String:
                        writer.Write((MetaTagString)tag);
                        break;
                    case MetaTagType.UInt16:
                        writer.Write((MetaTagUInt16)tag);
                        break;
                    case MetaTagType.UInt32:
                        writer.Write((MetaTagUInt32)tag);
                        break;
                    case MetaTagType.Unknownx03:
                        writer.Write((MetaTag03)tag);
                        break;
                    case MetaTagType.Unknownx04:
                        writer.Write((MetaTag04)tag);
                        break;
                    case MetaTagType.Unknownx05:
                        writer.Write((MetaTag05)tag);
                        break;
                }
            }
        }

        public static void Write(this BinaryWriter writer, MetaTagString tag)
        {
            writer.Write((MetaTag)tag);
            writer.WriteEpString(tag.Value);
        }
        public static void Write(this BinaryWriter writer, MetaTagUInt16 tag)
        {
            writer.Write((MetaTag)tag);
            writer.Write(tag.Value);
        }
        public static void Write(this BinaryWriter writer, MetaTagUInt32 tag)
        {
            writer.Write((MetaTag)tag);
            writer.Write(tag.Value);
            writer.Write(tag.Unknown);
        }
        public static void Write(this BinaryWriter writer, MetaTag03 tag)
        {
            writer.Write((MetaTag)tag);
            writer.Write(tag.Value);
            writer.Write(tag.Unknown);
        }
        public static void Write(this BinaryWriter writer, MetaTag04 tag)
        {
            writer.Write((MetaTag)tag);
            writer.Write(tag.Value);
        }
        public static void Write(this BinaryWriter writer, MetaTag05 tag)
        {
            writer.Write((MetaTag)tag);
            writer.Write(tag.Value.ToBinary());
            writer.Write(tag.Unknown);
        }

        public static void Write(this BinaryWriter writer, MetaTag tag)
        {
            writer.Write((UInt32)tag.Key);
            writer.Write((UInt32)tag.TagType);
        }

        #endregion MetaTags

        #region Devices

        public static void Write(this BinaryWriter writer, List<DeviceGroup> groups)
        {
            writer.Write((byte)5);
            writer.Write((UInt16)groups.Count);
            foreach (var group in groups)
            {
                writer.Write(group);
            }
        }

        public static void Write(this BinaryWriter writer, DeviceGroup group)
        {
            writer.WriteEpString(group.Name);
            writer.Write(group.DeviceGroupUnknown03);
            writer.Write(group.DeviceGroupUnknown01);
            writer.Write(group.Shortcut);
            writer.Write((UInt16)group.Entries.Count);
            foreach (var device in group.Entries)
            {
                writer.Write(device);
            }
        }

        public static void Write(this BinaryWriter writer, DeviceGroupEntry entry)
        {
            writer.Write(entry.Pos);
            writer.WriteEpString(entry.Name);
        }
        #endregion Devices

        #endregion Blueprint

        #region DataSection
        public static void WriteEpbDataSection(this BinaryWriter writer, Blueprint epb, Func<Blueprint, byte[]> generateData, DataTypeFlags flags = DataTypeFlags.Zipped)
        {
            byte[] blockBuffer = generateData(epb);
            if ((flags & DataTypeFlags.Zipped) != 0)
            {
                using (MemoryStream outputMemStream = new MemoryStream())
                {
                    using (MemoryStream memStreamIn = new MemoryStream(blockBuffer))
                    {
                        ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);
                        ZipEntry newEntry = new ZipEntry("0");
                        newEntry.DateTime = DateTime.Now;
                        newEntry.CompressionMethod = CompressionMethod.Deflated;
                        newEntry.Size = blockBuffer.Length;         // If the size is not set, it will force zip64! Zip64 is not supported by Empyrion
                        zipStream.PutNextEntry(newEntry);
                        StreamUtils.Copy(memStreamIn, zipStream, new byte[4096]);
                        zipStream.CloseEntry();
                        zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
                        zipStream.Close();                  // Must finish the ZipOutputStream before using outputMemStream.
                    }
                    outputMemStream.Position = 0;
                    byte[] zipBuffer = outputMemStream.ToArray();
                    zipBuffer[0] = 0x00;
                    zipBuffer[1] = 0x00;
                    writer.Write((UInt32)zipBuffer.Length);
                    writer.Write((UInt16)DataTypeFlags.Zipped);
                    writer.Write(zipBuffer);
                }
            }
            else
            {
                writer.Write((UInt32)blockBuffer.Length);
                writer.Write((UInt16)0);
                writer.Write(blockBuffer);
            }
        }

        private static long AddStringToList(Blueprint epb, List<byte> byteList, string s)
        {
            long byteCount = 0;
            if (s == null)
            {
                byteList.Add(0); // Zero length string
                return 1;
            }

            byte[] buf = System.Text.Encoding.UTF8.GetBytes(s);
            int len = buf.Length;
            do
            {
                byte b = (byte)(len & 0x7f);
                len >>= 7;
                if (len > 0)
                {
                    b += 0x80; // Set top bit to indicate more length bytes
                }
                byteList.Add(b);
                byteCount += 1;
            } while (len > 0);
            byteList.AddRange(buf);
            byteCount += buf.Length;
            return byteCount;
        }

        public static long AddMatrixToList(Blueprint epb, List<byte> list, Func<Blueprint, Block, List<byte>, bool> func)
        {
            bool[] m = new bool[epb.Width * epb.Height * epb.Depth];
            List<byte> tmpList = new List<byte>();

            for (UInt32 z = 0; z < epb.Depth; z++)
            {
                for (UInt32 y = 0; y < epb.Height; y++)
                {
                    for (UInt32 x = 0; x < epb.Width; x++)
                    {
                        Block block = epb.Blocks[(byte)x, (byte)y, (byte)z];
                        if (func(epb, block, tmpList))
                        {
                            //BlockPos pos = block.Position;
                            //m[pos.Z * epb.Width * epb.Height + pos.Y * epb.Width + pos.X] = true;
                            m[z * epb.Width * epb.Height + y * epb.Width + x] = true;
                        }
                    }
                }
            }

            byte[] matrix = m.ToByteArray();
            UInt32 matrixLength = (UInt32)matrix.Length;
            list.AddRange(BitConverter.GetBytes(matrixLength));
            list.AddRange(matrix);
            list.AddRange(tmpList);
            return 4 + matrix.Length + tmpList.Count;
        }

        #endregion DataSection

        #region Section0

        private static byte[] GenerateSection0(Blueprint epb)
        {
            long byteCount = 0;
            List<byte> byteList = new List<byte>();

            byteCount = AddBlockTypesToList(epb, byteCount, byteList);
            byteCount = AddDamageStatesToList(epb, byteCount, byteList);
            byteCount = AddUnknown02ToList(byteList, byteCount);
            byteCount = AddColourMatrixToList(epb, byteCount, byteList);
            byteCount = AddTextureMatrixToList(epb, byteCount, byteList);
            byteCount = AddTextureFlipMatrixToList(epb, byteCount, byteList);
            byteCount = AddSymbolMatrixToList(epb, byteCount, byteList);
            byteCount = AddSymbolRotationMatrixToList(epb, byteCount, byteList);
            byteCount = AddBlockTagsToList(epb, byteCount, byteList);
            byteCount = AddLockCodesToList(epb, byteCount, byteList);
            byteCount = AddSignalSourcesToList(epb, byteCount, byteList);
            byteCount = AddSignalTargetsToList(epb, byteCount, byteList);
            byteCount = AddSignalOperatorsToList(epb, byteCount, byteList);
            byteCount = AddCustomNamesToList(epb, byteCount, byteList);
            byteCount = AddUnknown08ToList(epb, byteCount, byteList);
            byteCount = AddCustomPalettesToList(epb, byteCount, byteList);

            return byteList.ToArray();
        }

        private static long AddBlockTypesToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                if (block == null)
                {
                    return false;
                }

                UInt32 data = (UInt32)block.BlockType.Id
                            + (UInt32)((byte)block.Rotation << 11)
                            +      (UInt32)(block.Unknown00 << 16)
                            +      (UInt32)(block.Variant   << 25);
                list.AddRange(BitConverter.GetBytes(data));
                return true;
            });
            return byteCount;
        }

        private static long AddDamageStatesToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                if (block == null || block.DamageState == 0)
                {
                    return false;
                }
                list.AddRange(BitConverter.GetBytes(block.DamageState));
                return true;
            });
            return byteCount;
        }

        private static long AddUnknown02ToList(List<byte> byteList, long byteCount)
        {
            byteList.Add(0x01);
            byteCount += 1;
            byteList.Add(0x7f);
            byteCount += 1;
            return byteCount;
        }

        private static long AddColourMatrixToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                if (block == null)
                {
                    return false;
                }

                UInt32 bits = 0;
                UInt32 factor = 1;
                for (int i = 0; i < 6; i++)
                {
                    bits += (byte)block.Colours[i] * factor;
                    factor = factor << 5;
                }

                if (bits == 0)
                {
                    return false;
                }

                list.AddRange(BitConverter.GetBytes(bits));
                return true;
            });
            return byteCount;
        }

        private static long AddTextureMatrixToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                if (block == null)
                {
                    return false;
                }

                UInt64 bits = 0;
                UInt64 factor = 1;
                for (int i = 0; i < 6; i++)
                {
                    bits += block.Textures[i] * factor;
                    factor = factor << 6;
                }

                if (bits == 0)
                {
                    return false;
                }

                list.AddRange(BitConverter.GetBytes(bits));
                return true;
            });
            return byteCount;
        }

        private static long AddTextureFlipMatrixToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                if (block == null)
                {
                    return false;
                }

                byte bits = 0;
                int factor = 1;
                for (int i = 0; i < 6; i++)
                {
                    bits += (byte)((block.TextureFlips[i] ? 1 : 0) * factor);
                    factor = factor << 1;
                }

                if (bits == 0)
                {
                    return false;
                }

                list.Add(bits);
                return true;
            });
            return byteCount;
        }

        private static long AddSymbolMatrixToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                if (block == null)
                {
                    return false;
                }

                UInt32 bits = 0;
                UInt32 factor = 1;
                for (int i = 0; i < 6; i++)
                {
                    bits += block.Symbols[i] * factor;
                    factor = factor << 5;
                }

                bits += block.SymbolPage * factor;

                if (bits == 0)
                {
                    return false;
                }

                list.AddRange(BitConverter.GetBytes(bits));
                return true;
            });
            return byteCount;
        }

        private static long AddSymbolRotationMatrixToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                if (block == null)
                {
                    return false;
                }

                UInt32 bits = 0;
                UInt32 factor = 1;
                for (int i = 0; i < 6; i++)
                {
                    bits += (byte)block.SymbolRotations[i] * factor;
                    factor = factor << 2;
                }

                if (bits == 0)
                {
                    return false;
                }

                list.AddRange(BitConverter.GetBytes(bits));
                return true;
            });
            return byteCount;
        }

        private static long AddBlockTagsToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            int nBlocks = 0;
            List<byte> l = new List<byte>();
            foreach (Block block in epb.Blocks)
            {
                if (block.Tags.Count != 0)
                {
                    byteCount += AddBlockPosToList(epb, l, block.Position);
                    l.Add(1); // Unknown06
                    byteCount += 1;
                    byteCount += AddBlockTagListToList(epb, l, block.Tags.Values.ToArray()); // TODO: Is the order of these significant?
                    nBlocks++;
                }
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)nBlocks));
            byteCount += 2;
            byteList.AddRange(l.ToArray());
            return byteCount;
        }

        private static long AddLockCodesToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            int nCodes = 0;
            List<byte> l = new List<byte>();
            foreach (Block block in epb.Blocks)
            {
                if (block.HasLockCode)
                {
                    byteCount += AddBlockPosToList(epb, l, block.Position);
                    UInt16 data = (UInt16)(block.LockCode + (block.LockCodeFlags1 << 14));
                    l.AddRange(BitConverter.GetBytes(data));
                    byteCount += 2;
                    l.AddRange(BitConverter.GetBytes(block.LockCodeFlags2)); //v25
                    nCodes++;
                }
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)nCodes));
            byteCount += 2;
            byteList.AddRange(l);
            return byteCount;
        }

        private static long AddSignalSourcesToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            UInt16 nSignalSources = (UInt16)epb.SignalSources.Count;
            byteList.AddRange(BitConverter.GetBytes(nSignalSources));
            byteCount += 2;
            foreach (SignalSource source in epb.SignalSources)
            {
                byteList.Add(source.Unknown01);
                byteCount += 1;
                byteCount += AddBlockTagListToList(epb, byteList, source.Tags.Values.ToArray()); // TODO: Is the order of these significant?
            }
            return byteCount;
        }

        private static long AddSignalTargetsToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            UInt16 nSignalSources = 0;
            List<byte> bufAll = new List<byte>();
            List<byte> bufOne = new List<byte>();
            string signalName = "";
            UInt16 nSignalTargets = 0;
            foreach (SignalTarget target in epb.SignalTargets.OrderBy(target => target.SignalName))
            {
                if (signalName != target.SignalName)
                {
                    if (nSignalTargets != 0)
                    {
                        bufAll.AddRange(BitConverter.GetBytes(nSignalTargets));
                        byteCount += 2;
                        bufAll.AddRange(bufOne.ToArray());
                        bufOne.Clear();
                        nSignalTargets = 0;
                    }
                    signalName = target.SignalName;
                    byteCount += AddStringToList(epb, bufAll, signalName);
                    nSignalSources++;
                }
                bufOne.Add(target.Unknown01);
                byteCount += 1;
                byteCount += AddBlockTagListToList(epb, bufOne, target.Tags.Values.ToArray()); // TODO: Is the order of these significant?
                nSignalTargets++;
            }
            if (nSignalTargets != 0)
            {
                bufAll.AddRange(BitConverter.GetBytes(nSignalTargets));
                byteCount += 2;
                bufAll.AddRange(bufOne.ToArray());
            }

            byteList.AddRange(BitConverter.GetBytes(nSignalSources));
            byteCount += 2;
            byteList.AddRange(bufAll.ToArray());
            return byteCount;
        }

        private static long AddSignalOperatorsToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            UInt16 nSignalOperators = (UInt16)epb.SignalOperators.Count;
            byteList.AddRange(BitConverter.GetBytes(nSignalOperators));
            byteCount += 2;
            foreach (SignalOperator signalOperator in epb.SignalOperators)
            {
                byteList.Add(signalOperator.Unknown01);
                byteCount += 1;
                byteCount += AddBlockTagListToList(epb, byteList, signalOperator.Tags.Values.ToArray()); // TODO: Is the order of these significant?
            }
            return byteCount;
        }

        private static long AddCustomNamesToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            UInt16 nCustom = (UInt16)epb.CustomNames.Count;
            byteList.AddRange(BitConverter.GetBytes(nCustom));
            byteCount += 2;
            foreach (string name in epb.CustomNames)
            {
                byteCount += AddStringToList(epb, byteList, name);
            }
            return byteCount;
        }

        private static long AddUnknown08ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            UInt16 nUnknown08 = (UInt16)epb.Unknown08.Count;
            byteList.AddRange(BitConverter.GetBytes(nUnknown08));
            byteCount += 2;
            foreach (string key in epb.Unknown08.Keys)
            {
                byteCount += AddStringToList(epb, byteList, key);
                byteList.AddRange(epb.Unknown08[key]);
                byteCount += epb.Unknown08[key].Length;
            }
            return byteCount;
        }

        private static long AddCustomPalettesToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            byteList.Add(1); // TODO: I used to think that this is a count, now I think it is a version type thing
            byteCount += 1;
            byteList.AddRange(BitConverter.GetBytes((UInt32)epb.Palette.Length));
            byteCount += 4;
            for (int i = 1; i < epb.Palette.Length; i++)
            {
                Colour c = epb.Palette[i];
                byteList.Add(c.R);
                byteList.Add(c.G);
                byteList.Add(c.B);
                byteCount += 3;
            }
            return byteCount;
        }
        
        #region BlockTags
        private static long AddBlockTagListToList(Blueprint epb, List<byte> byteList, BlockTag[] tags)
        {
            long byteCount = 0;
            byteList.AddRange(BitConverter.GetBytes((UInt16)tags.Length));
            foreach (BlockTag tag in tags)
            {
                byteList.Add((byte)tag.BlockTagType);
                byteCount += 1;
                byteCount += AddStringToList(epb, byteList, tag.Name);
                switch (tag)
                {
                    case BlockTagBool tagBool:
                        byteCount += AddBlockTagValueToList(epb, byteList, tagBool);
                        break;
                    case BlockTagColour tagColour:
                        byteCount += AddBlockTagValueToList(epb, byteList, tagColour);
                        break;
                    case BlockTagFloat tagFloat:
                        byteCount += AddBlockTagValueToList(epb, byteList, tagFloat);
                        break;
                    case BlockTagPos tagPos:
                        byteCount += AddBlockTagValueToList(epb, byteList, tagPos);
                        break;
                    case BlockTagString tagString:
                        byteCount += AddBlockTagValueToList(epb, byteList, tagString);
                        break;
                    case BlockTagUInt32 tagUInt32:
                        byteCount += AddBlockTagValueToList(epb, byteList, tagUInt32);
                        break;
                }
            }
            return byteCount;
        }

        private static long AddBlockTagValueToList(Blueprint epb, List<byte> byteList, BlockTagBool tag)
        {
            byteList.Add(tag.Value ? (byte)1 : (byte)0);
            return 1;
        }
        private static long AddBlockTagValueToList(Blueprint epb, List<byte> byteList, BlockTagColour tag)
        {
            byteList.AddRange(BitConverter.GetBytes(tag.Value));
            return 4;
        }
        private static long AddBlockTagValueToList(Blueprint epb, List<byte> byteList, BlockTagFloat tag)
        {
            byteList.AddRange(BitConverter.GetBytes(tag.Value));
            return 4;
        }
        private static long AddBlockTagValueToList(Blueprint epb, List<byte> byteList, BlockTagPos tag)
        {
            return AddBlockPosToList(epb, byteList, tag.Value);
        }

        private static long AddBlockTagValueToList(Blueprint epb, List<byte> byteList, BlockTagString tag)
        {
            return AddStringToList(epb, byteList, tag.Value);
        }

        private static long AddBlockTagValueToList(Blueprint epb, List<byte> byteList, BlockTagUInt32 tag)
        {
            byteList.AddRange(BitConverter.GetBytes(tag.Value));
            return 4;
        }
        #endregion BlockTags
        
        private static long AddBlockPosToList(Blueprint epb, List<byte> byteList, BlockPos pos)
        {
            UInt32 data = ((UInt32)(pos.X  & 0xff) << 20)
                        + ((UInt32)(pos.Y  & 0xff) << 12)
                        + ((UInt32)(pos.Z  & 0xff) <<  0)
                        + ((UInt32)(pos.U1 & 0x0f) << 28)
                        + ((UInt32)(pos.U2 & 0x0f) <<  8);
            byteList.AddRange(BitConverter.GetBytes(data));
            return 4;
        }

        #endregion Section0

        #region Section1
        private static byte[] GenerateSection1(Blueprint epb)
        {
            long byteCount = 0;
            List<byte> byteList = new List<byte>();

            byteCount = AddFillerMatrixToList(epb, byteCount, byteList);
            byteCount = AddUnknown12ToList(epb, byteCount, byteList);
            byteCount = AddUnknown13ToList(epb, byteCount, byteList);
            byteCount = AddUnknown14ToList(epb, byteCount, byteList);
            byteCount = AddUnknown15ToList(epb, byteCount, byteList);
            byteCount = AddUnknown16ToList(epb, byteCount, byteList);
            byteCount = AddUnknown17ToList(epb, byteCount, byteList);
            byteCount = AddUnknown18ToList(epb, byteCount, byteList);
            byteCount = AddUnknown19ToList(epb, byteCount, byteList);
            byteCount = AddUnknown20ToList(epb, byteCount, byteList);
            byteCount = AddUnknown21ToList(epb, byteCount, byteList);

            return byteList.ToArray();
        }

        private static long AddFillerMatrixToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            BlockType t = BlockType.GetBlockType(1681); // Filler block type
            if (!epb.BlockCounts.ContainsKey(t) || epb.BlockCounts[t] == 0)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)0));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) => block == null || block.BlockType.Id == 1681);  // Filler block type
            return byteCount;
        }

        private static long AddUnknown12ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)12));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        private static long AddUnknown13ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)13));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        private static long AddUnknown14ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)14));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        private static long AddUnknown15ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)15));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        private static long AddUnknown16ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)16));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        private static long AddUnknown17ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)17));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        private static long AddUnknown18ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)18));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        private static long AddUnknown19ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)19));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        private static long AddUnknown20ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)20));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        private static long AddUnknown21ToList(Blueprint epb, long byteCount, List<byte> byteList)
        {
            if (true)
            {
                return byteCount;
            }
            byteList.AddRange(BitConverter.GetBytes((UInt16)21));
            byteCount += 2;
            byteCount += AddMatrixToList(epb, byteList, (blueprint, block, list) =>
            {
                return false;
            });
            return byteCount;
        }

        #endregion Unknown

        public static void Write(this BinaryWriter writer, BlockPos pos)
        {
            UInt32 data = (UInt32)(pos.U1 << 28 | pos.X << 20 | pos.Y << 12 | pos.U2 << 8 | pos.Z);
            writer.Write(data);
        }
        public static void WriteEpString(this BinaryWriter writer, string s)
        {
            if (s == null)
            {
                writer.Write((byte)0); // Zero length string
                return;
            }
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(s);
            int len = buf.Length;
            do
            {
                byte b = (byte)(len & 0x7f);
                len >>= 7;
                if (len > 0)
                {
                    b += 0x80; // Set top bit to indicate more length bytes
                }
                writer.Write(b);
            } while (len > 0);
            writer.Write(buf);
        }

    }
}
