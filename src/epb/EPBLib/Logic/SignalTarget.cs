
using System;
using System.Collections.Generic;

namespace EPBLib.Logic
{
    public class SignalTarget
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
        public Dictionary<string, BlockTag> Tags = new Dictionary<string, BlockTag>();

        public BlockPos Pos => Tags.ContainsKey("Pos") && Tags["Pos"].BlockTagType == BlockTag.TagType.UInt32 ? ((BlockTagPos)Tags["Pos"]).Value : null;
        public UInt32 Func => Tags.ContainsKey("Func") && Tags["Func"].BlockTagType == BlockTag.TagType.UInt32 ? ((BlockTagUInt32)Tags["Func"]).Value : 0;
        public Behaviour Beh => Tags.ContainsKey("Beh") && Tags["Beh"].BlockTagType == BlockTag.TagType.UInt32 ? (Behaviour)((BlockTagUInt32)Tags["Beh"]).Value : Behaviour.Follow;
        public bool Inv => Tags.ContainsKey("Inv") && Tags["Inv"].BlockTagType == BlockTag.TagType.Bool && ((BlockTagBool)Tags["Inv"]).Value;

    }
}
