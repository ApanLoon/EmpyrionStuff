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
        public override string ValueString
        {
            get => $"\"{Red}, {Green}, {Blue}\"";
            set
            {
                string[] v = value.Split(',');
                Red   = byte.Parse(v[0]);
                Green = byte.Parse(v[1]);
                Blue  = byte.Parse(v[2]);
            }
        }
    }
}

