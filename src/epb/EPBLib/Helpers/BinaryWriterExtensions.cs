﻿using System;
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

        public static readonly byte[] BoilerPlate_Unknown01 = new byte[]
        {
            0x01, 0x00
        };
        public static readonly byte[] BoilerPlate_UnknownXX = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, // nUnknown04
            0x2e, 0x02,
            0x01, 0x00, 0x00, 0x00, 0x04 // unknown05
        };


        public static void Write(this BinaryWriter writer, EpBlueprint epb)
        {
            writer.Write(EpbIdentifier);
            writer.Write(EpbVersion);
            writer.Write((byte) epb.Type);
            writer.Write(epb.Width);
            writer.Write(epb.Height);
            writer.Write(epb.Depth);
            writer.Write(BoilerPlate_Unknown01);
            writer.Write(epb.MetaTags);
            writer.Write(BoilerPlate_UnknownXX);
            writer.Write(epb.DeviceGroups);
            writer.WriteEpbBlocks(epb);
        }
        #endregion EpBlueprint

        #region EpbBlocks
        public static void WriteEpbBlocks(this BinaryWriter writer, EpBlueprint epb)
        {
            byte[] blockBuffer = new byte[]
            {
                0x01, 0x00, 0x00, 0x00, 0x01, 0x2e, 0x0a, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x7f,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

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
                    case EpMetaTagType.Unknownx01:
                        writer.Write((EpMetaTag01)tag);
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
        public static void Write(this BinaryWriter writer, EpMetaTag01 tag)
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
            writer.Write(group.Flags);
            writer.Write((UInt16)group.Entries.Count);
            foreach (var device in group.Entries)
            {
                writer.Write(device);
            }
        }

        public static void Write(this BinaryWriter writer, EpbDeviceGroupEntry entry)
        {
            writer.Write(entry.Unknown);
            writer.WriteEpString(entry.Name);
        }
        #endregion EpbDevices

        public static void WriteEpString(this BinaryWriter writer, string s)
        {
            byte[] buf = System.Text.Encoding.ASCII.GetBytes(s);
            writer.Write((byte)buf.Length);
            writer.Write(buf);
        }

    }
}
