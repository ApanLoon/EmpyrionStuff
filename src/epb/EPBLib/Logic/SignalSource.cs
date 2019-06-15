
using System;
using System.Collections.Generic;

namespace EPBLib.Logic
{
    public class SignalSource
    {
        public byte Unknown01;
        public Dictionary<string, BlockTag> Tags = new Dictionary<string, BlockTag>();

        public string Name
        {
            get => Tags.ContainsKey("Name") && Tags["Name"].BlockTagType == BlockTag.TagType.String
                ? ((BlockTagString) Tags["Name"]).Value
                : "";
            set => Tags["Name"] = new BlockTagString("Name", value);
        }

        public BlockPos Pos
        {
            get => Tags.ContainsKey("Pos") && Tags["Pos"].BlockTagType == BlockTag.TagType.UInt32
                ? ((BlockTagPos) Tags["Pos"]).Value
                : null;
            set => Tags["Pos"] = new BlockTagPos(value);
        }

        public UInt32 State
        {
            get => Tags.ContainsKey("State") && Tags["State"].BlockTagType == BlockTag.TagType.UInt32
                ? ((BlockTagUInt32) Tags["State"]).Value
                : 0;
            set => Tags["State"] = new BlockTagUInt32("State", value);
        }

        public bool IsActive => Tags.ContainsKey("State") && Tags["State"].BlockTagType == BlockTag.TagType.UInt32 && (((BlockTagUInt32)Tags["State"]).Value & 0x00020000) != 0;
        public bool IsOn => Tags.ContainsKey("State") && Tags["State"].BlockTagType == BlockTag.TagType.UInt32 && (((BlockTagUInt32)Tags["State"]).Value & 0x00000001) != 0;

    }
}
