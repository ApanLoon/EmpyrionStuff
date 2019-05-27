using System;

namespace EPBLib
{
    public class EpMetaTag03 : EpMetaTag
    {
        public EpMetaTag03(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.Unknownx03;
            Key = key;
        }

        public byte[] Value { get; set; }

        public override string ValueToString()
        {
            return $"{BitConverter.ToString(Value).Replace("-", "")}";
        }

        public override string ToString()
        {
            return $"{base.ToString()} Value={BitConverter.ToString(Value).Replace("-", "")}";
        }
    }
}
