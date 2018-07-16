using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static string ToHexString(this byte[] buf)
        {
            return BitConverter.ToString(buf).Replace("-", "");
        }
        public static string ToHexDump(this byte[] buf)
        {
            string s = "";
            int start = 0;
            int n = buf.Length;
            while (start < n)
            {
                int len = Math.Min(16, n - start);
                string bytes = BitConverter.ToString(buf, start, len).Replace("-", " ");
                string ascii = "";
                for (int c = 0; c < len; c++)
                {
                    if (buf[start + c] < 32 || buf[start + c] >= 0x7f)
                    {
                        ascii += ".";
                    }
                    else
                    {
                        ascii += Encoding.ASCII.GetString(buf, start + c, 1);
                    }
                }
                s += $"{bytes,-48} {ascii,-16}\n\r";
                start += 16;
            }
            return s;
        }
    }
}
