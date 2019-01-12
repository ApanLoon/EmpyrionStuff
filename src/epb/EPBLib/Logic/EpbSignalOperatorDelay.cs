namespace EPBLib.Logic
{
    public class EpbSignalOperatorDelay : EpbSignalOperator
    {
        public string InSig0 => Tags.ContainsKey("InSig0") && Tags["InSig0"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["InSig0"]).Value : "";
        public float Time => Tags.ContainsKey("FPar") && Tags["FPar"].BlockTagType == EpbBlockTag.TagType.Float ? ((EpbBlockTagFloat)Tags["FPar"]).Value : 0.0f;
    }
}
