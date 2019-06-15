using System;

namespace EPBLib
{
    public class MetaTag04 : MetaTag
    {
        public MetaTag04(MetaTagKey key)
        {
            TagType = MetaTagType.Unknownx04;
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
