
using System;
using System.Collections.Generic;

namespace EPBLib.Logic
{
    public class EpbSignalSource
    {
        public byte Unknown01;
        public Dictionary<string, EpbBlockTag> Tags = new Dictionary<string, EpbBlockTag>();

        public string Name
        {
            get => Tags.ContainsKey("Name") && Tags["Name"].BlockTagType == EpbBlockTag.TagType.String
                ? ((EpbBlockTagString) Tags["Name"]).Value
                : "";
            set => Tags["Name"] = new EpbBlockTagString("Name", value);
        }

        public EpbBlockPos Pos
        {
            get => Tags.ContainsKey("Pos") && Tags["Pos"].BlockTagType == EpbBlockTag.TagType.UInt32
                ? ((EpbBlockTagPos) Tags["Pos"]).Value
                : null;
            set => Tags["Pos"] = new EpbBlockTagPos(value);
        }

        public UInt32 State
        {
            get => Tags.ContainsKey("State") && Tags["State"].BlockTagType == EpbBlockTag.TagType.UInt32
                ? ((EpbBlockTagUInt32) Tags["State"]).Value
                : 0;
            set => Tags["State"] = new EpbBlockTagUInt32("State", value);
        }

        public bool IsActive => Tags.ContainsKey("State") && Tags["State"].BlockTagType == EpbBlockTag.TagType.UInt32 && (((EpbBlockTagUInt32)Tags["State"]).Value & 0x00020000) != 0;
        public bool IsOn => Tags.ContainsKey("State") && Tags["State"].BlockTagType == EpbBlockTag.TagType.UInt32 && (((EpbBlockTagUInt32)Tags["State"]).Value & 0x00000001) != 0;

    }
}
