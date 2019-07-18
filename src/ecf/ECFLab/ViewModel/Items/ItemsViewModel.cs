using ECFLib;
using GalaSoft.MvvmLight;

namespace ECFLab.ViewModel.Items
{
    public class ItemsViewModel : ViewModelBase
    {
        private Config _config;

        public ItemsViewModel(Config config)
        {
            _config = config;
        }
    }
}
