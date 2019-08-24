
using EPBLib.BlockData;
using EPBLib.Helpers;
using EPBLib.Logic;
using System;
using System.Collections.Generic;

namespace EPBLib
{
    public enum BlueprintType
    {
        Voxel = 0x00,
        Base = 0x02,
        SmallVessel = 0x04,
        CapitalVessel = 0x08,
        HoverVessel = 0x10
    }

    public class Blueprint
    {
        #region Properties
        public UInt32 Version { get; set; }
        public BlueprintType Type { get; set; }
        public UInt32 Width { get; set; }
        public UInt32 Height { get; set; }
        public UInt32 Depth { get; set; }
        public UInt16 Unknown01 { get; set; }

        public Dictionary<MetaTagKey, MetaTag> MetaTags = new Dictionary<MetaTagKey, MetaTag>();

        public UInt16 Unknown02 { get; set; }
        public UInt32 LightCount { get; set; }
        public UInt32 DoorCount { get; set; }
        public UInt32 DeviceCount { get; set; }
        public UInt32 UnknownCount02 { get; set; }
        public UInt32 SolidCount { get; set; }
        public UInt32 UnknownCount03 { get; set; }
        public UInt32 TriangleCount { get; set; }

        public Dictionary<BlockType, UInt32> BlockCounts = new Dictionary<BlockType, uint>();

        public List<DeviceGroup> DeviceGroups = new List<DeviceGroup>();

        BlockList _blocks;
        public BlockList Blocks => _blocks ?? (_blocks = new BlockList());

        public List<SignalSource> SignalSources = new List<SignalSource>();
        public List<SignalTarget> SignalTargets = new List<SignalTarget>();
        public List<SignalOperator> SignalOperators = new List<SignalOperator>();
        public List<string> CustomNames = new List<string>();
        public Dictionary<string, byte[]> Unknown08 = new Dictionary<string, byte[]>();

        public Palette Palette = new Palette();
        #endregion Properties

        public Blueprint (BlueprintType type = BlueprintType.Base, UInt32 width = 0, UInt32 height = 0, UInt32 depth = 0)
        {
            Version        = 20;
            Type           = type;
            Width          = width;
            Height         = height;
            Depth          = depth;
            Unknown01      = 1;
            Unknown02      = 0;
            LightCount     = 0;
            DoorCount = 0;
            DeviceCount    = 0;
            UnknownCount02 = 0;
            UnknownCount03 = 0;
            TriangleCount  = 0;

            MetaTags[MetaTagKey.GroundOffset]   = new MetaTag03     (MetaTagKey.GroundOffset)   { Value = 0f, Unknown = 0 };
            MetaTags[MetaTagKey.TerrainRemoval] = new MetaTagUInt16 (MetaTagKey.TerrainRemoval) { Value = 0x0000};
            MetaTags[MetaTagKey.UnknownMetax0E] = new MetaTag03     (MetaTagKey.UnknownMetax0E) { Value = 0f, Unknown = 0 };
            MetaTags[MetaTagKey.UnknownMetax0F] = new MetaTag03     (MetaTagKey.UnknownMetax0F) { Value = 0f, Unknown = 0 };
            MetaTags[MetaTagKey.UnknownMetax05] = new MetaTagUInt16 (MetaTagKey.UnknownMetax05) { Value = 0x0000 };
            MetaTags[MetaTagKey.UnknownMetax04] = new MetaTag02     (MetaTagKey.UnknownMetax04) { Value = 0, Unknown = 0 };
            MetaTags[MetaTagKey.UnknownMetax06] = new MetaTag04     (MetaTagKey.UnknownMetax06) { Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }};
            MetaTags[MetaTagKey.GroupName]      = new MetaTagString (MetaTagKey.GroupName)      { Value = "" };
            MetaTags[MetaTagKey.ChangedTime]   = new MetaTag05     (MetaTagKey.ChangedTime)   { Value = DateTime.Now, Unknown = 0 };
            MetaTags[MetaTagKey.BuildVersion]   = new MetaTag02     (MetaTagKey.BuildVersion)   { Value = 1838, Unknown = 0 };
            MetaTags[MetaTagKey.CreatorId]      = new MetaTagString (MetaTagKey.CreatorId)      { Value = "" };
            MetaTags[MetaTagKey.CreatorName]    = new MetaTagString (MetaTagKey.CreatorName)    { Value = "" };
            MetaTags[MetaTagKey.ChangedById]        = new MetaTagString (MetaTagKey.ChangedById)        { Value = "" };
            MetaTags[MetaTagKey.ChangedByName]      = new MetaTagString (MetaTagKey.ChangedByName)      { Value = "" };
            MetaTags[MetaTagKey.SpawnName]      = new MetaTagString (MetaTagKey.SpawnName)      { Value = "" };
            MetaTags[MetaTagKey.UnknownMetax12] = new MetaTag05     (MetaTagKey.UnknownMetax12) { Value = DateTime.MinValue, Unknown = 0 };
        }

        public void SetBlock(Block block)
        {
            // TODO: Update blockCounts
            Blocks[block.Position] = block;
        }
        public void SetBlock(BlockList blocks)
        {
            foreach (Block block in blocks)
            {
                Blocks[block.Position] = block;
            }           
        }

        public void ComputeDimensions()
        {
            UInt32 width  = 0;
            UInt32 height = 0;
            UInt32 depth  = 0;
            foreach (Block block in Blocks)
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
            Dictionary<BlockType, UInt32> blockCounts = new Dictionary<BlockType, uint>();
            foreach (Block block in Blocks)
            {
                UInt16 id = block.BlockType.CountAs;
                BlockType t = BlockType.BlockTypes[id];
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
