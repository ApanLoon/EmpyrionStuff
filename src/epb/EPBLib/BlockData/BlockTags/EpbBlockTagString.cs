using System;

namespace EPBLib
{
    public class EpbBlockTagString : EpbBlockTag
    {
        public EpbBlockTagString(string name)
        {
            BlockTagType = TagType.String;
            Name = name;
        }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value=\"{Value}\"";
        }
    }
}
