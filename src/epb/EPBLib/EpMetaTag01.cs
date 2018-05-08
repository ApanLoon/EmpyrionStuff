using System;

namespace EPBLib.Helpers
{
    public class EpMetaTag01 : EpMetaTag
    {
        public EpMetaTag01(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.Unknownx01;
            Key = key;
        }

        public UInt16 Value { get; set; }
    }
}
