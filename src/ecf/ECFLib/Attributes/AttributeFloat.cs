namespace ECFLib.Attributes
{
    public class AttributeFloat : EcfAttribute
    {
        public float Value { get; set; }

        public AttributeFloat(float value)
        {
            Value = value;
        }
        public override string ValueString()
        {
            return $"{Value}";
        }

    }
}

