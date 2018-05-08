
namespace EPBLib
{
    public class EpMetaTagString : EpMetaTag
    {
        public EpMetaTagString(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.String;
            Key = key;
        }

        public string Value { get; set; }
        public override string ToString()
        {
            return $"{Key,-14}: \"{Value}\"";
        }
    }
}
