namespace EPBLib.Logic
{
    public class SignalOperatorNand2 : SignalOperator
    {
        public string InSig0 => Tags.ContainsKey("InSig0") && Tags["InSig0"].BlockTagType == BlockTag.TagType.String ? ((BlockTagString)Tags["InSig0"]).Value : "";
        public string InSig1 => Tags.ContainsKey("InSig1") && Tags["InSig1"].BlockTagType == BlockTag.TagType.String ? ((BlockTagString)Tags["InSig1"]).Value : "";
    }
}
