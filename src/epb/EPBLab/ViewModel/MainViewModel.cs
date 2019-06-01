using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EPBLab.Behaviours;
using EPBLab.Messages;
using GalaSoft.MvvmLight;
using EPBLab.Model;
using GalaSoft.MvvmLight.Command;
using EPBLib;
using EPBLib.BlockData;
using EPBLib.Helpers;
using EPBLib.Logic;
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
        private int _selectedBlueprintIndex = -1;
        public int SelectedBlueprintIndex
        {
            get => _selectedBlueprintIndex;
            set
            {
                Set(ref _selectedBlueprintIndex, value);
                RaisePropertyChanged(ShowBuildStructuresPropertyName);
            } 
        }

        public const string ShowBuildStructuresPropertyName = "ShowBuildStructures";

        public Visibility ShowBuildStructures => SelectedBlueprintIndex != -1 ? Visibility.Visible : Visibility.Collapsed;

        public const string BuildStructureCommandsPropertyName = "BuildStructureCommands";
        private ObservableCollection<ToolBarCommand> _buildStructureCommands = new ObservableCollection<ToolBarCommand>();
        public ObservableCollection<ToolBarCommand> BuildStructureCommands
        {
            get => _buildStructureCommands;
            set => Set(ref _buildStructureCommands, value);
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

        #region Command_CreateBox
        public RelayCommand CommandCreateBox
        {
            get
            {
                return _commandCreateBox ?? (_commandCreateBox = new RelayCommand(() =>
                {
                    if (SelectedBlueprintIndex == -1)
                    {
                        return;
                    }
                    EpBlueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;

                    int width = 10;
                    int height = 10;
                    int depth = 10;
                    bool hollow = false;
                    EpbBlock.EpbBlockType blockType = EpbBlock.GetBlockType("HullFullLarge", "Cube");
                    byte blockVariant = EpbBlock.GetVariant(blockType.Id, "Cube");
                    for (int z = 0; z < depth; z++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                bool isInterior = (x > 0 && x < (width - 1))
                                                  && (y > 0 && y < (height - 1))
                                                  && (z > 0 && z < (depth - 1));

                                if (!isInterior || !hollow)
                                {
                                    EpbBlock block = new EpbBlock(new EpbBlockPos((byte)x, (byte)y, (byte)z)) { BlockType = blockType, Variant = blockVariant };
                                    block.SetColour(isInterior ? EpbColourIndex.Pink : EpbColourIndex.None);
                                    block.SetTexture(14, (x % 2) == 1);
                                    block.SetSymbol(1, (EpbBlock.SymbolRotation)(x % 4), EpbBlock.FaceIndex.Back);
                                    block.SetSymbol(2, face: EpbBlock.FaceIndex.Right);
                                    block.SetSymbol(3, face: EpbBlock.FaceIndex.Front);
                                    block.SetSymbol(4, face: EpbBlock.FaceIndex.Left);
                                    block.SetSymbol(5, face: EpbBlock.FaceIndex.Top);
                                    block.SetSymbol(6, face: EpbBlock.FaceIndex.Bottom);
                                    blueprint.SetBlock(block);
                                }
                            }
                        }
                    }
                    blueprint.CountBlocks();
                    blueprint.ComputeDimensions();

                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                }));
            }
        }
        private RelayCommand _commandCreateBox;
        #endregion Command_CreateBox
        #region Command_CreateBoxFrame
        public RelayCommand CommandCreateBoxFrame
        {
            get
            {
                return _commandCreateBoxFrame ?? (_commandCreateBoxFrame = new RelayCommand(() =>
                {
                    if (SelectedBlueprintIndex == -1)
                    {
                        return;
                    }
                    EpBlueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;

                    int width = 10;
                    int height = 10;
                    int depth = 10;
                    EpbBlock.EpbBlockType blockType = EpbBlock.GetBlockType("HullFullLarge", "Cube");
                    byte blockVariant = EpbBlock.GetVariant(blockType.Id, "Cube");
                    for (int z = 0; z < depth; z++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                bool a = x % (width - 1) == 0;
                                bool b = y % (height - 1) == 0;
                                bool c = z % (depth - 1) == 0;
                                int d = (a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0);
                                if (d >= 2)
                                {
                                    blueprint.SetBlock(new EpbBlock(new EpbBlockPos((byte)x, (byte)y, (byte)z)) { BlockType = blockType, Variant = blockVariant });
                                }
                            }
                        }
                    }
                    blueprint.CountBlocks();
                    blueprint.ComputeDimensions();

                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                }));
            }
        }
        private RelayCommand _commandCreateBoxFrame;
        #endregion Command_CreateBoxFrame
        #region Command_CreatePyramid
        public RelayCommand CommandCreatePyramid
        {
            get
            {
                return _commandCreatePyramid ?? (_commandCreatePyramid = new RelayCommand(() =>
                {
                    if (SelectedBlueprintIndex == -1)
                    {
                        return;
                    }
                    EpBlueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;

                    int width = 10;
                    int height = 10;
                    int depth = 10;
                    bool hollow = false;
                    EpbBlock.EpbBlockType blockType = EpbBlock.GetBlockType("HullFullLarge", "Cube");
                    byte blockVariant = EpbBlock.GetVariant(blockType.Id, "Cube");

                    for (int y = 0; y < height; y++)
                    {
                        for (int z = y; z < depth; z++)
                        {
                            for (int x = y; x < width; x++)
                            {
                                bool isBackEdge = (z == y);
                                bool isFrontEdge = (z == (depth - 1));
                                bool isLeftEdge = (x == y);
                                bool isRightEdge = (x == (width - 1));
                                bool isInterior = !isBackEdge && !isFrontEdge && !isRightEdge && !isLeftEdge && y > 0 && y < (height - 1);

                                EpbBlock.EpbBlockType t = blockType;
                                EpbBlock.EpbBlockRotation r = EpbBlock.EpbBlockRotation.PzPy;
                                byte v = blockVariant;
                                byte[] c = new byte[] { 0, 0, 0, 0, 0, 0 };

                                if (isBackEdge && isLeftEdge)
                                {
                                    t = EpbBlock.GetBlockType("HullFullLarge", "CornerC");
                                    v = EpbBlock.GetVariant(t.Id, "CornerC");
                                    r = EpbBlock.EpbBlockRotation.PxPy;
                                }
                                else if (isBackEdge && isRightEdge)
                                {
                                    t = EpbBlock.GetBlockType("HullFullLarge", "CornerC");
                                    v = EpbBlock.GetVariant(t.Id, "CornerC");
                                    r = EpbBlock.EpbBlockRotation.PzPy;
                                }
                                else if (isFrontEdge && isLeftEdge)
                                {
                                    t = EpbBlock.GetBlockType("HullFullLarge", "CornerC");
                                    v = EpbBlock.GetVariant(t.Id, "CornerC");
                                    r = EpbBlock.EpbBlockRotation.NzPy;
                                }
                                else if (isFrontEdge && isRightEdge)
                                {
                                    t = EpbBlock.GetBlockType("HullFullLarge", "CornerC");
                                    v = EpbBlock.GetVariant(t.Id, "CornerC");
                                    r = EpbBlock.EpbBlockRotation.NxPy;
                                }
                                else if (isBackEdge)
                                {
                                    t = EpbBlock.GetBlockType("HullFullLarge", "RampC");
                                    v = EpbBlock.GetVariant(t.Id, "RampC");
                                    r = EpbBlock.EpbBlockRotation.NzPy;
                                }
                                else if (isFrontEdge)
                                {
                                    t = EpbBlock.GetBlockType("HullFullLarge", "RampC");
                                    v = EpbBlock.GetVariant(t.Id, "RampC");
                                    r = EpbBlock.EpbBlockRotation.PzPy;
                                }
                                else if (isLeftEdge)
                                {
                                    t = EpbBlock.GetBlockType("HullFullLarge", "RampC");
                                    v = EpbBlock.GetVariant(t.Id, "RampC");
                                    r = EpbBlock.EpbBlockRotation.NxPy;
                                }
                                else if (isRightEdge)
                                {
                                    t = EpbBlock.GetBlockType("HullFullLarge", "RampC");
                                    v = EpbBlock.GetVariant(t.Id, "RampC");
                                    r = EpbBlock.EpbBlockRotation.PxPy;
                                }

                                if (!isInterior || !hollow)
                                {
                                    EpbBlock block = new EpbBlock(new EpbBlockPos((byte)x, (byte)y, (byte)z)) { BlockType = t, Rotation = r, Variant = v };
                                    block.SetColour(isInterior ? EpbColourIndex.Pink : EpbColourIndex.None);
                                    blueprint.SetBlock(block);
                                }
                            }
                        }
                        width -= 1;
                        depth -= 1;
                    }
                    blueprint.CountBlocks();
                    blueprint.ComputeDimensions();

                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                }));
            }
        }
        private RelayCommand _commandCreatePyramid;
        #endregion Command_CreatePyramid
        #region Command_CreateCore
        public RelayCommand CommandCreateCore
        {
            get { return _commandCreateCore ?? (_commandCreateCore = new RelayCommand(() =>
            {
                if (SelectedBlueprintIndex == -1)
                {
                    return;
                }
                EpBlueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;
                EpbBlockPos corePos = new EpbBlockPos(0, 0, 0, 8, 8);
                blueprint.SetBlock(
                    new EpbBlock(corePos)
                    {
                        BlockType = EpbBlock.BlockTypes[558],
                        Variant = 0
                    });
                blueprint.DeviceCount = 1;
                blueprint.CountBlocks();
                blueprint.ComputeDimensions();

                EpbDeviceGroup group = new EpbDeviceGroup()
                {
                    Name = "Ungrouped",
                    DeviceGroupUnknown01 = 1,
                    Shortcut = 255,
                    DeviceGroupUnknown03 = 0
                };
                group.Entries.Add(new EpbDeviceGroupEntry() { Pos = corePos });
                blueprint.DeviceGroups.Add(group);

                Blueprints[SelectedBlueprintIndex].UpdateViewModels();
            })); }
        }
        private RelayCommand _commandCreateCore;
        #endregion Command_CreateCore
        #region Command_CreateCoreWithLever
        public RelayCommand CommandCreateCoreWithLever
        {
            get { return _commandCreateCoreWithLever ?? (_commandCreateCoreWithLever = new RelayCommand(() =>
            {
                if (SelectedBlueprintIndex == -1)
                {
                    return;
                }
                EpBlueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;
                EpbBlockPos leverPos = new EpbBlockPos(0, 0, 0, 8, 8);
                EpbBlockPos corePos = new EpbBlockPos(0, 0, 1, 8, 8);

                blueprint.SetBlock(
                    new EpbBlock(leverPos)
                    {
                        BlockType = EpbBlock.BlockTypes[1262],
                        Variant = 0
                    });
                blueprint.SetBlock(
                    new EpbBlock(corePos)
                    {
                        BlockType = EpbBlock.BlockTypes[558],
                        Variant = 0
                    });

                blueprint.DeviceCount = 2;
                blueprint.CountBlocks();
                blueprint.ComputeDimensions();

                EpbDeviceGroup group = new EpbDeviceGroup()
                {
                    Name = "Ungrouped",
                    DeviceGroupUnknown01 = 1,
                    Shortcut = 255,
                    DeviceGroupUnknown03 = 0
                };
                group.Entries.Add(new EpbDeviceGroupEntry() { Pos = corePos });
                group.Entries.Add(new EpbDeviceGroupEntry() { Pos = leverPos });
                blueprint.DeviceGroups.Add(group);

                blueprint.SignalSources.Add(new EpbSignalSource()
                {
                    Name = "Lever0",
                    Pos = leverPos,
                    State = 0x00020000
                });
                Blueprints[SelectedBlueprintIndex].UpdateViewModels();
            })); }
        }
        private RelayCommand _commandCreateCoreWithLever;

        #endregion Command_CreateCoreWithLever
        #region Command_CreateHullVariants
        public RelayCommand CommandCreateHullVariants
        {
            get { return _commandCreateHullVariants ?? (_commandCreateHullVariants = new RelayCommand(() =>
            {
                if (SelectedBlueprintIndex == -1)
                {
                    return;
                }
                EpBlueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;
                UInt16[] types = { 381, 382, 1791, 1833, 1846, 1859, 1872 }; // HullFullSmall, HullThinSmall, HullExtendedSmall, HullExtendedSmall2, HullExtendedSmall3, HullExtendedSmall4, HullExtendedSmall5
                byte y = 0;
                foreach (UInt16 t in types)
                {
                    int i = 0;
                    foreach (string variantName in EpbBlock.BlockVariants[t])
                    {
                        if (variantName != null)
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
                        }
                        i++;
                    }
                    y += 2;
                }
                blueprint.CountBlocks();
                blueprint.ComputeDimensions();

                Blueprints[SelectedBlueprintIndex].UpdateViewModels();
            })); }
        }
        private RelayCommand _commandCreateHullVariants;
        #endregion Command_CreateHullVariants

        #endregion Commands

        public class ToolBarCommand
        {
            public string Name { get; set; }
            public string Icon { get => _icon; set => _icon = $"pack://application:,,,/Images/ToolbarCommandIcons/{value}"; }
            private string _icon;
            public RelayCommand Command { get; set; }
        }

        protected void NewBlueprint()
        {
            EpBlueprint blueprint = new EpBlueprint();

            BlueprintViewModel vm = new BlueprintViewModel("New", blueprint);
            Blueprints.Add(vm);
            SelectedBlueprintIndex = Blueprints.Count - 1;
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

            BuildStructureCommands.Add(new ToolBarCommand() { Name = "Box",           Icon = "BuildStructure/Box.png",      Command = CommandCreateBox });
            BuildStructureCommands.Add(new ToolBarCommand() { Name = "Box frame",     Icon = "BuildStructure/BoxFrame.png", Command = CommandCreateBoxFrame });
            BuildStructureCommands.Add(new ToolBarCommand() { Name = "Pyramid",       Icon = "BuildStructure/Pyramid.png",  Command = CommandCreatePyramid });
            BuildStructureCommands.Add(new ToolBarCommand() { Name = "Core",          Icon = "Empty.png",                   Command = CommandCreateCore});
            BuildStructureCommands.Add(new ToolBarCommand() { Name = "Core + lever",  Icon = "Empty.png",                   Command = CommandCreateCoreWithLever});
            BuildStructureCommands.Add(new ToolBarCommand() { Name = "Hull variants", Icon = "Empty.png",                   Command = CommandCreateHullVariants});

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