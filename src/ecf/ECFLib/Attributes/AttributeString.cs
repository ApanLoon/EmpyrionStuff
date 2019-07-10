
using System.Text.RegularExpressions;

namespace ECFLib.Attributes
{
    public class AttributeString : EcfAttribute
    {
        public string Value { get; set; }

        public AttributeString(string value)
        {
            Value = value;
        }
        public override string ValueString()
        {
            if (Value == null)
            {
                return "";
            }

            if (Regex.IsMatch(Value, "\\s|:"))
            {
                return $"\"{Value}\"";
            }

            return Value;
        }

    }
}
