namespace EPBLib.Logic
{
    public class SignalOperatorInverter : SignalOperator
    {
        public string InSig0 => Tags.ContainsKey("InSig0") && Tags["InSig0"].BlockTagType == BlockTag.TagType.String ? ((BlockTagString)Tags["InSig0"]).Value : "";
    }
}
