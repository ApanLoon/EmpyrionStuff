using System;

namespace EPBLib
{
    public class BlockTagBool : BlockTag
    {
        public BlockTagBool(string name)
        {
            BlockTagType = TagType.Bool;
            Name = name;
        }
        public BlockTagBool(string name, bool value)
        {
            BlockTagType = TagType.Bool;
            Name = name;
            Value = value;
        }

        public bool Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value={Value}";
        }
    }
}
