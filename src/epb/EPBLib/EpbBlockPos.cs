
namespace EPBLib
{
    public class EpbBlockPos
    {
        public byte U1 { get; set; } // Four bit unknown value
        public byte U2 { get; set; } // Four bit unknown value

        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Z { get; set; }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z}) U1=0x{U1:x1} U2=0x{U2:x1}";
        }
    }
}
