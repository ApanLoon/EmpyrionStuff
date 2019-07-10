namespace ECFLib.Attributes
{
    public class AttributeColour : EcfAttribute
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public AttributeColour(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
        public override string ValueString()
        {
            return $"\"{Red}, {Green}, {Blue}\"";
        }

    }
}

