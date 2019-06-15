using System;

namespace EPBLib
{
    public class MetaTag02 : MetaTag
    {
        public MetaTag02(MetaTagKey key)
        {
            TagType = MetaTagType.Unknownx02;
            Key = key;
        }

        public UInt32 Value { get; set; }
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
