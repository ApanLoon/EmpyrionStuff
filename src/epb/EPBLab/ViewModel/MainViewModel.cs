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

        #region CommandCreateBox
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
                    Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;

                    int width = 10;
                    int height = 10;
                    int depth = 10;
                    bool hollow = false;
                    BlockType blockType = BlockType.GetBlockType("HullFullLarge", "Cube");
                    byte blockVariant = BlockType.GetVariant(blockType.Id, "Cube");
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
                                    Block block = new Block(new BlockPos((byte)x, (byte)y, (byte)z)) { BlockType = blockType, Variant = blockVariant };
                                    block.SetColour(isInterior ? ColourIndex.Pink : ColourIndex.None);
                                    block.SetTexture(14, (x % 2) == 1);
                                    block.SetSymbol(1, (Block.SymbolRotation)(x % 4), Block.FaceIndex.Back);
                                    block.SetSymbol(2, face: Block.FaceIndex.Right);
                                    block.SetSymbol(3, face: Block.FaceIndex.Front);
                                    block.SetSymbol(4, face: Block.FaceIndex.Left);
                                    block.SetSymbol(5, face: Block.FaceIndex.Top);
                                    block.SetSymbol(6, face: Block.FaceIndex.Bottom);
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
        #endregion CommandCreateBox
        #region CommandCreateBoxFrame
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
                    Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;

                    int width = 10;
                    int height = 10;
                    int depth = 10;
                    BlockType blockType = BlockType.GetBlockType("HullFullLarge", "Cube");
                    byte blockVariant = BlockType.GetVariant(blockType.Id, "Cube");
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
                                    blueprint.SetBlock(new Block(new BlockPos((byte)x, (byte)y, (byte)z)) { BlockType = blockType, Variant = blockVariant });
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
        #endregion CommandCreateBoxFrame
        #region CommandCreatePyramid
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
                    Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;

                    int width = 10;
                    int height = 10;
                    int depth = 10;
                    bool hollow = false;
                    BlockType blockType = BlockType.GetBlockType("HullFullLarge", "Cube");
                    byte blockVariant = BlockType.GetVariant(blockType.Id, "Cube");

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

                                BlockType t = blockType;
                                Block.BlockRotation r = Block.BlockRotation.PzPy;
                                byte v = blockVariant;
                                byte[] c = new byte[] { 0, 0, 0, 0, 0, 0 };

                                if (isBackEdge && isLeftEdge)
                                {
                                    t = BlockType.GetBlockType("HullFullLarge", "CornerC");
                                    v = BlockType.GetVariant(t.Id, "CornerC");
                                    r = Block.BlockRotation.PxPy;
                                }
                                else if (isBackEdge && isRightEdge)
                                {
                                    t = BlockType.GetBlockType("HullFullLarge", "CornerC");
                                    v = BlockType.GetVariant(t.Id, "CornerC");
                                    r = Block.BlockRotation.PzPy;
                                }
                                else if (isFrontEdge && isLeftEdge)
                                {
                                    t = BlockType.GetBlockType("HullFullLarge", "CornerC");
                                    v = BlockType.GetVariant(t.Id, "CornerC");
                                    r = Block.BlockRotation.NzPy;
                                }
                                else if (isFrontEdge && isRightEdge)
                                {
                                    t = BlockType.GetBlockType("HullFullLarge", "CornerC");
                                    v = BlockType.GetVariant(t.Id, "CornerC");
                                    r = Block.BlockRotation.NxPy;
                                }
                                else if (isBackEdge)
                                {
                                    t = BlockType.GetBlockType("HullFullLarge", "RampC");
                                    v = BlockType.GetVariant(t.Id, "RampC");
                                    r = Block.BlockRotation.NzPy;
                                }
                                else if (isFrontEdge)
                                {
                                    t = BlockType.GetBlockType("HullFullLarge", "RampC");
                                    v = BlockType.GetVariant(t.Id, "RampC");
                                    r = Block.BlockRotation.PzPy;
                                }
                                else if (isLeftEdge)
                                {
                                    t = BlockType.GetBlockType("HullFullLarge", "RampC");
                                    v = BlockType.GetVariant(t.Id, "RampC");
                                    r = Block.BlockRotation.NxPy;
                                }
                                else if (isRightEdge)
                                {
                                    t = BlockType.GetBlockType("HullFullLarge", "RampC");
                                    v = BlockType.GetVariant(t.Id, "RampC");
                                    r = Block.BlockRotation.PxPy;
                                }

                                if (!isInterior || !hollow)
                                {
                                    Block block = new Block(new BlockPos((byte)x, (byte)y, (byte)z)) { BlockType = t, Rotation = r, Variant = v };
                                    block.SetColour(isInterior ? ColourIndex.Pink : ColourIndex.None);
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
        #endregion CommandCreatePyramid
        #region CommandCreateCore
        public RelayCommand CommandCreateCore
        {
            get { return _commandCreateCore ?? (_commandCreateCore = new RelayCommand(() =>
            {
                if (SelectedBlueprintIndex == -1)
                {
                    return;
                }
                Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;
                BlockPos corePos = new BlockPos(0, 0, 0, 8, 8);
                blueprint.SetBlock(
                    new Block(corePos)
                    {
                        BlockType = BlockType.BlockTypes[558],
                        Variant = 0
                    });
                blueprint.DeviceCount = 1;
                blueprint.CountBlocks();
                blueprint.ComputeDimensions();

                DeviceGroup group = new DeviceGroup()
                {
                    Name = "Ungrouped",
                    DeviceGroupUnknown01 = 1,
                    Shortcut = 255,
                    DeviceGroupUnknown03 = 0
                };
                group.Entries.Add(new DeviceGroupEntry() { Pos = corePos });
                blueprint.DeviceGroups.Add(group);

                Blueprints[SelectedBlueprintIndex].UpdateViewModels();
            })); }
        }
        private RelayCommand _commandCreateCore;
        #endregion CommandCreateCore
        #region CommandCreateCoreWithLever
        public RelayCommand CommandCreateCoreWithLever
        {
            get { return _commandCreateCoreWithLever ?? (_commandCreateCoreWithLever = new RelayCommand(() =>
            {
                if (SelectedBlueprintIndex == -1)
                {
                    return;
                }
                Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;
                BlockPos leverPos = new BlockPos(0, 0, 0, 8, 8);
                BlockPos corePos = new BlockPos(0, 0, 1, 8, 8);

                blueprint.SetBlock(
                    new Block(leverPos)
                    {
                        BlockType = BlockType.BlockTypes[1262],
                        Variant = 0
                    });
                blueprint.SetBlock(
                    new Block(corePos)
                    {
                        BlockType = BlockType.BlockTypes[558],
                        Variant = 0
                    });

                blueprint.DeviceCount = 2;
                blueprint.CountBlocks();
                blueprint.ComputeDimensions();

                DeviceGroup group = new DeviceGroup()
                {
                    Name = "Ungrouped",
                    DeviceGroupUnknown01 = 1,
                    Shortcut = 255,
                    DeviceGroupUnknown03 = 0
                };
                group.Entries.Add(new DeviceGroupEntry() { Pos = corePos });
                group.Entries.Add(new DeviceGroupEntry() { Pos = leverPos });
                blueprint.DeviceGroups.Add(group);

                blueprint.SignalSources.Add(new SignalSource()
                {
                    Name = "Lever0",
                    Pos = leverPos,
                    State = 0x00020000
                });
                Blueprints[SelectedBlueprintIndex].UpdateViewModels();
            })); }
        }
        private RelayCommand _commandCreateCoreWithLever;

        #endregion CommandCreateCoreWithLever
        #region CommandCreateHullVariants
        public RelayCommand CommandCreateHullVariants
        {
            get { return _commandCreateHullVariants ?? (_commandCreateHullVariants = new RelayCommand(() =>
            {
                if (SelectedBlueprintIndex == -1)
                {
                    return;
                }
                Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;
                UInt16[] types = { 381, 382, 1791, 1833, 1846, 1859, 1872 }; // HullFullSmall, HullThinSmall, HullExtendedSmall, HullExtendedSmall2, HullExtendedSmall3, HullExtendedSmall4, HullExtendedSmall5
                byte y = 0;
                foreach (UInt16 t in types)
                {
                    int i = 0;
                    foreach (string variantName in BlockType.BlockVariants[t])
                    {
                        if (variantName != null)
                        {
                            byte x = (byte)((i % 8) * 2);
                            byte z = (byte)((i / 8) * 2);
                            BlockType bt = BlockType.BlockTypes[t];
                            byte v = BlockType.GetVariant(t, variantName);
                            Block block =
                                new Block(new BlockPos(x, y, z))
                                {
                                    BlockType = bt,
                                    Variant = v,
                                    Colours =
                                    {
                                        [0] = ColourIndex.Red,
                                        [1] = ColourIndex.BrightGreen,
                                        [2] = ColourIndex.Blue,
                                        [3] = ColourIndex.Cyan,
                                        [4] = ColourIndex.Purple,
                                        [5] = ColourIndex.Yellow
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
        #endregion CommandCreateHullVariants
        #region CommandCreateAllBlocks
        public RelayCommand CommandCreateAllBlocks
        {
            get
            {
                return _commandCreateAllBlocks ?? (_commandCreateAllBlocks = new RelayCommand(() =>
                {
                    if (SelectedBlueprintIndex == -1)
                    {
                        return;
                    }
                    Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;

                    byte x = 0;
                    byte y = 0;
                    byte z = 0;
                    byte spacing = 6;
                    foreach (BlockType blockType in BlockType.BlockTypes.Values)
                    {
                        if (BlockType.IsAllowed(blockType, blueprint.Type))
                        {
                            Block block =
                                new Block(new BlockPos(x, y, z))
                                {
                                    BlockType = blockType,
                                    Variant = 0,
                                    Colours =
                                    {
                                        [0] = ColourIndex.Red,
                                        [1] = ColourIndex.BrightGreen,
                                        [2] = ColourIndex.Blue,
                                        [3] = ColourIndex.Cyan,
                                        [4] = ColourIndex.Purple,
                                        [5] = ColourIndex.Yellow
                                    }
                                };
                            blueprint.SetBlock(block);

                            x += spacing;
                            if (x >= 20 * spacing)
                            {
                                x = 0;
                                z += spacing;
                                if (z >= 20 * spacing)
                                {
                                    z = 0;
                                    y += spacing;
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
        private RelayCommand _commandCreateAllBlocks;
        #endregion CommandCreateAllBlocks

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
            Blueprint blueprint = new Blueprint();

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
                Blueprint blueprint = OpenEpb(fileName);
                BlueprintViewModel vm = new BlueprintViewModel(fileName, blueprint);
                Blueprints.Add(vm);
                SelectedBlueprintIndex = Blueprints.Count - 1;
            }
        }
        protected Blueprint OpenEpb(string path)
        {
            Blueprint blueprint = null;

            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                try
                {
                    long bytesLeft = reader.BaseStream.Length;
                    blueprint = reader.ReadBlueprint(ref bytesLeft);
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
            BuildStructureCommands.Add(new ToolBarCommand() { Name = "All blocks",    Icon = "Empty.png",                   Command = CommandCreateAllBlocks });

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
                Blueprint blueprint = OpenEpb("C:\\Users\\fredrik\\Documents\\Projects\\Empyrion\\EmpyrionStuff\\Research\\SampleData\\v22\\BA_Logic.epb");
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