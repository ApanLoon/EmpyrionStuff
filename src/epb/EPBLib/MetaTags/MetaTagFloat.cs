namespace EPBLib.MetaTags
{
    public class MetaTagFloat : MetaTag
    {
        public MetaTagFloat(MetaTagKey key)
        {
            TagType = MetaTagType.Float;
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
