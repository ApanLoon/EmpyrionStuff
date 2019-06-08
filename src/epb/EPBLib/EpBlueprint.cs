
using EPBLib.BlockData;
using EPBLib.Helpers;
using EPBLib.Logic;
using System;
using System.Collections.Generic;

namespace EPBLib
{
    public class EpBlueprint
    {
        #region Types
        public enum EpbType
        {
            Voxel         = 0x00,
            Base          = 0x02,
            SmallVessel   = 0x04,
            CapitalVessel = 0x08,
            HoverVessel   = 0x10
        }
        #endregion Types

        #region Properties
        public UInt32 Version { get; set; }
        public EpbType Type { get; set; }
        public UInt32 Width { get; set; }
        public UInt32 Height { get; set; }
        public UInt32 Depth { get; set; }
        public UInt16 Unknown01 { get; set; }

        public Dictionary<EpMetaTagKey, EpMetaTag> MetaTags = new Dictionary<EpMetaTagKey, EpMetaTag>();

        public UInt16 Unknown02 { get; set; }
        public UInt32 LightCount { get; set; }
        public UInt32 UnknownCount01 { get; set; }
        public UInt32 DeviceCount { get; set; }
        public UInt32 UnknownCount02 { get; set; }
        public UInt32 UnknownCount03 { get; set; }
        public UInt32 TriangleCount { get; set; }

        public Dictionary<EpbBlockType, UInt32> BlockCounts = new Dictionary<EpbBlockType, uint>();

        public List<EpbDeviceGroup> DeviceGroups = new List<EpbDeviceGroup>();

        EpbBlockList _blocks;
        public EpbBlockList Blocks => _blocks ?? (_blocks = new EpbBlockList());

        public byte[] Unknown07 = new byte[0];
        public List<EpbSignalSource> SignalSources = new List<EpbSignalSource>();
        public List<EpbSignalTarget> SignalTargets = new List<EpbSignalTarget>();
        public List<EpbSignalOperator> SignalOperators = new List<EpbSignalOperator>();
        public List<string> CustomNames = new List<string>();
        public Dictionary<string, byte[]> Unknown08 = new Dictionary<string, byte[]>();

        public EpbPalette Palette = new EpbPalette();
        #endregion Properties

        public EpBlueprint (EpbType type = EpbType.Base, UInt32 width = 0, UInt32 height = 0, UInt32 depth = 0)
        {
            Version        = 20;
            Type           = type;
            Width          = width;
            Height         = height;
            Depth          = depth;
            Unknown01      = 1;
            Unknown02      = 0;
            LightCount     = 0;
            UnknownCount01 = 0;
            DeviceCount    = 0;
            UnknownCount02 = 0;
            UnknownCount03 = 0;
            TriangleCount  = 0;

            MetaTags[EpMetaTagKey.UnknownMetax11] = new EpMetaTag03(EpMetaTagKey.UnknownMetax11)     { Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }};
            MetaTags[EpMetaTagKey.UnknownMetax01] = new EpMetaTagUInt16(EpMetaTagKey.UnknownMetax01) { Value = 0x0000};
            MetaTags[EpMetaTagKey.UnknownMetax0E] = new EpMetaTag03(EpMetaTagKey.UnknownMetax0E)     { Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }};
            MetaTags[EpMetaTagKey.UnknownMetax0F] = new EpMetaTag03(EpMetaTagKey.UnknownMetax0F)     { Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }};
            MetaTags[EpMetaTagKey.UnknownMetax05] = new EpMetaTagUInt16(EpMetaTagKey.UnknownMetax05) { Value = 0x0000 };
            MetaTags[EpMetaTagKey.UnknownMetax04] = new EpMetaTag02(EpMetaTagKey.UnknownMetax04)     { Value = 0, Unknown = 0 };
            MetaTags[EpMetaTagKey.UnknownMetax06] = new EpMetaTag04(EpMetaTagKey.UnknownMetax06)     { Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }};
            MetaTags[EpMetaTagKey.BlueprintName]  = new EpMetaTagString(EpMetaTagKey.BlueprintName)  { Value = "" };
            MetaTags[EpMetaTagKey.CreationTime]   = new EpMetaTag05(EpMetaTagKey.CreationTime)       { Value = DateTime.Now, Unknown = 0 };
            MetaTags[EpMetaTagKey.BuildVersion]   = new EpMetaTag02(EpMetaTagKey.BuildVersion)       { Value = 1838, Unknown = 0 };
            MetaTags[EpMetaTagKey.CreatorId]      = new EpMetaTagString(EpMetaTagKey.CreatorId)      { Value = "" };
            MetaTags[EpMetaTagKey.CreatorName]    = new EpMetaTagString(EpMetaTagKey.CreatorName)    { Value = "" };
            MetaTags[EpMetaTagKey.OwnerId]        = new EpMetaTagString(EpMetaTagKey.OwnerId)        { Value = "" };
            MetaTags[EpMetaTagKey.OwnerName]      = new EpMetaTagString(EpMetaTagKey.OwnerName)      { Value = "" };
            MetaTags[EpMetaTagKey.DisplayName]    = new EpMetaTagString(EpMetaTagKey.DisplayName)    { Value = "" };
            MetaTags[EpMetaTagKey.UnknownMetax12] = new EpMetaTag05(EpMetaTagKey.UnknownMetax12)     { Value = DateTime.MinValue, Unknown = 0 };
        }

        public void SetBlock(EpbBlock block)
        {
            // TODO: Update blockCounts
            Blocks[block.Position] = block;
        }

        public void ComputeDimensions()
        {
            UInt32 width  = 0;
            UInt32 height = 0;
            UInt32 depth  = 0;
            foreach (EpbBlock block in Blocks)
            {
                width  = Math.Max(width,  block.Position.X + 1u);
                height = Math.Max(height, block.Position.Y + 1u);
                depth  = Math.Max(depth,  block.Position.Z + 1u);
            }
            Width  = width;
            Height = height;
            Depth = depth;
        }

        public void CountBlocks()
        {
            Dictionary<EpbBlockType, UInt32> blockCounts = new Dictionary<EpbBlockType, uint>();
            foreach (EpbBlock block in Blocks)
            {
                UInt16 id = block.BlockType.CountAs;
                EpbBlockType t = EpbBlockType.BlockTypes[id];
                if (!blockCounts.ContainsKey(t))
                {
                    blockCounts.Add(t, 0);
                }
                blockCounts[t]++;
            }
            BlockCounts = blockCounts;
        }
    }
}
