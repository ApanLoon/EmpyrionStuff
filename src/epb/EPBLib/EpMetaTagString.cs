
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
    }
}
