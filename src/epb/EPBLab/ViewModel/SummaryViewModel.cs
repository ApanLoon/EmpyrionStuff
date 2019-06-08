using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EPBLab.ViewModel.MetaTags;
using EPBLab.ViewModel.Tree;
using EPBLib;
using EPBLib.Helpers;
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

        public ObservableCollection<MetaTagViewModel> MetaTags { get; } = new ObservableCollection<MetaTagViewModel>();

        private ObservableCollection<KeyValuePair<EpbBlockType, UInt32>> _blockCounts = new ObservableCollection<KeyValuePair<EpbBlockType, UInt32>>();
        public ObservableCollection<KeyValuePair<EpbBlockType, UInt32>> BlockCounts
        {
            get => _blockCounts;
            set => Set(ref _blockCounts, value);
        }
        public static readonly string BlockCountsPropertyName = "BlockCounts";

        private ObservableCollection<DeviceGroupViewModel> _deviceGroups = new ObservableCollection<DeviceGroupViewModel>();
        public ObservableCollection<DeviceGroupViewModel> DeviceGroups
        {
            get => _deviceGroups;
            set => Set(ref _deviceGroups, value);
        }
        public static readonly string DeviceGroupsPropertyName = "DeviceGroups";
        public EpBlueprint Blueprint;

        #endregion Properties

        #region Commands

        #region Command_ComputeDimensions
        public RelayCommand CommandComputeDimensions
        {
            get
            {
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
                    foreach (KeyValuePair<EpbBlockType, uint> blockCount in Blueprint.BlockCounts)
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
            Update();
        }

        public void Update()
        {
            RaisePropertyChanged(string.Empty);

            MetaTags.Clear();
            foreach (EpMetaTag tag in Blueprint.MetaTags.Values)
            {
                MetaTagViewModel vm;
                switch (tag)
                {
                    case EpMetaTag02 t:
                        vm = new MetaTag02ViewModel(t);
                        break;
                    case EpMetaTag03 t:
                        vm = new MetaTag03ViewModel(t);
                        break;
                    case EpMetaTag04 t:
                        vm = new MetaTag04ViewModel(t);
                        break;
                    case EpMetaTag05 t:
                        vm = new MetaTag05ViewModel(t);
                        break;
                    case EpMetaTagString t:
                        vm = new MetaTagStringViewModel(t);
                        break;
                    case EpMetaTagUInt16 t:
                        vm = new MetaTagUInt16ViewModel(t);
                        break;
                    default:
                        vm = new MetaTagViewModel(tag);
                        break;
                }
                MetaTags.Add(vm);
            }

            BlockCounts.Clear();
            foreach (KeyValuePair<EpbBlockType, uint> blockCount in Blueprint.BlockCounts)
            {
                BlockCounts.Add(blockCount);
            }

            DeviceGroups.Clear();
            foreach (EpbDeviceGroup group in Blueprint.DeviceGroups)
            {
                DeviceGroups.Add(new DeviceGroupViewModel(group));
            }
        }
    }
}
