using System;

namespace EPBLib.Helpers
{
    public class EpMetaTagUInt16 : EpMetaTag
    {
        public EpMetaTagUInt16(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.UInt16;
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
