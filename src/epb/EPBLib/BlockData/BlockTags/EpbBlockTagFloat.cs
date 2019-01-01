using System;
using EPBLib.Helpers;

namespace EPBLib
{
    public class EpbBlockTagFloat : EpbBlockTag
    {
        public EpbBlockTagFloat(string name)
        {
            BlockTagType = TagType.Float;
            Name = name;
        }

        public float Value { get; set; }
        public override string ToString()
        {
            return $"{base.ToString()} Value={Value}";
        }
    }
}
