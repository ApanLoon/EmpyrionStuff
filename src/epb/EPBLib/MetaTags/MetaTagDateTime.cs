using System;

namespace EPBLib.MetaTags
{
    public class MetaTagDateTime : MetaTag
    {
        public MetaTagDateTime(MetaTagKey key)
        {
            TagType = MetaTagType.DateTime;
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
