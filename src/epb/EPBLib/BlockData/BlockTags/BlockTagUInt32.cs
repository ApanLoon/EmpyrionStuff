using System;

namespace EPBLib
{
    public class BlockTagUInt32 : BlockTag
    {
        public BlockTagUInt32(string name)
        {
            BlockTagType = TagType.UInt32;
            Name = name;
        }
        public BlockTagUInt32(string name, UInt32 value)
        {
            BlockTagType = TagType.UInt32;
            Name = name;
            Value = value;
        }

        public UInt32 Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value=0x{Value:x8}({Value})";
        }
    }
}
