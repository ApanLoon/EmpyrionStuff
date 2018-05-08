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
    }
}
