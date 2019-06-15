using System;
using EPBLib;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTagStringViewModel : MetaTagViewModel
    {
        private MetaTagString MyTag => Tag as MetaTagString;

        public override string Value
        {
            get => MyTag.Value;
            set
            {
                MyTag.Value = value;
                RaisePropertyChanged();
            }
        }

        public MetaTagStringViewModel(MetaTagString tag) : base(tag)
        {
        }
    }
}
