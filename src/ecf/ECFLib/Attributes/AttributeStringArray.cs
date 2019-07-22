using System;

namespace ECFLib.Attributes
{
    public class AttributeStringArray : EcfAttribute
    {
        public string[] Value { get; set; }

        public AttributeStringArray(string[] value)
        {
            Value = value;
        }

        public override string ValueString
        {
            get
            {
                string s = "";
                if (Value == null)
                {
                    return s;
                }
                bool first = true;
                bool single = true;
                foreach (string v in Value)
                {
                    if (!first)
                    {
                        s += ", ";
                        single = false;
                    }
                    s += v;
                    first = false;
                }
                if (!single)
                {
                    s = "\"" + s + "\"";
                }
                return s;
            }
            set => Value = Array.ConvertAll(value.Split(','), s => s.Trim());
        }

    }
}
