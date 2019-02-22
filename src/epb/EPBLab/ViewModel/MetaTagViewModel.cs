using EPBLib;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel
{
    public class MetaTagViewModel : ViewModelBase
    {
        private string _key;
        public string Key
        {
            get => _key;
            set => Set(ref _key, value);
        }

        private string _typeName;
        public string TypeName
        {
            get => _typeName;
            set => Set(ref _typeName, value);
        }

        private string _value;

        public string Value
        {
            get => _value;
            set => Set(ref _value, value); //TODO: Allow setting the value in the model
        }

        public MetaTagViewModel(EpMetaTag tag)
        {
            Key = tag.Key.ToString();
            TypeName = tag.TagType.ToString();
            Value = tag.ToString();
        }
    }
}
