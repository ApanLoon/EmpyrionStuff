using System;
using EPBLib.Helpers;

namespace EPBLib
{
    public class EpbBlockTagx03 : EpbBlockTag
    {
        public EpbBlockTagx03(string name)
        {
            BlockTagType = TagType.x03;
            Name = name;
        }

        public byte[] Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value={Value.ToHexString()}";
        }
    }
}
