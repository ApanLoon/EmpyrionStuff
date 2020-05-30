using System;
using EPBLib.MetaTags;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTagUInt32ViewModel : MetaTagViewModel
    {
        private MetaTagUInt32 MyTag => Tag as MetaTagUInt32;

        public override string Value
        {
            get => MyTag.Value.ToString();
            set
            {
                MyTag.Value = UInt32.Parse(value);
                RaisePropertyChanged();
            }
        }

        public string Unknown
        {
            get => MyTag.Unknown.ToString();
            set
            {
                MyTag.Unknown = byte.Parse(value);
                RaisePropertyChanged();
            }
        }

        public MetaTagUInt32ViewModel(MetaTagUInt32 tag) : base(tag)
        {
        }
    }
}
