namespace EPBLib.Logic
{
    public class EpbSignalOperatorOr2 : EpbSignalOperator
    {
        public string InSig0 => Tags.ContainsKey("InSig0") && Tags["InSig0"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["InSig0"]).Value : "";
        public string InSig1 => Tags.ContainsKey("InSig1") && Tags["InSig1"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["InSig1"]).Value : "";
    }
}
