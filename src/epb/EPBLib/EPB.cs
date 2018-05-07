
using EPBLib.Helpers;
using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace EPBLib
{
    public class EPB
    {
        #region Types
        public enum BluePrintType
        {
            Voxel         = 0x00,
            Base          = 0x02,
            SmallVessel   = 0x04,
            CapitalVessel = 0x08,
            HoverVessel   = 0x10
        }

        public enum MetaKey
        {
            CreatorID    = 0x0b,
            CreatorName  = 0x0a,
            OwnerId      = 0x0d,
            OwnerName    = 0x0c,
            UnknownMeta1 = 0x10,
            UnknownMeta2 = 0x12
        }
        public enum MetaType
        {
            String = 0x00000000,
            Unknown = 0x05000000
        }
        #endregion Types

        protected static readonly byte[] ZipDataStartPattern = new byte[] {0x00, 0x00, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00};

        #region Fields
        public UInt32 Version;
        public BluePrintType Type;
        public UInt32 Width;
        public UInt32 Height;
        public UInt32 Depth;
        #endregion Fields

        public EPB (string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                long fileSize = stream.Length;

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    try
                    {
                        ReadFile(reader, fileSize);
                    }
                    catch (System.Exception ex)
                    {
                        throw new Exception("Failed reading EPB file", ex);
                    }
                }
            }
        }

        protected void ReadFile(BinaryReader reader, long fileSize)
        {
            long bytesLeft = fileSize;
 
            UInt32 identifier = reader.ReadUInt32();
            if (identifier != 0x78945245)
            {
                throw new Exception($"Unknown file identifier. 0x{identifier:x4}");
            }
            Version = reader.ReadUInt32();
            if (Version < 12)
            {
                throw new Exception($"Version {Version} is too old. (Needs to be at least 12)");
            }
            Type = (BluePrintType)reader.ReadByte();
            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            Depth = reader.ReadUInt32();
            bytesLeft -= 4 + 4 + 1 + 4 + 4 + 4;

            Console.WriteLine($"Version:  {Version}");
            Console.WriteLine($"Type:     {Type}");
            Console.WriteLine($"Width:    {Width}");
            Console.WriteLine($"Height:   {Height}");
            Console.WriteLine($"Depth:    {Depth}");

            byte[] unknown01 = reader.ReadBytes(132);
            bytesLeft -= 132;
            Console.WriteLine($"Unknown01: {BitConverter.ToString(unknown01).Replace("-", "")}");

            UInt32 nMeta = reader.ReadUInt32(); //TODO: Something fishy here! In some files this is one short!
            bytesLeft -= 4;
            Console.WriteLine($"nMeta:   {nMeta}");
            MetaKey metaKey;
            MetaType metaType;
            for (int i = 0; i < nMeta; i++)
            {
                metaKey = (MetaKey)reader.ReadUInt32();
                metaType = (MetaType)reader.ReadUInt32();
                bytesLeft -= 4 + 4;
                switch (metaType)
                {
                    case MetaType.String:
                        string metaStringValue = "";
                        bytesLeft = ReadString(reader, bytesLeft, out metaStringValue);
                        Console.WriteLine($"{metaKey,-15}: \"{metaStringValue}\"");
                        break;
                    case MetaType.Unknown:
                        byte[] unknownMeta2 = reader.ReadBytes(4 * 5);
                        bytesLeft -= 4 * 5;
                        Console.WriteLine($"{metaKey,-15}: metaType={metaType} {BitConverter.ToString(unknownMeta2).Replace("-", "")}");
                        break;
                }                
            }

            byte[] unknown03 = reader.ReadBytes(19);
            bytesLeft -= 19;
            Console.WriteLine($"Unknown03: {BitConverter.ToString(unknown03).Replace("-", "")}");

            if (Version <= 12)
            {
                byte[] unknown04 = reader.ReadBytes(5);
                bytesLeft -= 5;
                Console.WriteLine($"Unknown04: 5 {BitConverter.ToString(unknown04).Replace("-", "")}");
            }
            else
            {
                UInt16 nUnknown04 = reader.ReadUInt16();
                bytesLeft -= 2;
                int bytesToRead = nUnknown04 * 6 - 4; // First value is 2 bytes, the rest are 6 bytes each
                byte[] unknown04 = reader.ReadBytes(bytesToRead);
                bytesLeft -= bytesToRead;
                Console.WriteLine($"Unknown04: {nUnknown04:x4} {BitConverter.ToString(unknown04).Replace("-", "")}");
            }

            byte[] unknown05 = reader.ReadBytes(5);
            bytesLeft -= 5;
            Console.WriteLine($"Unknown05: {BitConverter.ToString(unknown05).Replace("-", "")}");

            UInt16 nGroups = reader.ReadUInt16();
            bytesLeft -= 2;
            Console.WriteLine($"Groups ({nGroups})");
            for (int g = 0; g < nGroups; g++)
            {
                string groupName = "";
                bytesLeft = ReadString(reader, bytesLeft, out groupName);
                UInt16 groupUnknown01 = reader.ReadUInt16();
                UInt16 nDevicesInGroup = reader.ReadUInt16();
                bytesLeft -= 2 + 2;
                Console.WriteLine($"    {groupName} ({groupUnknown01:x4})");
                for (int d = 0; d < nDevicesInGroup; d++)
                {
                    UInt32 deviceUnknown01 = reader.ReadUInt32();
                    bytesLeft -= 4;
                    string deviceName = "";
                    bytesLeft = ReadString(reader, bytesLeft, out deviceName);
                    Console.WriteLine($"        device: {deviceUnknown01:x8} \"{deviceName}\"");
                }
            }

            // Here comes a block of unknown length, so read the rest and do some precarious searching for things:
            byte[] buf = reader.ReadBytes((int)bytesLeft);
            int dataStart;

            dataStart = buf.IndexOf(ZipDataStartPattern);
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
                        ReadBlocks(zipReader, entry.Size);
                    }
                }
            }
        }

        protected long ReadString(BinaryReader reader, long bytesLeft, out string s)
        {
            byte len = reader.ReadByte();
            s = (len == 0) ? "" : System.Text.Encoding.ASCII.GetString(reader.ReadBytes(len));
            return bytesLeft;
        }

        protected void ReadBlocks(BinaryReader reader, long length)
        {
            long bytesLeft = length;
            int blockCount = 0;
            bytesLeft = ReadMatrix("Blocks", reader, length, (r, x, y, z, b) =>
            {
                byte type     = r.ReadByte();
                byte rotation = r.ReadByte();
                byte unknown2 = r.ReadByte();
                byte variant  = r.ReadByte();
                blockCount++;
                Console.WriteLine($"    {blockCount} ({x}, {y}, {z}): Type=0x{type:x2} Rot=0x{rotation:x2} Unknown2=0x{unknown2:x2} Variant=0x{variant:x2}");
                return b - 4;
            });

            int unknown3Count = 0;
            bytesLeft = ReadMatrix("Unknown3", reader, length, (r, x, y, z, b) =>
            {
                byte unknown31 = r.ReadByte();
                byte unknown32 = r.ReadByte();
                byte unknown33 = r.ReadByte();
                byte unknown34 = r.ReadByte();
                unknown3Count++;
                Console.WriteLine($"    {unknown3Count} ({x}, {y}, {z}): 0x{unknown31:x2} 0x{unknown32:x2} 0x{unknown33:x2} 0x{unknown34:x2}");
                return b - 4;
            });

            int nUnknown4 = reader.ReadByte();
            bytesLeft -= 1;
            if (nUnknown4 == 0)
            {
                nUnknown4 = (int)(Width * Height * Depth); // blockCount;
            }
            byte[] unknown4 = reader.ReadBytes(nUnknown4);
            bytesLeft -= nUnknown4;
            Console.WriteLine($"Unknown4: {BitConverter.ToString(unknown4).Replace("-", "")}");

            int colourCount = 0;
            bytesLeft = ReadMatrix("Colour", reader, length, (r, x, y, z, b) =>
            {
                UInt32 bits = r.ReadUInt32();
                byte[] colours = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    colours[i] = (byte)(bits & 0x1f);
                    bits = bits >> 5;
                }
                colourCount++;
                Console.WriteLine($"    {colourCount} ({x}, {y}, {z}): {string.Join(", ", colours)}");
                return b - 4;
            });

            int TextureCount = 0;
            bytesLeft = ReadMatrix("Texture", reader, length, (r, x, y, z, b) =>
            {
                UInt64 bits = r.ReadUInt64();
                byte[] textures = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    textures[i] = (byte)(bits & 0x3f);
                    bits = bits >> 6;
                }
                TextureCount++;
                Console.WriteLine($"    {TextureCount} ({x}, {y}, {z}): {string.Join(", ", textures)}");
                return b - 8;
            });

            if (Version >= 20)
            {
                int unknown7Count = 0;
                bytesLeft = ReadMatrix("Unknown7", reader, length, (r, x, y, z, b) =>
                {
                    byte unknown71 = r.ReadByte();
                    unknown7Count++;
                    Console.WriteLine($"    {unknown7Count} ({x}, {y}, {z}): 0x{unknown71:x2}");
                    return b - 1;
                });
            }
            else
            {
                int unknown7Count = 0;
                bytesLeft = ReadMatrix("Unknown7", reader, length, (r, x, y, z, b) =>
                {
                    UInt32 unknown71 = r.ReadUInt32();
                    unknown7Count++;
                    Console.WriteLine($"    {unknown7Count} ({x}, {y}, {z}): 0x{unknown71:x8}");
                    return b - 4;
                });
            }

            int symbolCount = 0;
            bytesLeft = ReadMatrix("Symbol", reader, length, (r, x, y, z, b) =>
            {
                UInt32 bits = r.ReadUInt32();
                byte[] symbols = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    symbols[i] = (byte)(bits & 0x1f);
                    bits = bits >> 5;
                }
                byte symbolPage = (byte)bits;
                symbolCount++;
                Console.WriteLine($"    {symbolCount} ({x}, {y}, {z}): Page={symbolPage} {string.Join(", ", symbols)}");
                return b - 4;
            });

            if (Version >= 20) // TODO: I have no idea when this appeared
            {
                int unknown9Count = 0;
                bytesLeft = ReadMatrix("Unknown9", reader, length, (r, x, y, z, b) =>
                {
                    byte unknown91 = r.ReadByte();
                    byte unknown92 = r.ReadByte();
                    byte unknown93 = r.ReadByte();
                    byte unknown94 = r.ReadByte();
                    unknown9Count++;
                    Console.WriteLine($"    {unknown9Count} ({x}, {y}, {z}): 0x{unknown91:x2} 0x{unknown92:x2} 0x{unknown93:x2} 0x{unknown94:x2}");
                    return b - 4;
                });
            }

            byte[] remainingData = reader.ReadBytes((int)(bytesLeft));
            Console.WriteLine($"Remaining data: {BitConverter.ToString(remainingData).Replace("-", "")}");
        }

        protected long ReadMatrix(string name, BinaryReader reader, long bytesLeft, Func<BinaryReader, int, int, int, long, long> func)
        {
            UInt32 matrixSize = reader.ReadUInt32();
            byte[] matrix = reader.ReadBytes((int)matrixSize);
            bytesLeft -= 4;
            Console.WriteLine($"{name} Matrix: {BitConverter.ToString(matrix).Replace("-", "")}");
            if (func == null)
            {
                return bytesLeft;
            }

            bool[] m = matrix.ToBoolArray();
            for (int z = 0; z < Depth; z++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (m[z * Width * Height + y * Width + x])
                        {
                            bytesLeft = func(reader, x, y, z, bytesLeft);
                        }
                    }
                }
            }
            return bytesLeft;
        }
    }
}
