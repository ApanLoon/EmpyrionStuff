using System;
using EPBLib;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTagStringViewModel : MetaTagViewModel
    {
        private EpMetaTagString MyTag => Tag as EpMetaTagString;

        public override string Value
        {
            get => MyTag.Value;
            set
            {
                MyTag.Value = value;
                RaisePropertyChanged();
            }
        }

        public MetaTagStringViewModel(EpMetaTagString tag) : base(tag)
        {
        }
    }
}
