using System;

namespace EPBLib.MetaTags
{
    public class MetaTagUInt32 : MetaTag
    {
        public MetaTagUInt32(MetaTagKey key)
        {
            TagType = MetaTagType.UInt32;
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
