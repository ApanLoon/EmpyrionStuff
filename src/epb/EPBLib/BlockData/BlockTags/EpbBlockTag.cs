
namespace EPBLib
{
    public class EpbBlockTag
    {
        #region Types
        public enum TagType
        {
            UInt32 = 0x00,
            String = 0x01,
            Bool   = 0x02,
            x03    = 0x03,
            Colour = 0x05
        }
        #endregion Types

        public TagType BlockTagType { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name,-14}: Type={BlockTagType}";
        }
    }
}
