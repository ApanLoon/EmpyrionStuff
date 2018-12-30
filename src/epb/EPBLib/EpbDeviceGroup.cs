using System;
using System.Collections.Generic;
using System.Globalization;

namespace EPBLib
{
    public class EpbDeviceGroup
    {
        public string Name { get; set; }
        public byte DeviceGroupUnknown01 { get; set; }
        public byte Shortcut { get; set; }
        public byte DeviceGroupUnknown03 { get; set; }

        public List<EpbDeviceGroupEntry> Entries = new List<EpbDeviceGroupEntry>();
    }
}
