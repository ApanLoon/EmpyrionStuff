
using System;
using System.Collections.Generic;

namespace EPBLib
{
    public class EpbBlock
    {
        #region Types

        public enum EpbBlockType
        {
            CvCockpit     = 0x0101,
            CvFuelTankT1  = 0x0103,
            CvCore        = 0x022e,
            BaCore        = 0x0a2e,
            SteelBlockL_A = 0x0193, // Variants 0x00-0x1f
            SteelBlockL_B = 0x0194  // Variants 0x00-0x1e
        }

        public struct EpbBlockVariant
        {
            public string Name { get; set; }
            public byte Id { get; set; }
        }
        #endregion Types

        #region Variants
        public static readonly Dictionary<EpbBlockType,string[]> BlockVariants = new Dictionary<EpbBlockType, string[]>()
        {
            {
                EpbBlockType.SteelBlockL_A, new string[]
                {
                    "Cube", "Cut Corner", "Corner Long A", "Corner Long B", "Corner Long C",
                    "Corner Long D", "Corner Large A", "Corner", "Ramp Bottom", "Ramp Top",
                    "Slope", "Curved Corner", "Round Cut Corner", "Round Corner",
                    "Round Corner Long", "Round Slope", "Cylinder", "Inward Corner",
                    "Inward Round Slope", "Inward Curved Corner", "Round Slope Edge Inward",
                    "Cylinder End A", "Cylinder End B", "Cylinder End C", "Ramp Wedge Top",
                    "Round 4 Way Connector", "Round Slope Edge", "Corner Large B", "Corner Large C",
                    "Corner Large D", "Corner Long E", "Pyramid A"
                }
            },
            {
                EpbBlockType.SteelBlockL_B, new string[]
                {
                    "Wall", "Wall L-shape", "Thin Slope", "Thin Corner", "Sloped Wall",
                    "Sloped Wall Bottom (right)", "Sloped Wall Top (right)",
                    "Sloped Wall Bottom (left)", "Sloped Wall Top (left)", "Round Corner Thin",
                    "Round Slope Thin", "Round Cut Corner Thin", "Round Slope Edge Thin",
                    "Round Corner Long Thin", "Corner Round Thin 2", "Corner Thin 2",
                    "Wall 3 Corner", "Wall Half", "Cube Half", "Ramp Top Double", "Ramp Bottom A",
                    "Ramp Bottom B", "Ramp Bottom C", "Ramp Wedge Bottom", "Beam", "Cylinder Thin",
                    "Cylinder Thin T Joint", "Cylinder Thin Curved", "Cylinder Fence Bottom",
                    "Cylinder Fence Top", "Slope Half"
                }
            }
        };

        public static byte GetVariant(EpbBlockType type, string variantName)
        {
            if (!BlockVariants.ContainsKey(type))
            {
                return 0x00;
            }

            int i = Array.FindIndex(BlockVariants[type], s => s == variantName);
            if (i == -1)
            {
                return 0x00;
            }

            return (byte)i;
        }

        public static string GetVariantName(EpbBlockType type, byte variant)
        {
            if (!BlockVariants.ContainsKey(type) || variant >= BlockVariants[type].Length)
            {
                return $"{variant:x2}";
            }
            return BlockVariants[type][variant];
        }
        #endregion Variants

        public EpbBlockType BlockType { get; set; }
        public byte Rotation { get; set; }
        public UInt16 Unknown00 { get; set; }
        public byte Variant { get; set; }
        public string VariantName
        {
            get => GetVariantName(BlockType, Variant);
            set => Variant = GetVariant(BlockType, value);
        }

        public byte[] FaceColours = new byte[6]; // 5 bit colour index
        public byte[] FaceTextures = new byte[6]; // 6 bit texture index
        public byte SymbolPage { get; set; } // 2 bit page index
        public byte[] FaceSymbols = new byte[6]; // 5 bit symbol index


    }
}
