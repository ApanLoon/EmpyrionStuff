
namespace ECFLib.Attributes
{
    public class AttributeBool : EcfAttribute
    {
        public bool Value { get; set; }

        public AttributeBool(bool value)
        {
            Value = value;
        }

        public override string ValueString()
        {
            return Value ? "true" : "false";
        }

    }
}
