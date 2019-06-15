
namespace EPBLib
{
    public class MetaTagString : MetaTag
    {
        public MetaTagString(MetaTagKey key)
        {
            TagType = MetaTagType.String;
            Key = key;
        }

        public string Value { get; set; }

        public override string ValueToString()
        {
            return $"{Value}";
        }

        public override string ToString()
        {
            return $"{Key,-14}: \"{Value}\"";
        }
    }
}
