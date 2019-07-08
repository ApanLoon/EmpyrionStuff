namespace ECFLib.Attributes
{
    public class AttributeStringArray : EcfAttribute
    {
        public string[] Value { get; set; }

        public AttributeStringArray(string[] value)
        {
            Value = value;
        }
        public override string ValueString()
        {
            string s = "";
            bool first = true;
            foreach (string v in Value)
            {
                if (!first)
                {
                    s += ", ";
                }
                s += v;
                first = false;
            }
            return s;
        }

    }
}
