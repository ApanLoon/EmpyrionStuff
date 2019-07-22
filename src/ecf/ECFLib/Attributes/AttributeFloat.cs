namespace ECFLib.Attributes
{
    public class AttributeFloat : EcfAttribute
    {
        public float Value { get; set; }

        public AttributeFloat(float value)
        {
            Value = value;
        }
        public override string ValueString
        {
            get => $"{Value}";
            set => Value = float.Parse(value);
        }
    }
}

