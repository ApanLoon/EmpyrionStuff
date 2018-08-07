using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace EPBLib.Helpers
{
    public static class BinaryWriterExtensions
    {
        #region EpBlueprint
        public static readonly UInt32 EpbIdentifier = 0x78945245;
        public static UInt32 EpbVersion = 20;

        public static void Write(this BinaryWriter writer, EpBlueprint epb)
        {
            writer.Write(EpbIdentifier);
            writer.Write(EpbVersion);
            writer.Write((byte)epb.Type);
            writer.Write(epb.Width);
            writer.Write(epb.Height);
            writer.Write(epb.Depth);
            writer.Write((UInt16)1); //Unknown01
            writer.Write(epb.MetaTags);

            writer.Write((UInt16)0); //Unknown02
            writer.Write((UInt32)0); // TODO: Count the number of light blocks in the model
            writer.Write((UInt32)0); // UnknownCount01
            writer.Write((UInt32)0); // TODO: Count the number of devices in the model
            writer.Write((UInt32)0); // UnknownCount02
            writer.Write((UInt32)epb.Blocks.Length);
            writer.Write((UInt32)0); // UnknownCount03
            writer.Write((UInt32)0); // TODO: Count the number of triangles in the model

            Dictionary<EpbBlock.EpbBlockType, UInt32> blockCounts = new Dictionary<EpbBlock.EpbBlockType, uint>();
            for (UInt32 z = 0; z < epb.Depth; z++)
            {
                for (UInt32 y = 0; y < epb.Height; y++)
                {
                    for (UInt32 x = 0; x < epb.Width; x++)
                    {
                        EpbBlock block = epb.Blocks[x, y, z];
                        if (block != null)
                        {
                            if (!blockCounts.ContainsKey(block.BlockType))
                            {
                                blockCounts.Add(block.BlockType, 0);
                            }
                            blockCounts[block.BlockType]++;
                        }
                    }
                }
            }

            writer.Write((UInt16)blockCounts.Count);
            foreach (EpbBlock.EpbBlockType type in blockCounts.Keys)
            {
                writer.Write(type);
                writer.Write(blockCounts[type]);
            }

            writer.Write((byte)0x04); // unknown05

            writer.Write(epb.DeviceGroups);
            writer.WriteEpbBlocks(epb);
        }
        #endregion EpBlueprint

        #region EpbBlocks

        public static void Write(this BinaryWriter writer, EpbBlock.EpbBlockType type)
        {
            writer.Write((UInt16) type);
        }

        public static void WriteEpbBlocks(this BinaryWriter writer, EpBlueprint epb)
        {
            long byteCount = 0;
            List<byte> byteList = new List<byte>();

            // Blocks:
            byteCount += AddEpbMatrixToList(epb, byteList, (blueprint, x, y, z, list) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
                if (block == null)
                {
                    return false;
                }

                UInt32 data = (UInt32)block.BlockType + (UInt32)((byte)block.Rotation << 11) + (UInt32)(block.Unknown00 << 16) + (UInt32)(block.Variant << 25);
                list.AddRange(BitConverter.GetBytes(data));
                return true;
            });

            // Unknown3:
            byteCount += AddEpbMatrixToList(epb, byteList, (blueprint, x, y, z, list) =>
            {
                return false;
            });

            //nUnknown4:
            byteList.Add(0x01);
            byteCount += 1;
            //Unknown4:
            byteList.Add(0x7f);
            byteCount += 1;

            // Colours:
            byteCount += AddEpbMatrixToList(epb, byteList, (blueprint, x, y, z, list) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
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

            // Textures:
            byteCount += AddEpbMatrixToList(epb, byteList, (blueprint, x, y, z, list) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
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

            // TextureFlip:
            byteCount += AddEpbMatrixToList(epb, byteList, (blueprint, x, y, z, list) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
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

            // Symbols:
            byteCount += AddEpbMatrixToList(epb, byteList, (blueprint, x, y, z, list) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
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

            // SymbolRotations:
            byteCount += AddEpbMatrixToList(epb, byteList, (blueprint, x, y, z, list) =>
            {
                EpbBlock block = epb.Blocks[x, y, z];
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

            byteList.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            byteCount += 14;

            byte[] blockBuffer = byteList.ToArray();




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
                writer.Write(zipBuffer);
            }
        }


        public static long AddEpbMatrixToList(EpBlueprint epb, List<byte> list, Func<EpBlueprint, UInt32, UInt32, UInt32, List<byte>, bool> func)
        {
            bool[] m = new bool[epb.Width * epb.Height * epb.Depth];
            List<byte> tmpList = new List<byte>();

            for (UInt32 z = 0; z < epb.Depth; z++)
            {
                for (UInt32 y = 0; y < epb.Height; y++)
                {
                    for (UInt32 x = 0; x < epb.Width; x++)
                    {
                        if (func(epb, x, y, z, tmpList))
                        {
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




        #endregion EpbBlocks

        #region EpbMetaTags
        public static void Write(this BinaryWriter writer, Dictionary<EpMetaTagKey, EpMetaTag> dictionary)
        {
            writer.Write((UInt16)dictionary.Count);
            foreach (EpMetaTag tag in dictionary.Values)
            {
                switch (tag.TagType)
                {
                    case EpMetaTagType.String:
                        writer.Write((EpMetaTagString)tag);
                        break;
                    case EpMetaTagType.UInt16:
                        writer.Write((EpMetaTagUInt16)tag);
                        break;
                    case EpMetaTagType.Unknownx02:
                        writer.Write((EpMetaTag02)tag);
                        break;
                    case EpMetaTagType.Unknownx03:
                        writer.Write((EpMetaTag03)tag);
                        break;
                    case EpMetaTagType.Unknownx04:
                        writer.Write((EpMetaTag04)tag);
                        break;
                    case EpMetaTagType.Unknownx05:
                        writer.Write((EpMetaTag05)tag);
                        break;
                }
            }
        }

        public static void Write(this BinaryWriter writer, EpMetaTagString tag)
        {
            writer.Write((EpMetaTag)tag);
            writer.WriteEpString(tag.Value);
        }
        public static void Write(this BinaryWriter writer, EpMetaTagUInt16 tag)
        {
            writer.Write((EpMetaTag)tag);
            writer.Write(tag.Value);
        }
        public static void Write(this BinaryWriter writer, EpMetaTag02 tag)
        {
            writer.Write((EpMetaTag)tag);
            writer.Write(tag.Value);
        }
        public static void Write(this BinaryWriter writer, EpMetaTag03 tag)
        {
            writer.Write((EpMetaTag)tag);
            writer.Write(tag.Value);
        }
        public static void Write(this BinaryWriter writer, EpMetaTag04 tag)
        {
            writer.Write((EpMetaTag)tag);
            writer.Write(tag.Value);
        }
        public static void Write(this BinaryWriter writer, EpMetaTag05 tag)
        {
            writer.Write((EpMetaTag)tag);
            writer.Write(tag.Value);
        }

        public static void Write(this BinaryWriter writer, EpMetaTag tag)
        {
            writer.Write((UInt32)tag.Key);
            writer.Write((UInt32)tag.TagType);
        }

        #endregion EpbMetaTags

        #region EpbDevices

        public static void Write(this BinaryWriter writer, List<EpbDeviceGroup> groups)
        {
            writer.Write((UInt16)groups.Count);
            foreach (var group in groups)
            {
                writer.Write(group);
            }
        }

        public static void Write(this BinaryWriter writer, EpbDeviceGroup group)
        {
            writer.WriteEpString(group.Name);
            writer.Write(group.DeviceGroupUnknown01);
            writer.Write(group.DeviceGroupUnknown02);
            writer.Write((UInt16)group.Entries.Count);
            foreach (var device in group.Entries)
            {
                writer.Write(device);
            }
        }

        public static void Write(this BinaryWriter writer, EpbDeviceGroupEntry entry)
        {
            writer.Write(entry.Pos);
            writer.WriteEpString(entry.Name);
        }
        #endregion EpbDevices

        public static void Write(this BinaryWriter writer, EpbBlockPos pos)
        {
            UInt32 data = (UInt32)(pos.U1 << 28 | pos.X << 20 | pos.Y << 12 | pos.U2 << 8 | pos.Z);
            writer.Write(data);
        }
        public static void WriteEpString(this BinaryWriter writer, string s)
        {
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(s);
            writer.Write((byte)buf.Length); //TODO: Handle multi-byte length
            writer.Write(buf);
        }

    }
}
