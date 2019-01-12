
using System;
using System.Collections.Generic;

namespace EPBLib.Logic
{
    public class EpbSignalOperator
    {
        public byte Unknown01 { get; set; }
        public Dictionary<string, EpbBlockTag> Tags = new Dictionary<string, EpbBlockTag>();

        public string OpName => Tags.ContainsKey("OpName") && Tags["OpName"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["OpName"]).Value : "";
        public string OutSig => Tags.ContainsKey("OutSig") && Tags["OutSig"].BlockTagType == EpbBlockTag.TagType.String ? ((EpbBlockTagString)Tags["OutSig"]).Value : "";

    }
}
