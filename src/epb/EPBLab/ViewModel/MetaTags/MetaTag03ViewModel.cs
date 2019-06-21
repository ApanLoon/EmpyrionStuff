
using System;
using System.Linq;
using EPBLib;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTag03ViewModel : MetaTagViewModel
    {
        private MetaTag03 MyTag => Tag as MetaTag03;

        public override string Value
        {
            get => $"{MyTag.Value}";
            set
            {
                if (float.TryParse(value, out float f))
                {
                    MyTag.Value = f;
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

        public MetaTag03ViewModel(MetaTag03 tag) : base(tag)
        {
        }
    }
}
