using System;

namespace EPBLib
{
    public class MetaTag03 : MetaTag
    {
        public MetaTag03(MetaTagKey key)
        {
            TagType = MetaTagType.Unknownx03;
            Key = key;
        }

        public float Value { get; set; }
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
