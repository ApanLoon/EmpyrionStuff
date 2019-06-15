using System;
using EPBLib.Helpers;

namespace EPBLib
{
    public class BlockTagFloat : BlockTag
    {
        public BlockTagFloat(string name)
        {
            BlockTagType = TagType.Float;
            Name = name;
        }
        public BlockTagFloat(string name, float value)
        {
            BlockTagType = TagType.Float;
            Name = name;
            Value = value;
        }

        public float Value { get; set; }
        public override string ToString()
        {
            return $"{base.ToString()} Value={Value}";
        }
    }
}
