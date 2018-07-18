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
            return $"{Key,-14}: {BitConverter.ToInt32(Value, 0)} {Value[4]}";
//            return $"{base.ToString()} Value={BitConverter.ToString(Value).Replace("-", "")}";
        }
    }
}
