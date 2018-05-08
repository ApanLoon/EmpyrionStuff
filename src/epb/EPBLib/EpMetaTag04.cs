namespace EPBLib
{
    public class EpMetaTag04 : EpMetaTag
    {
        public EpMetaTag04(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.Unknownx04;
            Key = key;
        }

        public byte[] Value { get; set; }
    }
}
