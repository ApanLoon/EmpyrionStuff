using System;
using System.Collections.Generic;

namespace EPBLib
{
    public class EpbDeviceGroup
    {
        public string Name { get; set; }
        public UInt16 Flags { get; set; }

        public List<EpbDeviceGroupEntry> Entries = new List<EpbDeviceGroupEntry>();
    }
}
