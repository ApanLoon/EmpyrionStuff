namespace EPBLib
{
    public class EpMetaTag03 : EpMetaTag
    {
        public EpMetaTag03(EpMetaTagKey key)
        {
            TagType = EpMetaTagType.Unknownx03;
            Key = key;
        }

        public byte[] Value { get; set; }
    }
}
