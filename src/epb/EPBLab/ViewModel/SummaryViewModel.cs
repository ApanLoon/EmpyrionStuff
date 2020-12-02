using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EPBLab.ViewModel.MetaTags;
using EPBLab.ViewModel.Tree;
using EPBLib;
using EPBLib.Helpers;
using EPBLib.MetaTags;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace EPBLab.ViewModel
{
    public class SummaryViewModel : ViewModelBase
    {
        #region Properties

        public UInt32 Version => Blueprint.Version;

        public IEnumerable<BlueprintType> BlueprintTypes => Enum.GetValues(typeof(BlueprintType)).Cast<BlueprintType>();

        public const string TypePropertyName = "Type";
        public BlueprintType Type
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

        public UInt32 DoorCount
        {
            get => Blueprint.DoorCount;
            set
            {
                Blueprint.DoorCount = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string DoorCountPropertyName = "DoorCount";

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

        public UInt32 SolidCount
        {
            get => Blueprint.SolidCount;
            set
            {
                Blueprint.SolidCount = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string SolidCountPropertyName = "SolidCount";

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
        
        public UInt32 UnknownCount04
        {
            get => Blueprint.UnknownCount04;
            set
            {
                Blueprint.UnknownCount04 = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string UnknownCount04PropertyName = "UnknownCount04";

        public UInt32 UnknownCount05
        {
            get => Blueprint.UnknownCount05;
            set
            {
                Blueprint.UnknownCount05 = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string UnknownCount05PropertyName = "UnknownCount05";

        public UInt32 UnknownCount06
        {
            get => Blueprint.UnknownCount06;
            set
            {
                Blueprint.UnknownCount06 = value;
                RaisePropertyChanged();
            }
        }
        public static readonly string UnknownCount06PropertyName = "UnknownCount06";

        public ObservableCollection<MetaTagViewModel> MetaTags { get; } = new ObservableCollection<MetaTagViewModel>();

        private ObservableCollection<KeyValuePair<BlockType, UInt32>> _blockCounts = new ObservableCollection<KeyValuePair<BlockType, UInt32>>();
        public ObservableCollection<KeyValuePair<BlockType, UInt32>> BlockCounts
        {
            get => _blockCounts;
            set => Set(ref _blockCounts, value);
        }
        public static readonly string BlockCountsPropertyName = "BlockCounts";

        public float Attack
        {
            get => Blueprint.Attack;
            set
            {
                Blueprint.Attack = value;
                RaisePropertyChanged();
            }
        }
        public float Defence
        {
            get => Blueprint.Defence;
            set
            {
                Blueprint.Defence = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<DeviceGroupViewModel> _deviceGroups = new ObservableCollection<DeviceGroupViewModel>();
        public ObservableCollection<DeviceGroupViewModel> DeviceGroups
        {
            get => _deviceGroups;
            set => Set(ref _deviceGroups, value);
        }
        public static readonly string DeviceGroupsPropertyName = "DeviceGroups";
        public Blueprint Blueprint;

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
                    foreach (KeyValuePair<BlockType, uint> blockCount in Blueprint.BlockCounts)
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

        public SummaryViewModel(Blueprint blueprint)
        {
            Blueprint = blueprint;
            Update();
        }

        public void Update()
        {
            RaisePropertyChanged(string.Empty);

            MetaTags.Clear();
            foreach (MetaTag tag in Blueprint.MetaTags.Values)
            {
                MetaTagViewModel vm;
                switch (tag)
                {
                    case MetaTagUInt10x3 t:
                        vm = new MetaTagUInt10x3ViewModel(t);
                        break;
                    case MetaTagUInt32 t:
                        vm = new MetaTagUInt32ViewModel(t);
                        break;
                    case MetaTagFloat t:
                        vm = new MetaTagFloatViewModel(t);
                        break;
                    case MetaTag04 t:
                        vm = new MetaTag04ViewModel(t);
                        break;
                    case MetaTagDateTime t:
                        vm = new MetaTagDateTimeViewModel(t);
                        break;
                    case MetaTagString t:
                        vm = new MetaTagStringViewModel(t);
                        break;
                    case MetaTagUInt16 t:
                        vm = new MetaTagUInt16ViewModel(t);
                        break;
                    default:
                        vm = new MetaTagViewModel(tag);
                        break;
                }
                MetaTags.Add(vm);
            }

            BlockCounts.Clear();
            foreach (KeyValuePair<BlockType, uint> blockCount in Blueprint.BlockCounts)
            {
                BlockCounts.Add(blockCount);
            }

            DeviceGroups.Clear();
            foreach (DeviceGroup group in Blueprint.DeviceGroups)
            {
                DeviceGroups.Add(new DeviceGroupViewModel(group));
            }
        }
    }
}
