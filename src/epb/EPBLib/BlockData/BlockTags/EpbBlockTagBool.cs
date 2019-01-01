﻿using System;

namespace EPBLib
{
    public class EpbBlockTagBool : EpbBlockTag
    {
        public EpbBlockTagBool(string name)
        {
            BlockTagType = TagType.Bool;
            Name = name;
        }

        public bool Value { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} Value={Value}";
        }
    }
}
