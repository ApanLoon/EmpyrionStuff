using System;
using System.Collections.Generic;

namespace EPBLib
{
    public class EpbDeviceGroup
    {
        public string Name { get; set; }
        public byte DeviceGroupUnknown01 { get; set; }
        public byte DeviceGroupUnknown02 { get; set; }

        public List<EpbDeviceGroupEntry> Entries = new List<EpbDeviceGroupEntry>();
    }
}
