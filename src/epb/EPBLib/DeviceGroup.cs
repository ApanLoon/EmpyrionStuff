using System;
using System.Collections.Generic;
using System.Globalization;

namespace EPBLib
{
    public class DeviceGroup
    {
        public string Name { get; set; }
        public byte DeviceGroupUnknown01 { get; set; }
        public byte Shortcut { get; set; }
        public byte DeviceGroupUnknown03 { get; set; }

        public List<DeviceGroupEntry> Entries = new List<DeviceGroupEntry>();
    }
}
