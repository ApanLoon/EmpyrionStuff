using System;

namespace EPBLib
{
    public class EpMetaTag05 : EpMetaTag
    {
        public EpMetaTag05(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.Unknownx05;
            Key = key;
        }

        public byte[] Value { get; set; }

        public override string ToString()
        {
            System.DateTime dt = DateTime.FromBinary(BitConverter.ToInt64(Value, 0));
            return $"{base.ToString()} Value={BitConverter.ToString(Value).Replace("-", "")} {dt}";
        }
    }
}
