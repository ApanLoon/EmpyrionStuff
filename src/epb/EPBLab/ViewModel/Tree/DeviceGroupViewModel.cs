using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPBLib;

namespace EPBLab.ViewModel.Tree
{
    public class DeviceGroupViewModel
    {
        public EpbDeviceGroup DeviceGroup;

        public string Name => DeviceGroup.Name;
        public byte DeviceGroupUnknown01 => DeviceGroup.DeviceGroupUnknown01;
        public string Shortcut => DeviceGroup.Shortcut == 0xff ? "None" : DeviceGroup.Shortcut.ToString();
        public byte DeviceGroupUnknown03 => DeviceGroup.DeviceGroupUnknown03;
        public List<EpbDeviceGroupEntry> Entries => DeviceGroup.Entries;

        public DeviceGroupViewModel(EpbDeviceGroup deviceGroup)
        {
            DeviceGroup = deviceGroup;
        }
    }
}
