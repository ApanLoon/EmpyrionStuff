using System;

namespace EPBLib
{
    public class EpbBlockTagBool : EpbBlockTag
    {
        public EpbBlockTagBool(string name)
        {
            BlockTagType = TagType.Bool;
            Name = name;
        }
        public EpbBlockTagBool(string name, bool value)
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
