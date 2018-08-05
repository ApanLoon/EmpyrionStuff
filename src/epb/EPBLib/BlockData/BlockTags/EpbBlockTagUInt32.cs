using System;

namespace EPBLib
{
    public class EpbBlockTagUInt32 : EpbBlockTag
    {
        public EpbBlockTagUInt32(string name)
        {
            BlockTagType = TagType.UInt32;
            Name = name;
        }

        public UInt32 Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value=0x{Value:x8}({Value})";
        }
    }
}
