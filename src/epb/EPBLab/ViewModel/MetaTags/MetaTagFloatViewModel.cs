
using EPBLib;

namespace EPBLab.ViewModel.MetaTags
{
    public class MetaTagFloatViewModel : MetaTagViewModel
    {
        private MetaTagFloat MyTag => Tag as MetaTagFloat;

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

        public MetaTagFloatViewModel(MetaTagFloat tag) : base(tag)
        {
        }
    }
}
