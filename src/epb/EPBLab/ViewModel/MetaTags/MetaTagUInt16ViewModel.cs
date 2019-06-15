
using System;
using EPBLib;
using EPBLib.Helpers;

namespace EPBLab.ViewModel.MetaTags
{
    class MetaTagUInt16ViewModel : MetaTagViewModel
    {
        private MetaTagUInt16 MyTag => Tag as MetaTagUInt16;
        public override string Value
        {
            get => MyTag.Value.ToString();
            set
            {
                MyTag.Value = UInt16.Parse(value);
                RaisePropertyChanged();
            }
        }

        public MetaTagUInt16ViewModel(MetaTagUInt16 tag) : base(tag)
        {
        }
    }
}
