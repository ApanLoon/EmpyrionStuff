using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPBLib;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel
{
    public class SummaryViewModel : ViewModelBase
    {
        #region Properties

        public UInt32 Version => Blueprint.Version;

        public IEnumerable<EpBlueprint.EpbType> BlueprintTypes => Enum.GetValues(typeof(EpBlueprint.EpbType)).Cast<EpBlueprint.EpbType>();

        public const string TypePropertyName = "Type";
        public EpBlueprint.EpbType Type
        {
            get => Blueprint.Type;
            set => Blueprint.Type = value;
        }
        public UInt32 Width => Blueprint.Width;
        public UInt32 Height => Blueprint.Height;
        public UInt32 Depth => Blueprint.Depth;

        private ObservableCollection<MetaTagViewModel> _metaTags = new ObservableCollection<MetaTagViewModel>();
        public ObservableCollection<MetaTagViewModel> MetaTags
        {
            get => _metaTags;
            set => Set(ref _metaTags, value);
        }

        public EpBlueprint Blueprint;

        #endregion Properties

        public SummaryViewModel(EpBlueprint blueprint)
        {
            Blueprint = blueprint;

            foreach (EpMetaTag tag in blueprint.MetaTags.Values)
            {
                MetaTags.Add(new MetaTagViewModel(tag));
            }
        }
    }
}
