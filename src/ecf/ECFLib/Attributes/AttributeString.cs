
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
            return $"{Value}";
        }

    }
}
