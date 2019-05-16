using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using EPBLab.Behaviours;
using EPBLab.Messages;
using GalaSoft.MvvmLight;
using EPBLab.Model;
using GalaSoft.MvvmLight.Command;
using EPBLib;
using EPBLib.BlockData;
using EPBLib.Helpers;
using GalaSoft.MvvmLight.Messaging;

namespace EPBLab.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        //private readonly IDataService _dataService;


        #region Properties

        public const string MainWindowTitlePropertyName = "MainWindowTitle";
        private string _mainWindowTitle = string.Empty;
        public string MainWindowTitle
        {
            get => _mainWindowTitle;
            set => Set(ref _mainWindowTitle, value);
        }

        public const string BlueprintsPropertyName = "Blueprints";
        private ObservableCollection<BlueprintViewModel> _blueprints = new ObservableCollection<BlueprintViewModel>();
        public ObservableCollection<BlueprintViewModel> Blueprints
        {
            get => _blueprints;
            set => Set (ref _blueprints, value);
        }

        public const string SelectedBlueprintIndexPropertyName = "SelectedBlueprintIndex";
        private int _selectedBlueprintIndex;
        public int SelectedBlueprintIndex
        {
            get => _selectedBlueprintIndex;
            set => Set(ref _selectedBlueprintIndex, value);
        }

        /*
        public const string BlueprintIsLoadedPropertyName = "BlueprintIsLoaded";
        private bool _blueprintIsLoaded = false;
        public bool BlueprintIsLoaded
        {
            get
            {
                return _blueprintIsLoaded;
            }
            set
            {
                Set(ref _blueprintIsLoaded, value);
                CommandSave.RaiseCanExecuteChanged();
            }
        }

        */

        #endregion Properties

        #region Commands

        #region Command_New

        public RelayCommand CommandNew
        {
            get { return _commandNew ?? (_commandNew = new RelayCommand(() => { NewBlueprint(); }));}
        }

        private RelayCommand _commandNew;

        #endregion Command_New

        #endregion Commands


        protected void NewBlueprint()
        {
            var blueprint = CreateHullVariants();

            BlueprintViewModel vm = new BlueprintViewModel("New", blueprint);
            Blueprints.Add(vm);
            SelectedBlueprintIndex = Blueprints.Count - 1;
        }


        static EpBlueprint CreateHullVariants()
        {
            UInt16[] types = { 381, 382, 1791, 1833, 1846, 1859, 1872 }; // HullFullSmall, HullThinSmall, HullExtendedSmall, HullExtendedSmall2, HullExtendedSmall3, HullExtendedSmall4, HullExtendedSmall5
            EpBlueprint blueprint = new EpBlueprint(EpBlueprint.EpbType.SmallVessel, 16, (UInt32)types.Length * 2, 20);

            byte y = 0;
            foreach (UInt16 t in types)
            {
                int i = 0;
                foreach (string variantName in EpbBlock.BlockVariants[t])
                {
                    byte x = (byte)((i % 8) * 2);
                    byte z = (byte)((i / 8) * 2);
                    EpbBlock.EpbBlockType bt = EpbBlock.BlockTypes[t];
                    byte v = EpbBlock.GetVariant(t, variantName);
                    EpbBlock block =
                        new EpbBlock(new EpbBlockPos(x, y, z))
                        {
                            BlockType = bt,
                            Variant = v,
                            Colours =
                            {
                                [0] = EpbColourIndex.Red,
                                [1] = EpbColourIndex.BrightGreen,
                                [2] = EpbColourIndex.Blue,
                                [3] = EpbColourIndex.Cyan,
                                [4] = EpbColourIndex.Purple,
                                [5] = EpbColourIndex.Yellow
                            }
                        };
                    blueprint.SetBlock(block);
                    i++;
                }

                y += 2;
            }

            return blueprint;
        }


        protected void OpenBlueprints(FilesOpenedMessage m)
        {
            if (m.Identifier != "OpenBlueprints")
            {
                return;
            }

            foreach (string fileName in m.Content)
            {
                EpBlueprint blueprint = OpenEpb(fileName);
                BlueprintViewModel vm = new BlueprintViewModel(fileName, blueprint);
                Blueprints.Add(vm);
                SelectedBlueprintIndex = Blueprints.Count - 1;
            }
        }
        protected EpBlueprint OpenEpb(string path)
        {
            EpBlueprint blueprint = null;

            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                try
                {
                    long bytesLeft = reader.BaseStream.Length;
                    blueprint = reader.ReadEpBlueprint(ref bytesLeft);
                }
                catch (System.Exception ex)
                {
                    throw new Exception("Failed reading EPB file", ex);
                }
            }
            return blueprint;
        }

        protected void SaveBlueprint(SaveFileSelectedMessage m)
        {
            if (m.Identifier != "SaveBlueprint")
            {
                return;
            }

            FileStream stream = null;
            try
            {
                stream = File.Create(m.Content);
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    stream = null; // The stream will be closed when the writer is closed so set to null so that we don't try to close it twice.
                    writer.Write(Blueprints[SelectedBlueprintIndex].Blueprint);
                    Blueprints[SelectedBlueprintIndex].FileName = m.Content;
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }


        protected void CloseBlueprint(CloseBlueprintMessage m)
        {
            if (Blueprints.Contains(m.Content))
            {
                Blueprints.Remove(m.Content);
            }
        }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            SetMainWindowTitle();
            /*
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    // Update local properties with information from the data item.
                    //WelcomeTitle = item.Title;
                });
            */
            Messenger.Default.Register<FilesOpenedMessage>(this, OpenBlueprints);
            Messenger.Default.Register<SaveFileSelectedMessage>(this, SaveBlueprint);
            Messenger.Default.Register<CloseBlueprintMessage>(this, CloseBlueprint);

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                //Messenger.Default.Send(new FilesOpenedMessage(new string[]{ "BA_Logic.epb" }) { Identifier = "OpenBlueprints" });
                EpBlueprint blueprint = OpenEpb("C:\\Users\\fredrik\\Documents\\Projects\\Empyrion\\EmpyrionStuff\\Research\\SampleData\\v22\\BA_Logic.epb");
                BlueprintViewModel vm = new BlueprintViewModel("BA_Logic.epb", blueprint);
                Blueprints.Add(vm);
                SelectedBlueprintIndex = Blueprints.Count - 1;
            }
            else
            {
            }
        }

        private void SetMainWindowTitle()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName name = assembly.GetName();
            System.Version version = name.Version;
            MainWindowTitle = $"{name.Name} v{version.Major}.{version.Minor}";
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}