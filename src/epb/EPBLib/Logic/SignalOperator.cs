
using System;
using System.Collections.Generic;

namespace EPBLib.Logic
{
    public class SignalOperator
    {
        public byte Unknown01 { get; set; }
        public Dictionary<string, BlockTag> Tags = new Dictionary<string, BlockTag>();

        public string OpName => Tags.ContainsKey("OpName") && Tags["OpName"].BlockTagType == BlockTag.TagType.String ? ((BlockTagString)Tags["OpName"]).Value : "";
        public string OutSig => Tags.ContainsKey("OutSig") && Tags["OutSig"].BlockTagType == BlockTag.TagType.String ? ((BlockTagString)Tags["OutSig"]).Value : "";

    }
}
