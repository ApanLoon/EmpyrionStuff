
using EPBLib.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EPBLib.BlockData;
using EPBLib.Logic;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

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
        public UInt32 Version { get; protected set; }
        public EpbType Type { get; set; }
        public UInt32 Width { get; protected set; }
        public UInt32 Height { get; protected set; }
        public UInt32 Depth { get; protected set; }

        public Dictionary<EpMetaTagKey, EpMetaTag> MetaTags;

        public List<EpbDeviceGroup> DeviceGroups;

        //public EpbBlock[,,] Blocks { get; set; }
        public EpbBlockList Blocks { get; set; }

        public EpbPalette Palette = new EpbPalette();

        public List<EpbSignalSource> SignalSources = new List<EpbSignalSource>();
        public List<EpbSignalTarget> SignalTargets = new List<EpbSignalTarget>();
        public List<EpbSignalOperator> SignalOperators = new List<EpbSignalOperator>();

        #endregion Properties

        public EpBlueprint (EpbType type = EpbType.Base, UInt32 width = 0, UInt32 height = 0, UInt32 depth = 0)
        {
            Version      = 20;
            Type         = type;
            Width        = width;
            Height       = height;
            Depth        = depth;
            MetaTags     = new Dictionary<EpMetaTagKey, EpMetaTag>();
            DeviceGroups = new List<EpbDeviceGroup>();
        }

        public void SetBlock(EpbBlock block)
        {
            if (Blocks == null)
            {
                Blocks = new EpbBlockList();
            }

            // TODO: Update blockCounts
            Blocks[block.Position] = block;
        }
    }
}
