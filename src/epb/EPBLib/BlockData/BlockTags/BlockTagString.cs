using System;

namespace EPBLib
{
    public class BlockTagString : BlockTag
    {
        public BlockTagString(string name)
        {
            BlockTagType = TagType.String;
            Name = name;
        }
        public BlockTagString(string name, string value)
        {
            BlockTagType = TagType.String;
            Name = name;
            Value = value;
        }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value=\"{Value}\"";
        }
    }
}
