
using System;
using System.Collections.Generic;

namespace EPBLib.Logic
{
    public class EpbSignalTarget
    {
        public enum Behaviour
        {
            Follow = 1,
            Toggle,
            On,
            Off
        }

        public string SignalName { get; set; }
        public byte Unknown01 { get; set; }
        public Dictionary<string, EpbBlockTag> Tags = new Dictionary<string, EpbBlockTag>();

        public EpbBlockPos Pos => Tags.ContainsKey("Pos") && Tags["Pos"].BlockTagType == EpbBlockTag.TagType.UInt32 ? ((EpbBlockTagPos)Tags["Pos"]).Value : null;
        public UInt32 Func => Tags.ContainsKey("Func") && Tags["Func"].BlockTagType == EpbBlockTag.TagType.UInt32 ? ((EpbBlockTagUInt32)Tags["Func"]).Value : 0;
        public Behaviour Beh => Tags.ContainsKey("Beh") && Tags["Beh"].BlockTagType == EpbBlockTag.TagType.UInt32 ? (Behaviour)((EpbBlockTagUInt32)Tags["Beh"]).Value : Behaviour.Follow;
        public bool Inv => Tags.ContainsKey("Inv") && Tags["Inv"].BlockTagType == EpbBlockTag.TagType.Bool && ((EpbBlockTagBool)Tags["Inv"]).Value;

    }
}
