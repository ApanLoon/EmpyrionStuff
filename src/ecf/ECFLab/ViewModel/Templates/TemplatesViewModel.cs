using ECFLib;
using GalaSoft.MvvmLight;

namespace ECFLab.ViewModel.Templates
{
    public class TemplatesViewModel : ViewModelBase
    {
        private Config _config;

        public TemplatesViewModel(Config config)
        {
            _config = config;
        }
    }
}
