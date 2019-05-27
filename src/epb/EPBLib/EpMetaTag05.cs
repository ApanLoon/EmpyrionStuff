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

        public DateTime Value { get; set; }
        public byte Unknown { get; set; }

        public override string ValueToString()
        {
            return $"{Value} (0x{Unknown:x2})";
        }

        public override string ToString()
        {
            return $"{base.ToString()} Value={Value} Unknown=0x{Unknown:x2}";
        }
    }
}
