
using System;

namespace EPBLib.BlockData
{
    public struct Colour
    {
        public byte R;
        public byte G;
        public byte B;

        public Colour(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Colour(UInt32 c)
        {
            B = (byte)(c & 0xff);
            c >>= 8;
            G = (byte)(c & 0xff);
            c >>= 8;
            R = (byte)(c & 0xff);
        }
    }

    public class Palette
    {
        protected Colour[] Colours;

        public Colour this[ColourIndex index]
        {
            get => Colours[(int)index];
            set => Colours[(int)index] = value;
        }
        public Colour this[int index]
        {
            get => Colours[index];
            set => Colours[index] = value;
        }

        public int Length => Colours.Length;

        /// <summary>
        /// Creates a default palette.
        /// </summary>
        public Palette()
        {
            Colours = new Colour[]
            {
                new Colour(0xffffff),
                new Colour(0xffffa6),
                new Colour(0xff0000),
                new Colour(0xffa6e9),
                new Colour(0x0040ff),
                new Colour(0x00ffff),
                new Colour(0x00ff99),
                new Colour(0xbb875a),
                new Colour(0xdcdcdc),
                new Colour(0xfff700),
                new Colour(0xaa0505),
                new Colour(0xff0096),
                new Colour(0x1a3259),
                new Colour(0x3aa6d7),
                new Colour(0x66ff00),
                new Colour(0x7b492e),
                new Colour(0xaaaaaa),
                new Colour(0xffaa03),
                new Colour(0xb0223a),
                new Colour(0xab20a1),
                new Colour(0x000f64),
                new Colour(0x3973d7),
                new Colour(0x1c9524),
                new Colour(0x3e1e0a),
                new Colour(0x6e6e6e),
                new Colour(0xff5f03),
                new Colour(0x660000),
                new Colour(0x6021ad),
                new Colour(0x3a183b),
                new Colour(0x2722b2),
                new Colour(0x002800),
                new Colour(0x0a0a0a)
            };
        }

        /// <summary>
        /// Create a blank palette with n colours.
        /// </summary>
        /// <param name="n"></param>
        public Palette(UInt32 n)
        {
            Colours = new Colour[n];
        }
    }

    public enum ColourIndex
    {
        None            = 0x00,
        LightYellow     = 0x01,
        Red             = 0x02,
        Pink            = 0x03,
        Blue            = 0x04,
        Cyan            = 0x05,
        LightGreen      = 0x06,
        LightBrown      = 0x07,
        LightGrey       = 0x08,
        Yellow          = 0x09,
        ChristmasRed    = 0x0a,
        FlourescentPink = 0x0b,
        DarkGunBlue     = 0x0c,
        SkyBlue         = 0x0d,
        BrightGreen     = 0x0e,
        Brown           = 0x0f,
        Grey            = 0x10,
        Gold            = 0x11,
        Cerise          = 0x12,
        Purple          = 0x13,
        ImperialBlue    = 0x14,
        LightNavyBlue   = 0x15,
        ChristmasGreen  = 0x16,
        DarkBrown       = 0x17,
        DarkGrey        = 0x18,
        Orange          = 0x19,
        DarkRed         = 0x1a,
        Violet          = 0x1b,
        DarkViolet      = 0x1c,
        EgyptianBlue    = 0x1d,
        DarkGreen       = 0x1e,
        Black           = 0x1f
    }
    /* Closest colour name according to https://www.htmlcsscolor.com 
    public enum ColourIndex
    {
        None = 0,
        Shalimar,          //#ffffa6
        Red,               //#ff0000
        LavenderRose,      //#ffa6e9
        NavyBlue,          //#0040ff
        Aqua,              //#00ffff
        MediumSpringGreen, //#00ff99
        Twine,             //#bb875a
        Gainsboro,         //#dcdcdc
        Yellow,            //#fff700
        FreeSpeechRed,     //#aa0505
        HollywoodCerise,   //#ff0096
        CatalinaBlue,      //#1a3259
        SummerSky,         //#3aa6d7
        BrightGreen,       //#66ff00
        CapePalliser,      //#7b492e
        DarkGray,          //#aaaaaa
        Orange,            //#ffaa03
        Cardinal,          //#b0223a
        MediumRedViolet,   //#ab20a1
        Sapphire,          //#000f64
        RoyalBlue,         //#3973d7
        ForestGreen,       //#1c9524
        BrownPod,          //#3e1e0a
        DimGray,           //#6e6e6e
        SafetyOrange,      //#ff5f03
        Maroon,            //#660000
        PurpleHeart,       //#6021ad
        MardiGras,         //#3a183b
        PersianBlue,       //#2722b2
        Myrtle,            //#002800
        Black              //#0a0a0a
    }
     */
}
