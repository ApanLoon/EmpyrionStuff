using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EPBLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

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
        public UInt32 Width
        {
            get => Blueprint.Width;
            set
            {
                Blueprint.Width = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string WidthPropertyName = "Width";

        public UInt32 Height
        {
            get => Blueprint.Height;
            set
            {
                Blueprint.Height = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string HeightPropertyName = "Height";

        public UInt32 Depth
        {
            get => Blueprint.Depth;
            set
            {
                Blueprint.Depth = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string DepthPropertyName = "Depth";

        public UInt16 Unknown01
        {
            get => Blueprint.Unknown01;
            set
            {
                Blueprint.Unknown01 = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string Unknown01PropertyName = "Unknown01";

        public UInt16 Unknown02
        {
            get => Blueprint.Unknown02;
            set
            {
                Blueprint.Unknown02 = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string Unknown02PropertyName = "Unknown02";

        public UInt32 LightCount
        {
            get => Blueprint.LightCount;
            set
            {
                Blueprint.LightCount = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string LightCountPropertyName = "LightCount";

        public UInt32 UnknownCount01
        {
            get => Blueprint.UnknownCount01;
            set
            {
                Blueprint.UnknownCount01 = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string UnknownCount01PropertyName = "UnknownCount01";

        public UInt32 DeviceCount
        {
            get => Blueprint.DeviceCount;
            set
            {
                Blueprint.DeviceCount = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string DeviceCountPropertyName = "DeviceCount";

        public UInt32 UnknownCount02
        {
            get => Blueprint.UnknownCount02;
            set
            {
                Blueprint.UnknownCount02 = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string UnknownCount02PropertyName = "UnknownCount02";

        public UInt32 BlockCount => (UInt32)Blueprint.Blocks.Count;

        public UInt32 UnknownCount03
        {
            get => Blueprint.UnknownCount03;
            set
            {
                Blueprint.UnknownCount03 = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string UnknownCount03PropertyName = "UnknownCount03";

        public UInt32 TriangleCount
        {
            get => Blueprint.TriangleCount;
            set
            {
                Blueprint.TriangleCount = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string TriangleCountPropertyName = "TriangleCount";

        private ObservableCollection<MetaTagViewModel> _metaTags = new ObservableCollection<MetaTagViewModel>();
        public ObservableCollection<MetaTagViewModel> MetaTags
        {
            get => _metaTags;
            set => Set(ref _metaTags, value);
        }

        private ObservableCollection<KeyValuePair<EpbBlock.EpbBlockType, UInt32>> _blockCounts = new ObservableCollection<KeyValuePair<EpbBlock.EpbBlockType, UInt32>>();
        public ObservableCollection<KeyValuePair<EpbBlock.EpbBlockType, UInt32>> BlockCounts
        {
            get => _blockCounts;
            set => Set(ref _blockCounts, value);
        }
        public static readonly string BlockCountsPropertyName = "BlockCounts";

        public EpBlueprint Blueprint;

        #endregion Properties

        #region Commands

        #region Command_ComputeDimensions
        public RelayCommand CommandComputeDimensions
        {
            get {
                return _commandComputeDimensions ?? (_commandComputeDimensions = new RelayCommand(() =>
                           {
                               Blueprint.ComputeDimensions();
                               RaisePropertyChanged(WidthPropertyName);
                               RaisePropertyChanged(HeightPropertyName);
                               RaisePropertyChanged(DepthPropertyName);
                           }));
            }
        }
        private RelayCommand _commandComputeDimensions;
        #endregion Command_ComputeDimensions

        #region Command_CountBlocks
        public RelayCommand CommandCountBlocks
        {
            get
            {
                return _commandCountBlocks ?? (_commandCountBlocks = new RelayCommand(() =>
                {
                    Blueprint.CountBlocks();
                    BlockCounts.Clear();
                    foreach (KeyValuePair<EpbBlock.EpbBlockType, uint> blockCount in Blueprint.BlockCounts)
                    {
                        BlockCounts.Add(blockCount);
                    }
                    RaisePropertyChanged(BlockCountsPropertyName);
                }));
            }
        }
        private RelayCommand _commandCountBlocks;
        #endregion Command_CommandCountBlocks

        #endregion Commands

        public SummaryViewModel(EpBlueprint blueprint)
        {
            Blueprint = blueprint;

            foreach (EpMetaTag tag in blueprint.MetaTags.Values)
            {
                MetaTags.Add(new MetaTagViewModel(tag));
            }

            foreach (KeyValuePair<EpbBlock.EpbBlockType, uint> blockCount in blueprint.BlockCounts)
            {
                BlockCounts.Add(blockCount);
            }
        }
    }
}
