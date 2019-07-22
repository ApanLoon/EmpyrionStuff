using System;

namespace ECFLib.Attributes
{
    public class AttributeIntArray : EcfAttribute
    {
        public int[] Value { get; set; }

        public AttributeIntArray(int[] value)
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
                foreach (int v in Value)
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
            set => Value = Array.ConvertAll(value.Split(','), int.Parse);
        }

    }
}
