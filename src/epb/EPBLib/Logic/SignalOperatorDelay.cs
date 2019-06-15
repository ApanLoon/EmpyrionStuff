namespace EPBLib.Logic
{
    public class SignalOperatorDelay : SignalOperator
    {
        public string InSig0 => Tags.ContainsKey("InSig0") && Tags["InSig0"].BlockTagType == BlockTag.TagType.String ? ((BlockTagString)Tags["InSig0"]).Value : "";
        public float Time => Tags.ContainsKey("FPar") && Tags["FPar"].BlockTagType == BlockTag.TagType.Float ? ((BlockTagFloat)Tags["FPar"]).Value : 0.0f;
    }
}
