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
