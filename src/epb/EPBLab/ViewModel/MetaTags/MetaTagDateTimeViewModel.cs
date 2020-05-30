using System;
using EPBLib;
using EPBLib.MetaTags;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTagDateTimeViewModel : MetaTagViewModel
    {
        private MetaTagDateTime MyTag => Tag as MetaTagDateTime;

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

        public MetaTagDateTimeViewModel(MetaTagDateTime tag) : base(tag)
        {
        }
    }
}
