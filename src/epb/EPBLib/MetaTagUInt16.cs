using System;

namespace EPBLib.Helpers
{
    public class MetaTagUInt16 : MetaTag
    {
        public MetaTagUInt16(MetaTagKey key)
        {
            TagType = MetaTagType.UInt16;
            Key = key;
        }

        public UInt16 Value { get; set; }

        public override string ValueToString()
        {
            return $"{Value}";
        }

        public override string ToString()
        {
            return $"{Key,-14}: {Value}";
        }
    }
}
