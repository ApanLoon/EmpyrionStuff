
namespace ECFLib.Attributes
{
    public class AttributeInt : EcfAttribute
    {
        public int Value { get; set; }

        public AttributeInt(int value)
        {
            Value = value;
        }
        public override string ValueString()
        {
            return $"{Value}";
        }

    }
}

