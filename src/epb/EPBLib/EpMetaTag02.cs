using System;

namespace EPBLib
{
    public class EpMetaTag02 : EpMetaTag
    {
        public EpMetaTag02(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.Unknownx02;
            Key = key;
        }

        public byte[] Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value={BitConverter.ToString(Value).Replace("-", "")}";
        }
    }
}
