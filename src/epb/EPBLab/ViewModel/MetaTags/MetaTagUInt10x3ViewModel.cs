
using System;
using EPBLib.MetaTags;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTagUInt10x3ViewModel : MetaTagViewModel
    {
        private MetaTagUInt10x3 MyTag => Tag as MetaTagUInt10x3;

        public override string Value
        {
            get => MyTag.Value.ToString();
            set
            {
                MyTag.Value = UInt32.Parse(value);
                RaisePropertyChanged();
            }
        }

        public string X
        {
            get => MyTag.X.ToString();
            set
            {
                MyTag.X = UInt16.Parse(value);
                RaisePropertyChanged();
            }
        }
        public string Y
        {
            get => MyTag.Y.ToString();
            set
            {
                MyTag.Y = UInt16.Parse(value);
                RaisePropertyChanged();
            }
        }
        public string Z
        {
            get => MyTag.Z.ToString();
            set
            {
                MyTag.Z = UInt16.Parse(value);
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

        public MetaTagUInt10x3ViewModel(MetaTagUInt10x3 tag) : base(tag)
        {
        }

    }
}
