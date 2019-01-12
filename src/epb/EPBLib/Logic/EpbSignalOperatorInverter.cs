namespace EPBLib.Logic
{
    public class EpbSignalOperatorInverter : EpbSignalOperator
    {
        public string InSig0 => Tags.ContainsKey("InSig0") && Tags["InSig0"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["InSig0"]).Value : "";
    }
}
