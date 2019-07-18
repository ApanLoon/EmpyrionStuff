
using ECFLib;
using GalaSoft.MvvmLight;

namespace ECFLab.ViewModel.Entities
{
    public class EntitiesViewModel : ViewModelBase
    {
        private Config _config;

        public EntitiesViewModel(Config config)
        {
            _config = config;
        }
    }
}
