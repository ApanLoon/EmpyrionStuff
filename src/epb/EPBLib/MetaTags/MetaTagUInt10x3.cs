using System;

namespace EPBLib.MetaTags
{
    public class MetaTagUInt10x3 : MetaTagUInt32
    {
        public MetaTagUInt10x3(MetaTagKey key) : base(key)
        {
        }

        //TODO: Do the top two bits of Value have to be set to 1?

        public UInt16 X
        {
            get => (UInt16)((Value >> 20) & 0x3ff);
            set => Value = (UInt32)(((UInt32)(value & 0x3ff) << 20) + (Value & ~(0x3ff << 20)));
        }
        public UInt16 Y
        {
            get => (UInt16)((Value >> 10) & 0x3ff);
            set => Value = (UInt32)(((UInt32)(value & 0x3ff) << 10) + (Value & ~(0x3ff << 10)));
        }
        public UInt16 Z
        {
            get => (UInt16)(Value & 0x3ff);
            set => Value = (UInt32)((value & 0x3ff) + (Value & ~0x3ff));
        }

        public override string ValueToString()
        {
            return $"<{X}, {Y}, {Z}>";
        }

        public override string ToString()
        {
            return $"{Key,-14}: Type={TagType}(UInt10x3) Value={ValueToString()} Unknown=0x{Unknown:x2}";
        }

}
}
