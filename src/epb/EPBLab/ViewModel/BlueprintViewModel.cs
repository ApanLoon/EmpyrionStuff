
using System.IO;
using System.Security.Cryptography.X509Certificates;
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
        #region Properties
        public EpBlueprint Blueprint { get; set; }

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
            set
            {
                TabName = Path.GetFileNameWithoutExtension(value);
                Set(ref _fileName, value);
            }
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
        #endregion Properties

        #region Commands
        #region Command_Close

        public RelayCommand CommandClose
        {
            get { return _commandClose ?? (_commandClose = new RelayCommand(() => { Messenger.Default.Send(new CloseBlueprintMessage(this)); })); }
        }

        private RelayCommand _commandClose;

        #endregion Command_Close
        #endregion Commands

        public void UpdateViewModels()
        {
            Summary.Update();
            Blocks.Update();
            Logic.Update();
        }

        public BlueprintViewModel(string fileName, EpBlueprint blueprint)
        {
            FileName = fileName;
            Blueprint = blueprint;

            Summary = new SummaryViewModel(blueprint);
            Blocks = new BlocksViewModel(blueprint);
            Logic = new LogicViewModel(blueprint);
        }
    }
}
