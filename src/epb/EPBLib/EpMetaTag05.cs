namespace EPBLib
{
    public class EpMetaTag05 : EpMetaTag
    {
        public EpMetaTag05(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.Unknownx05;
            Key = key;
        }

        public byte[] Value { get; set; }
    }
}
