using System.Collections.Generic;
using System.Linq;

namespace EPBLib.Helpers
{
    public static class ByteArrayExtensions
    {
        public static int IndexOf(this byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool[] ToBoolArray(this byte[] buf)
        {
            bool[] result = new bool[8 * buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                byte b = buf[i];
                uint bit = 1;
                for (int j = 0; j < 8; j++)
                {
                    result[i * 8 + j] = (b & bit) != 0;
                    bit *= 2;
                }
            }
            return result;
        }
    }
}
