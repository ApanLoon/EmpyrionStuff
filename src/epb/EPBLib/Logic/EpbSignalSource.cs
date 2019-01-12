
using System;
using System.Collections.Generic;

namespace EPBLib.Logic
{
    public class EpbSignalSource
    {
        public byte Unknown01;
        public Dictionary<string, EpbBlockTag> Tags = new Dictionary<string, EpbBlockTag>();

        public string Name => Tags.ContainsKey("Name") && Tags["Name"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["Name"]).Value : "";
        public EpbBlockPos Pos => Tags.ContainsKey("Pos") && Tags["Pos"].BlockTagType == EpbBlockTag.TagType.UInt32 ? ((EpbBlockTagPos)Tags["Pos"]).Value : null;
        public UInt32 State => Tags.ContainsKey("State") && Tags["State"].BlockTagType == EpbBlockTag.TagType.UInt32 ? ((EpbBlockTagUInt32)Tags["State"]).Value : 0;

        public bool IsActive => Tags.ContainsKey("State") && Tags["State"].BlockTagType == EpbBlockTag.TagType.UInt32 && (((EpbBlockTagUInt32)Tags["State"]).Value & 0x00020000) != 0;
        public bool IsOn => Tags.ContainsKey("State") && Tags["State"].BlockTagType == EpbBlockTag.TagType.UInt32 && (((EpbBlockTagUInt32)Tags["State"]).Value & 0x00000001) != 0;

    }
}
