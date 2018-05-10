using System;

namespace EPBLib
{
    public class EpbBlockTagColour : EpbBlockTag
    {
        public EpbBlockTagColour(string name)
        {
            BlockTagType = TagType.Colour;
            Name = name;
        }

        public UInt32 Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value={Value}";
        }
    }
}
