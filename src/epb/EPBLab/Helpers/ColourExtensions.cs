
using System.Windows.Media;
using EPBLib.BlockData;

namespace EPBLab.Helpers
{
    public static class ColourExtensions
    {
        public static Color ToColor(this Colour c)
        {
            return Color.FromRgb(c.R, c.G, c.B);
        }
    }
}
