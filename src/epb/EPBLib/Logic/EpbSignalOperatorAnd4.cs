
namespace EPBLib.Logic
{
    public class EpbSignalOperatorAnd4 : EpbSignalOperator
    {
        public string InSig0 => Tags.ContainsKey("InSig0") && Tags["InSig0"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["InSig0"]).Value : "";
        public string InSig1 => Tags.ContainsKey("InSig1") && Tags["InSig1"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["InSig1"]).Value : "";
        public string InSig2 => Tags.ContainsKey("InSig2") && Tags["InSig2"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["InSig2"]).Value : "";
        public string InSig3 => Tags.ContainsKey("InSig3") && Tags["InSig3"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["InSig3"]).Value : "";
    }
}
