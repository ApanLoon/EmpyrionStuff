using EPBLib;
using EPBLib.MetaTags;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel
{
    public class MetaTagViewModel : ViewModelBase
    {
        protected MetaTag Tag;

        public string Key
        {
            get => $"{Tag.Key}:";
            //set => Set(ref _key, value);
        }

        public string TypeName
        {
            get => Tag.TagType.ToString();
            //set => Set(ref _typeName, value);
        }

        public virtual string Value
        {
            get => Tag.ValueToString();
            set => throw new System.NotImplementedException();
        }

        public MetaTagViewModel(MetaTag tag)
        {
            Tag = tag;
        }
    }
}
