using System;

namespace EPBLib
{
    public class EpMetaTag04 : EpMetaTag
    {
        public EpMetaTag04(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.Unknownx04;
            Key = key;
        }

        public byte[] Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value={BitConverter.ToString(Value).Replace("-", "")}";
        }
    }
}
