
using System.Windows.Media;
using EPBLib.BlockData;

namespace EPBLab.Helpers
{
    public static class EpbColourExtensions
    {
        public static Color ToColor(this EpbColour c)
        {
            return Color.FromRgb(c.R, c.G, c.B);
        }
    }
}
