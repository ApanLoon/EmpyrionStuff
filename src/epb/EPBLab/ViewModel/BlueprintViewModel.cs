
using System.IO;
using EPBLab.Messages;
using EPBLab.ViewModel.Logic;
using EPBLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace EPBLab.ViewModel
{
    public class BlueprintViewModel : ViewModelBase
    {
        private EpBlueprint _blueprint;

        public const string TabNamePropertyName = "TabName";
        private string _tabName;
        public string TabName
        {
            get => _tabName;
            set => Set(ref _tabName, value);
        }

        public const string FileNamePropertyName = "FileName";
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => Set(ref _fileName, value);
        }

        public const string SummaryPropertyName = "Summary";
        private SummaryViewModel _summary = null;
        public SummaryViewModel Summary
        {
            get => _summary;
            set => Set(ref _summary, value);
        }


        public const string BlocksPropertyName = "Blocks";
        private BlocksViewModel _blocks = null;
        public BlocksViewModel Blocks
        {
            get => _blocks;
            set => Set(ref _blocks, value);
        }
        public const string LogicPropertyName = "Logic";
        private LogicViewModel _logic = null;
        public LogicViewModel Logic
        {
            get => _logic;
            set => Set(ref _logic, value);
        }


        #region Command_Close

        public RelayCommand CommandClose
        {
            get { return _commandClose ?? (_commandClose = new RelayCommand(() => { Messenger.Default.Send(new CloseBlueprintMessage(this)); })); }
        }

        private RelayCommand _commandClose;

        #endregion Command_Close



        public BlueprintViewModel(string fileName, EpBlueprint blueprint)
        {
            FileName = fileName;
            TabName = Path.GetFileNameWithoutExtension(fileName);
            _blueprint = blueprint;

            Summary = new SummaryViewModel(blueprint);
            Blocks = new BlocksViewModel(blueprint);
            Logic = new LogicViewModel(blueprint);
        }
    }
}
