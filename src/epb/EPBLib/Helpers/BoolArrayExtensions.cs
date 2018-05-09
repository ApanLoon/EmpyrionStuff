namespace EPBLib.Helpers
{
    public static class BoolArrayExtensions
    {
        public static byte[] ToByteArray(this bool[] bools)
        {
            // pack (in this case, using the first bool as the lsb - if you want
            // the first bool as the msb, reverse things ;-p)
            int nBytes = bools.Length / 8;
            if ((bools.Length % 8) != 0) nBytes++;
            byte[] bytes = new byte[nBytes];
            int bitIndex = 0, byteIndex = 0;
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                {
                    bytes[byteIndex] |= (byte)(((byte)1) << bitIndex);
                }
                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
            return bytes;
        }
    }
}
