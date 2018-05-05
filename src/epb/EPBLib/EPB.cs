
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
            BA = 0x02,
            SV = 0x04,
            CV = 0x08,
            HV = 0x10
        }
        #endregion Types

        protected static readonly byte[] DataStartPattern = new byte[] {0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08, 0x00};
        protected static readonly byte[] PK = new byte[] { 0x50, 0x4b };

        #region Fields
        public UInt32 Version;
        public BluePrintType Type;
        public UInt32 Width;
        public UInt32 Height;
        public UInt32 Depth;
        public Byte[] Metadata;
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

            // TODO: This is funky stuff, should figure out the proper format instead:
            byte[] buf = reader.ReadBytes((int)bytesLeft);
            int dataStart = buf.IndexOf(DataStartPattern);
            if (dataStart == -1)
            {
                throw new Exception("ReadHeader: Unable to locate dataStart.");
            }
            Metadata = buf.Take(dataStart).ToArray();
            bytesLeft -= dataStart;
//            Console.WriteLine(BitConverter.ToString(Metadata).Replace("-", ""));

            byte[] zippedData = PK.Concat(buf.Skip(dataStart).Take((int)bytesLeft)).ToArray();

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

            int unknown7Count = 0;
            bytesLeft = ReadMatrix("Unknown7", reader, length, (r, x, y, z, b) =>
            {
                byte unknown71 = r.ReadByte();
                byte unknown72 = r.ReadByte();
                byte unknown73 = r.ReadByte();
                byte unknown74 = r.ReadByte();
                unknown7Count++;
                Console.WriteLine($"    {unknown7Count} ({x}, {y}, {z}): 0x{unknown71:x2} 0x{unknown72:x2} 0x{unknown73:x2} 0x{unknown74:x2}");
                return b - 4;
            });

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
