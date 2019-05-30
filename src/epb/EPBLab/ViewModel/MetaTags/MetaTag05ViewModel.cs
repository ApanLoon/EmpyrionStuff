using System;
using EPBLib;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTag05ViewModel : MetaTagViewModel
    {
        private EpMetaTag05 MyTag => Tag as EpMetaTag05;

        public string Date
        {
            get => $"{MyTag.Value}";
            set
            {
                DateTime dt;
                if (DateTime.TryParse(value, out dt))
                {
                    MyTag.Value = dt;
                }
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

        public MetaTag05ViewModel(EpMetaTag05 tag) : base(tag)
        {
        }
    }
}
