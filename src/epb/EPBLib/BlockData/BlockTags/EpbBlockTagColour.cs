using System;

namespace EPBLib
{
    public class EpbBlockTagColour : EpbBlockTag
    {
        public EpbBlockTagColour(string name)
        {
            BlockTagType = TagType.Colour;
            Name = name;
        }

        public UInt32 Value { get; set; }

        public byte Red
        {
            get => (byte)((Value >> 24) & 0xff);
            set => Value = (UInt32)((Value & 0xff000000) | ((UInt32)value << 24));
        }
        public byte Green
        {
            get => (byte)((Value >> 16) & 0xff);
            set => Value = (UInt32)((Value & 0x00ff0000) | ((UInt32)value << 16));
        }
        public byte Blue
        {
            get => (byte)((Value >> 8) & 0xff);
            set => Value = (UInt32)((Value & 0x0000ff00) | ((UInt32)value << 8));
        }
        public byte Alpha
        {
            get => (byte)((Value >> 0) & 0xff);
            set => Value = (UInt32)((Value & 0x000000ff) | ((UInt32)value << 0));
        }

        public override string ToString()
        {
            return $"{base.ToString()} Value=({Red}, {Green}, {Blue}, {Alpha})";
        }
    }
}
