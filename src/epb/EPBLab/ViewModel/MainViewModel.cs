﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using EPBLab.Messages;
using GalaSoft.MvvmLight;
using EPBLab.Model;
using GalaSoft.MvvmLight.Command;
using EPBLib;
using EPBLib.BlockData;
using EPBLib.Helpers;
using EPBLib.Logic;
using GalaSoft.MvvmLight.Messaging;
using EPBLab.ViewModel.ToolbarCommands;

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
        #region Window
        public const string MainWindowTitlePropertyName = "MainWindowTitle";
        private string _mainWindowTitle = string.Empty;
        public string MainWindowTitle
        {
            get => _mainWindowTitle;
            set => Set(ref _mainWindowTitle, value);
        }
        #endregion Window
        #region Blueprints
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
        #endregion Blueprints
        #region ToolbarCommandProperties
        public const string ShowBuildStructuresPropertyName = "ShowBuildStructures";

        public Visibility ShowBuildStructures => SelectedBlueprintIndex != -1 ? Visibility.Visible : Visibility.Collapsed;

        public const string BuildStructureCommandsPropertyName = "BuildStructureCommands";
        private ObservableCollection<Command> _buildStructureCommands = new ObservableCollection<Command>();
        public ObservableCollection<Command> BuildStructureCommands
        {
            get => _buildStructureCommands;
            set => Set(ref _buildStructureCommands, value);
        }

        public const string CurrentCommandPropertyName = "CurrentCommand";
        private Command _currentCommand = null;
        public Command CurrentCommand
        {
            get => _currentCommand;
            set
            {
                Set(ref _currentCommand, value);
                RaisePropertyChanged(ShowCommandParametersPropertyName);
            }
        }

        public const string ShowCommandParametersPropertyName = "ShowCommandParameters";
        public Visibility ShowCommandParameters => CurrentCommand != null ? Visibility.Visible : Visibility.Collapsed;
        #endregion ToolbarCommandProperties
        #region Progress
        public string ProgressDescription
        {
            get { return _progressDescription; }
            set { Set(ref _progressDescription, value); }
        }
        private string _progressDescription;
        public int ProgressCurrent
        {
            get { return _progressCurrent; }
            set { Set(ref _progressCurrent, value); }
        }
        private int _progressCurrent;
        public string ProgressElapsed
        {
            get { return _progressElapsed; }
            set { Set(ref _progressElapsed, value); }
        }
        private string _progressElapsed;
        public string ProgressRemaining
        {
            get { return _progressRemaining; }
            set { Set(ref _progressRemaining, value); }
        }
        private string _progressRemaining;
        public Visibility ProgressVisibility
        {
            get { return _progressVisibility; }
            set { Set(ref _progressVisibility, value); }
        }
        private Visibility _progressVisibility;
        #endregion Progress
        #endregion Properties

        #region Commands
        #region Command_New
        public RelayCommand CommandNew => _commandNew ?? (_commandNew = new RelayCommand(NewBlueprint));
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

                    ParameterIntVector originParameter     = (ParameterIntVector)CurrentCommand.ParameterByName("Origin");
                    ParameterIntVector sizeParameter       = (ParameterIntVector)CurrentCommand.ParameterByName("Size");
                    ParameterIntVector holeSizeParameter   = (ParameterIntVector)CurrentCommand.ParameterByName("Hole size");
                    ParameterBool      hollowParameter     =      (ParameterBool)CurrentCommand.ParameterByName("Hollow");
                    ParameterBool      thickShellParameter =      (ParameterBool)CurrentCommand.ParameterByName("Thick shell");
                    ParameterBool      toplessParameter    =      (ParameterBool)CurrentCommand.ParameterByName("Topless");

                    int    width      = sizeParameter.X;
                    int    height     = sizeParameter.Y;
                    int    depth      = sizeParameter.Z;
                    int    holeWidth  = holeSizeParameter.X;
                    int    holeHeight = holeSizeParameter.Y;
                    int    holeDepth  = holeSizeParameter.Z;
                    bool   hollow     = hollowParameter.IsTrue;
                    bool   thickShell = thickShellParameter.IsTrue;
                    bool   topless    = toplessParameter.IsTrue;
                    string typeName   = GetDefaultBuildingBlockTypeName(blueprint.Type);

                    BlockType blockType = BlockType.GetBlockType(typeName, "Cube");
                    byte blockVariant = BlockType.GetVariant(blockType.Id, "Cube");

                    int iMin = (int)(width  / 2f - holeWidth  / 2f + 0.5f);
                    int iMax = (int)(width  / 2f + holeWidth  / 2f + 0.5f);
                    int jMin = (int)(height / 2f - holeHeight / 2f + 0.5f);
                    int jMax = (int)(height / 2f + holeHeight / 2f + 0.5f);
                    int kMin = (int)(depth  / 2f - holeDepth  / 2f + 0.5f);
                    int kMax = (int)(depth  / 2f + holeDepth  / 2f + 0.5f);

                    BlockList blocks = CreateStructure (
                        (byte)(originParameter.X),
                        (byte)(originParameter.Y),
                        (byte)(originParameter.Z),
                        width,
                        height,
                        depth,
                        (i, j, k) =>
                        {
                            if (   (i >= iMin && i < iMax)
                                   && (j >= jMin && j < jMax)
                                   && (k >= kMin && k < kMax))
                            {
                                return null;
                            }
                            Block block = new Block()
                            {
                                BlockType = blockType,
                                Variant = blockVariant
                            };
                            block.SetTexture(14, (i % 2) == 1);
                            block.SetSymbol(1, (Block.SymbolRotation)(i % 4), Block.FaceIndex.Back);
                            block.SetSymbol(2, face: Block.FaceIndex.Right);
                            block.SetSymbol(3, face: Block.FaceIndex.Front);
                            block.SetSymbol(4, face: Block.FaceIndex.Left);
                            block.SetSymbol(5, face: Block.FaceIndex.Top);
                            block.SetSymbol(6, face: Block.FaceIndex.Bottom);
                            return block;
                        });

                    blocks = ModifyInterior(blocks, (l, block) =>
                    {
                        if (hollow)
                        {
                            l.Remove(block);
                        }
                        else
                        {
                            l[block.Position].SetColour(ColourIndex.Pink);
                        }
                    }, thickShell);

                    if (topless)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            for (int y = height / 2 + 1; y < height; y++)
                            {
                                for (int z = 0; z < depth; z++)
                                {
                                    blocks.Remove(
                                        (byte)(x + originParameter.X),
                                        (byte)(y + originParameter.Y),
                                        (byte)(z + originParameter.Z));
                                }
                            }
                        }
                    }

                    blueprint.SetBlock(blocks);
                    blueprint.CountBlocks();
                    blueprint.ComputeDimensions();

                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                    CurrentCommand = null;
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

                    ParameterIntVector originParameter = (ParameterIntVector)CurrentCommand.ParameterByName("Origin");
                    ParameterIntVector sizeParameter   = (ParameterIntVector)CurrentCommand.ParameterByName("Size");

                    int width  = sizeParameter.X;
                    int height = sizeParameter.Y;
                    int depth  = sizeParameter.Z;
                    string typeName = GetDefaultBuildingBlockTypeName(blueprint.Type);

                    BlockType blockType = BlockType.GetBlockType(typeName, "Cube");
                    byte blockVariant = BlockType.GetVariant(blockType.Id, "Cube");

                    BlockList blocks = CreateStructure(
                        (byte)(originParameter.X),
                        (byte)(originParameter.Y),
                        (byte)(originParameter.Z),
                        width,
                        height,
                        depth,
                        (i, j, k) =>
                        {
                            bool a = i % (width  - 1) == 0;
                            bool b = j % (height - 1) == 0;
                            bool c = k % (depth  - 1) == 0;
                            int d = (a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0);
                            if (d < 2)
                            {
                                return null;
                            }
                            return new Block()
                            {
                                BlockType = blockType,
                                Variant = blockVariant
                            };
                        });

                    blueprint.SetBlock(blocks);
                    blueprint.CountBlocks();
                    blueprint.ComputeDimensions();

                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                    CurrentCommand = null;
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

                    ParameterIntVector originParameter = (ParameterIntVector)CurrentCommand.ParameterByName("Origin");
                    ParameterInt sizeParameter = (ParameterInt)CurrentCommand.ParameterByName("Size");
                    ParameterBool hollowParameter = (ParameterBool)CurrentCommand.ParameterByName("Hollow");

                    int width = sizeParameter.Value;
                    bool hollow = hollowParameter.IsTrue;
                    string typeName = GetDefaultBuildingBlockTypeName(blueprint.Type);
                    BlockType blockType = BlockType.GetBlockType(typeName, "Cube");
                    byte blockVariant = BlockType.GetVariant(blockType.Id, "Cube");

                    bool cap = width % 2 == 1;

                    BlockType t;
                    Block.BlockRotation r;
                    byte v;
                    byte[] c = new byte[] { 0, 0, 0, 0, 0, 0 };

                    int y = 0;
                    while (width > 1)
                    {
                        for (int z = 0; z < width; z++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                bool isBackEdge = (z == 0);
                                bool isFrontEdge = (z == (width - 1));
                                bool isLeftEdge = (x == 0);
                                bool isRightEdge = (x == (width - 1));
                                bool isInterior = !isBackEdge && !isFrontEdge && !isRightEdge && !isLeftEdge && y > 0;

                                t = blockType;
                                r = Block.BlockRotation.PzPy;
                                v = blockVariant;

                                if (isBackEdge && isLeftEdge)
                                {
                                    t = BlockType.GetBlockType(typeName, "CornerC");
                                    v = BlockType.GetVariant(t.Id, "CornerC");
                                    r = Block.BlockRotation.PxPy;
                                }
                                else if (isBackEdge && isRightEdge)
                                {
                                    t = BlockType.GetBlockType(typeName, "CornerC");
                                    v = BlockType.GetVariant(t.Id, "CornerC");
                                    r = Block.BlockRotation.PzPy;
                                }
                                else if (isFrontEdge && isLeftEdge)
                                {
                                    t = BlockType.GetBlockType(typeName, "CornerC");
                                    v = BlockType.GetVariant(t.Id, "CornerC");
                                    r = Block.BlockRotation.NzPy;
                                }
                                else if (isFrontEdge && isRightEdge)
                                {
                                    t = BlockType.GetBlockType(typeName, "CornerC");
                                    v = BlockType.GetVariant(t.Id, "CornerC");
                                    r = Block.BlockRotation.NxPy;
                                }
                                else if (isBackEdge)
                                {
                                    t = BlockType.GetBlockType(typeName, "RampC");
                                    v = BlockType.GetVariant(t.Id, "RampC");
                                    r = Block.BlockRotation.NzPy;
                                }
                                else if (isFrontEdge)
                                {
                                    t = BlockType.GetBlockType(typeName, "RampC");
                                    v = BlockType.GetVariant(t.Id, "RampC");
                                    r = Block.BlockRotation.PzPy;
                                }
                                else if (isLeftEdge)
                                {
                                    t = BlockType.GetBlockType(typeName, "RampC");
                                    v = BlockType.GetVariant(t.Id, "RampC");
                                    r = Block.BlockRotation.NxPy;
                                }
                                else if (isRightEdge)
                                {
                                    t = BlockType.GetBlockType(typeName, "RampC");
                                    v = BlockType.GetVariant(t.Id, "RampC");
                                    r = Block.BlockRotation.PxPy;
                                }

                                if (!isInterior || !hollow)
                                {
                                    Block block = new Block((byte)(x + originParameter.X + y),
                                        (byte)(y + originParameter.Y),
                                        (byte)(z + originParameter.Z + y))
                                    {
                                        BlockType = t,
                                        Rotation = r,
                                        Variant = v
                                    };
                                    block.SetColour(isInterior ? ColourIndex.Pink : ColourIndex.None);
                                    blueprint.SetBlock(block);
                                }
                            }
                        }
                        width -= 2;
                        y++;
                    }
                    if (cap)
                    {
                        t = BlockType.GetBlockType(typeName, "PyramidA");
                        v = BlockType.GetVariant(t.Id, "PyramidA");
                        r = Block.BlockRotation.PxPy;
                        Block block = new Block((byte)(y + originParameter.X),
                            (byte)(y + originParameter.Y),
                            (byte)(y + originParameter.Z))
                        {
                            BlockType = t,
                            Rotation = r,
                            Variant = v
                        };
                        blueprint.SetBlock(block);
                    }
                    blueprint.CountBlocks();
                    blueprint.ComputeDimensions();

                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                    CurrentCommand = null;
                }));
            }
        }
        private RelayCommand _commandCreatePyramid;
        #endregion CommandCreatePyramid
        #region CommandCreateSphere
        public RelayCommand CommandCreateSphere
        {
            get
            {
                return _commandCreateSphere ?? (_commandCreateSphere = new RelayCommand(() =>
                {
                    if (SelectedBlueprintIndex == -1)
                    {
                        return;
                    }

                    Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;

                    ParameterIntVector originParameter       = (ParameterIntVector)CurrentCommand.ParameterByName("Origin");
                    ParameterInt       diameterParameter     =       (ParameterInt)CurrentCommand.ParameterByName("Diameter");
                    ParameterInt       holeDiameterParameter =       (ParameterInt)CurrentCommand.ParameterByName("Hole diameter");
                    ParameterBool      hollowParameter       =      (ParameterBool)CurrentCommand.ParameterByName("Hollow");
                    ParameterBool      thickShellParameter   =      (ParameterBool)CurrentCommand.ParameterByName("Thick shell");
                    ParameterBool      toplessParameter      =      (ParameterBool)CurrentCommand.ParameterByName("Topless");

                    int    diameter     = diameterParameter.Value;
                    int    holeDiameter = holeDiameterParameter.Value;
                    bool   hollow       = hollowParameter.IsTrue;
                    bool   thickShell   = thickShellParameter.IsTrue;
                    bool   topless      = toplessParameter.IsTrue;
                    string typeName     = GetDefaultBuildingBlockTypeName(blueprint.Type);

                    BlockType blockType = BlockType.GetBlockType(typeName, "Cube");
                    byte blockVariant = BlockType.GetVariant(blockType.Id, "Cube");

                    float oSquared = diameter * diameter / 4f;
                    float iSquared = holeDiameter * holeDiameter / 4f;

                    BlockList blocks = CreateStructure(
                        (byte)(originParameter.X),
                        (byte)(originParameter.Y),
                        (byte)(originParameter.Z),
                        diameter,
                        diameter,
                        diameter,
                        (i, j, k) =>
                        {
                            float x = i - diameter / 2f + 0.5f;
                            float y = j - diameter / 2f + 0.5f;
                            float z = k - diameter / 2f + 0.5f;
                            float dSquared = x * x + y * y + z * z;
                            if (dSquared > oSquared || dSquared < iSquared)
                            {
                                return null;
                            }
                            return new Block()
                            {
                                BlockType = blockType,
                                Variant = blockVariant
                            };
                        });

                    blocks = ModifyInterior(blocks, (l, block) =>
                    {
                        if (hollow)
                        {
                            l.Remove(block);
                        }
                        else
                        {
                            l[block.Position].SetColour(ColourIndex.Pink);
                        }
                    }, thickShell);

                    if (topless)
                    {
                        for (int x = 0; x < diameter; x++)
                        {
                            for (int y = diameter / 2 + 1; y < diameter; y++)
                            {
                                for (int z = 0; z < diameter; z++)
                                {
                                    blocks.Remove(
                                        (byte)(x + originParameter.X),
                                        (byte)(y + originParameter.Y),
                                        (byte)(z + originParameter.Z));
                                }
                            }
                        }
                    }

                    blueprint.SetBlock(blocks);
                    blueprint.CountBlocks();
                    blueprint.ComputeDimensions();

                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                    CurrentCommand = null;
                }));
            }
        }
        private RelayCommand _commandCreateSphere;
        #endregion CommandCreateSphere
        #region CommandCreateCylinder
        public RelayCommand CommandCreateCylinder
        {
            get
            {
                return _commandCreateCylinder ?? (_commandCreateCylinder = new RelayCommand(() =>
                {
                    if (SelectedBlueprintIndex == -1)
                    {
                        return;
                    }

                    Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;

                    ParameterIntVector originParameter       = (ParameterIntVector)CurrentCommand.ParameterByName("Origin");
                    ParameterInt       diameterParameter     =       (ParameterInt)CurrentCommand.ParameterByName("Diameter");
                    ParameterInt       holeDiameterParameter =       (ParameterInt)CurrentCommand.ParameterByName("Hole diameter");
                    ParameterInt       heightParameter       =       (ParameterInt)CurrentCommand.ParameterByName("Height");
                    ParameterBool      hollowParameter       =      (ParameterBool)CurrentCommand.ParameterByName("Hollow");
                    ParameterBool      thickShellParameter   =      (ParameterBool)CurrentCommand.ParameterByName("Thick shell");
                    ParameterBool      toplessParameter      =      (ParameterBool)CurrentCommand.ParameterByName("Topless");

                    int    diameter     = diameterParameter.Value;
                    int    holeDiameter = holeDiameterParameter.Value;
                    int    height       = heightParameter.Value;
                    bool   hollow       = hollowParameter.IsTrue;
                    bool   thickShell   = thickShellParameter.IsTrue;
                    bool   topless      = toplessParameter.IsTrue;
                    string typeName     = GetDefaultBuildingBlockTypeName(blueprint.Type);

                    BlockType blockType = BlockType.GetBlockType(typeName, "Cube");
                    byte blockVariant = BlockType.GetVariant(blockType.Id, "Cube");

                    float oSquared = diameter * diameter / 4f;
                    float iSquared = holeDiameter * holeDiameter / 4f;

                    BlockList blocks = CreateStructure(
                        (byte)(originParameter.X),
                        (byte)(originParameter.Y),
                        (byte)(originParameter.Z),
                        diameter,
                        diameter,
                        height,
                        (i, j, k) =>
                        {
                            float x = i - diameter / 2f + 0.5f;
                            float z = k - diameter / 2f + 0.5f;
                            float dSquared = x * x + z * z;
                            //Console.WriteLine($"{i}, {j}, {k} => {x}, {z} => {dSquared}, {oSquared}, {iSquared} => {dSquared > oSquared || dSquared < iSquared}");
                            if (dSquared > oSquared || dSquared < iSquared)
                            {
                                return null;
                            }
                            return new Block()
                            {
                                BlockType = blockType,
                                Variant = blockVariant
                            };
                        });


                    blocks = ModifyInterior(blocks, (l, block) =>
                    {
                        if (hollow)
                        {
                            l.Remove(block);
                        }
                        else
                        {
                            l[block.Position].SetColour(ColourIndex.Pink);
                        }
                    }, thickShell);

                    if (topless)
                    {
                        for (int x = 0; x < diameter; x++)
                        {
                            for (int y = height / 2 + 1; y < height; y++)
                            {
                                for (int z = 0; z < diameter; z++)
                                {
                                    blocks.Remove(
                                        (byte)(x + originParameter.X),
                                        (byte)(y + originParameter.Y),
                                        (byte)(z + originParameter.Z));
                                }
                            }
                        }
                    }

                    blueprint.SetBlock(blocks);
                    blueprint.CountBlocks();
                    blueprint.ComputeDimensions();

                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                    CurrentCommand = null;
                }));
            }
        }
        private RelayCommand _commandCreateCylinder;
        #endregion CommandCreateCylinder
        #region CommandCreateCore
        public RelayCommand CommandCreateCore
        {
            get 
            {
                return _commandCreateCore ?? (_commandCreateCore =  new RelayCommand(() =>
                {
                    if (SelectedBlueprintIndex == -1)
                    {
                        return;
                    }
                    Blueprint blueprint = Blueprints[SelectedBlueprintIndex].Blueprint;
                    ParameterIntVector originParameter = (ParameterIntVector)CurrentCommand.ParameterByName("Origin");
                    if (originParameter == null)
                    {
                        return; // TODO: should we also cancel here?
                    }

                    BlockPos corePos = new BlockPos((byte)originParameter.X, (byte)originParameter.Y, (byte)originParameter.Z, 8, 8);
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
                    @group.Entries.Add(new DeviceGroupEntry() { Pos = corePos });
                    blueprint.DeviceGroups.Add(@group);

                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                    CurrentCommand = null;
                }));
            }
        }
        private RelayCommand _commandCreateCore;
        #endregion CommandCreateCore
        #region CommandCreateCoreWithLever
        public RelayCommand CommandCreateCoreWithLever
        {
            get
            {
                return _commandCreateCoreWithLever ?? (_commandCreateCoreWithLever = new RelayCommand(() =>
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
                    @group.Entries.Add(new DeviceGroupEntry() { Pos = corePos });
                    @group.Entries.Add(new DeviceGroupEntry() { Pos = leverPos });
                    blueprint.DeviceGroups.Add(@group);

                    blueprint.SignalSources.Add(new SignalSource()
                    {
                        Name = "Lever0",
                        Pos = leverPos,
                        State = 0x00020000
                    });
                    Blueprints[SelectedBlueprintIndex].UpdateViewModels();
                    CurrentCommand = null;
                }));
            }
        }
        private RelayCommand _commandCreateCoreWithLever;

        #endregion CommandCreateCoreWithLever
        #region CommandCreateHullVariants
        public RelayCommand CommandCreateHullVariants
        {
            get
            {
                return _commandCreateHullVariants ?? (_commandCreateHullVariants = new RelayCommand(() =>
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
                                    new Block(x, y, z)
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
                    CurrentCommand = null;
                }));
            }
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
                                new Block(x, y, z)
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
                    CurrentCommand = null;
                }));
            }
        }
        private RelayCommand _commandCreateAllBlocks;
        #endregion CommandCreateAllBlocks

        #region Command_Select
        public RelayCommand<Command> CommandSelect
        {
            get { return _commandSelect ?? (_commandSelect = new RelayCommand<Command>((command) => { CurrentCommand = command; })); }
        }
        private RelayCommand<Command> _commandSelect;
        #endregion Command_Select
        #region Command_Cancel
        public RelayCommand CommandCancel
        {
            get { return _commandCancel ?? (_commandCancel = new RelayCommand(() => { CurrentCommand = null; })); }
        }
        private RelayCommand _commandCancel;
        #endregion Command_Cancel
        #endregion Commands

        #region MessageHandlers
        #region NewBlueprint
        protected void NewBlueprint()
        {
            Blueprint blueprint = new Blueprint();

            BlueprintViewModel vm = new BlueprintViewModel("New", blueprint);
            Blueprints.Add(vm);
            SelectedBlueprintIndex = Blueprints.Count - 1;
        }
        #endregion NewBlueprint
        #region OpenBlueprints
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
        #endregion OpenBlueprints
        #region OpenEpb
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
        #endregion OpenEpb
        #region SaveBlueprint
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
        #endregion SaveBlueprint
        #region CloseBlueprint
        protected void CloseBlueprint(CloseBlueprintMessage m)
        {
            if (Blueprints.Contains(m.Content))
            {
                Blueprints.Remove(m.Content);
            }
        }
        #endregion CloseBlueprint
        #region UpdateProgress
        protected void UpdateProgress(ProgressUpdateMessage m)
        {
            float progress = m.Content.Progress;

            DateTime now = DateTime.Now;
            if (progress == 0f)
            {
                ProgressElapsed = "";
                ProgressRemaining = "";
                ProgressVisibility = Visibility.Visible;
                ProgressStopwatch.Reset();
                ProgressStopwatch.Start();
            }
            else if (progress >= 1f)
            {
                ProgressStopwatch.Stop();
                ProgressElapsed = "";
                ProgressRemaining = "";
                ProgressVisibility = Visibility.Hidden;
            }
            else
            {
                float elapsedTime = ProgressStopwatch.ElapsedMilliseconds;
                TimeSpan e = TimeSpan.FromMilliseconds(elapsedTime);
                ProgressElapsed = $"Elapsed: {TimeSpan2String(e)}";
                if (elapsedTime > 5000f)
                {
                    TimeSpan remainingTime = TimeSpan.FromMilliseconds((elapsedTime / progress) * (1f - progress));
                    ProgressRemaining = $" Remaining: {TimeSpan2String(remainingTime)}";
                }
            }
            ProgressDescription = m.Content.Description;
            ProgressCurrent = (int)(m.Content.Progress * 100);
        }
        protected Stopwatch ProgressStopwatch = new Stopwatch();
        #endregion UpdateProgress
        #endregion MessageHandlers

        #region ConversionHelpers
        protected string TimeSpan2String(TimeSpan t)
        {
            string s = "";
            if (t.Days    > 0) s += $"{t.Days} day{      (t.Days    > 1 ? "s" : "")} ";
            if (t.Hours   > 0) s += $"{t.Hours} hour{    (t.Hours   > 1 ? "s" : "")} ";
            if (t.Minutes > 0) s += $"{t.Minutes} minute{(t.Minutes > 1 ? "s" : "")} ";
            if (t.Seconds > 0) s += $"{t.Seconds} second{(t.Seconds > 1 ? "s" : "")} ";
            if (s == "") s = "<1 second";
            return s;
        }
        #endregion ConversionHelpers

        #region CreationHelpers
        protected string GetDefaultBuildingBlockTypeName(BlueprintType type)
        {
            switch (type)
            {
                case BlueprintType.Voxel:
                    return "HullFullLarge";
                case BlueprintType.Base:
                    return "HullFullLarge";
                case BlueprintType.SmallVessel:
                    return "HullFullSmall";
                case BlueprintType.CapitalVessel:
                    return "HullFullLarge";
                case BlueprintType.HoverVessel:
                    return "HullFullSmall";
            }
            return "HullFullLarge";
        }

        protected BlockList CreateStructure(
            byte xOffset, byte yOffset, byte zOffset,
            int width,   int height,  int depth,
            Func<byte, byte, byte, Block> testPosition)
        {
            BlockList blocks = new BlockList();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        Block block = testPosition((byte)x, (byte)y, (byte)z);
                        if (block != null)
                        {
                            BlockPos pos = new BlockPos((byte)(x + xOffset), (byte)(y + yOffset), (byte)(z + zOffset));
                            block.SetPosition(pos);
                            blocks[pos] = block;
                        }
                    }
                }
            }

            return blocks;
        }

        protected BlockList ModifyInterior(BlockList srcBlocks, Action<BlockList, Block> modifyInterior, bool thickShell = false)
        {
            BlockList blocks = new BlockList();
            foreach (Block block in srcBlocks)
            {
                if (block == null)
                {
                    continue;
                }
                BlockPos pos = block.Position;
                blocks[pos] = block;
                bool isInterior = IsInterior(srcBlocks, pos, thickShell);
                if (isInterior)
                {
                    modifyInterior(blocks, block);
                }
            }
            return blocks;
        }

        protected bool IsInterior(BlockList blocks, BlockPos pos, bool thickShell = true)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if ((!thickShell && (Math.Abs(i) + Math.Abs(j) + Math.Abs(k) != 1)) || (i == 0 && j == 0 && k == 0))
                        {
                            continue;
                        }

                        if (blocks[(byte)(pos.X + i),
                                (byte)(pos.Y + j),
                                (byte)(pos.Z + k)] == null)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        #endregion CreationHelpers

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            SetMainWindowTitle();

            #region BuildStructureCommands
            #region Box
            BuildStructureCommands.Add(new Command()
            {
                Name = "Box",
                Icon = "BuildStructure/Box.png",
                Parameters = new List<Parameter>()
                {
                    new ParameterIntVector()
                    {
                        Name = "Origin",
                        Description = "Places the structure in the blueprint"
                    },
                    new ParameterIntVector()
                    {
                        Name = "Size",
                        Description = "Width, height and depth of the new structure",
                        X = 10,
                        Y = 10,
                        Z = 10
                    },
                    new ParameterIntVector()
                    {
                        Name = "Hole size",
                        Description = "Cut a box hole at the centre",
                        X = 0,
                        Y = 0,
                        Z = 0
                    },
                    new ParameterBool()
                    {
                        Name = "Hollow",
                        Description = "Make the box hollow",
                        IsTrue = false
                    },
                    new ParameterBool()
                    {
                        Name = "Thick shell",
                        Description = "Include diagonals in interior check to make the shell thicker",
                        IsTrue = true
                    },
                    new ParameterBool()
                    {
                        Name = "Topless",
                        Description = "Cut off top half",
                        IsTrue = false
                    }
                },
                Accept = CommandCreateBox,
                Select = CommandSelect,
                Cancel = CommandCancel
            });
            #endregion Box
            #region BoxFrame
            BuildStructureCommands.Add(new Command()
            {
                Name = "Box frame",
                Icon = "BuildStructure/BoxFrame.png",
                Parameters = new List<Parameter>()
                {
                    new ParameterIntVector()
                    {
                        Name = "Origin",
                        Description = "Places the structure in the blueprint"
                    },
                    new ParameterIntVector()
                    {
                        Name = "Size",
                        Description = "Width, height and depth of the new structure",
                        X = 10,
                        Y = 10,
                        Z = 10
                    }
                },
                Accept = CommandCreateBoxFrame,
                Select = CommandSelect,
                Cancel = CommandCancel
            });
            #endregion BoxFrame
            #region Pyramid
            BuildStructureCommands.Add(new Command()
            {
                Name = "Pyramid",
                Icon = "BuildStructure/Pyramid.png",
                Parameters = new List<Parameter>()
                {
                    new ParameterIntVector()
                    {
                        Name = "Origin",
                        Description = "Places the structure in the blueprint"
                    },
                    new ParameterInt()
                    {
                        Name = "Size",
                        Description = "Number of blocks in the side edge of the base square",
                        Value = 10
                    },
                    new ParameterBool()
                    {
                        Name = "Hollow",
                        Description = "Make the pyramid hollow",
                        IsTrue = false
                    }
                },
                Accept = CommandCreatePyramid,
                Select = CommandSelect,
                Cancel = CommandCancel
            });
            #endregion Pyramid
            #region Sphere
            BuildStructureCommands.Add(new Command()
            {
                Name = "Sphere",
                Icon = "BuildStructure/Sphere.png",
                Parameters = new List<Parameter>()
                {
                    new ParameterIntVector()
                    {
                        Name = "Origin",
                        Description = "Places the structure in the blueprint"
                    },
                    new ParameterInt()
                    {
                        Name = "Diameter",
                        Description = "Number of blocks",
                        Value = 21
                    },
                    new ParameterInt()
                    {
                        Name = "Hole diameter",
                        Description = "Cut a spherical hole at the centre",
                        Value = 0
                    },
                    new ParameterBool()
                    {
                        Name = "Hollow",
                        Description = "Make the sphere hollow",
                        IsTrue = false
                    },
                    new ParameterBool()
                    {
                        Name = "Thick shell",
                        Description = "Include diagonals in interior check to make the shell thicker",
                        IsTrue = false
                    },
                    new ParameterBool()
                    {
                        Name = "Topless",
                        Description = "Cut off top half",
                        IsTrue = false
                    }
                },
                Accept = CommandCreateSphere,
                Select = CommandSelect,
                Cancel = CommandCancel
            });
            #endregion Sphere
            #region Cylinder
            BuildStructureCommands.Add(new Command()
            {
                Name = "Cylinder",
                Icon = "BuildStructure/Cylinder.png",
                Parameters = new List<Parameter>()
                {
                    new ParameterIntVector()
                    {
                        Name = "Origin",
                        Description = "Places the structure in the blueprint"
                    },
                    new ParameterInt()
                    {
                        Name = "Diameter",
                        Description = "Number of blocks",
                        Value = 21
                    },
                    new ParameterInt()
                    {
                        Name = "Hole diameter",
                        Description = "Cut a cylindrical hole at the centre",
                        Value = 0
                    },
                    new ParameterInt()
                    {
                        Name = "Height",
                        Description = "Number of blocks",
                        Value = 21
                    },
                    new ParameterBool()
                    {
                        Name = "Hollow",
                        Description = "Make the cylinder hollow",
                        IsTrue = false
                    },
                    new ParameterBool()
                    {
                        Name = "Thick shell",
                        Description = "Include diagonals in interior check to make the shell thicker",
                        IsTrue = false
                    },
                    new ParameterBool()
                    {
                        Name = "Topless",
                        Description = "Cut off top half",
                        IsTrue = false
                    }
                },
                Accept = CommandCreateCylinder,
                Select = CommandSelect,
                Cancel = CommandCancel
            });
            #endregion Cylinder
            #region Core
            BuildStructureCommands.Add(new Command()
            {
                Name = "Core",
                Icon = "Empty.png",
                Parameters = new List<Parameter>()
                {
                    new ParameterIntVector()
                    {
                        Name = "Origin",
                        Description = "Places the structure in the blueprint"
                    }
                },
                Accept = CommandCreateCore,
                Select = CommandSelect,
                Cancel = CommandCancel
            });
            #endregion Core
            BuildStructureCommands.Add(new Command() { Name = "Core + lever", Icon = "Empty.png", Accept = CommandCreateCoreWithLever, Select = CommandSelect, Cancel = CommandCancel });
            BuildStructureCommands.Add(new Command() { Name = "Hull variants", Icon = "Empty.png", Accept = CommandCreateHullVariants, Select = CommandSelect, Cancel = CommandCancel });
            BuildStructureCommands.Add(new Command() { Name = "All blocks", Icon = "Empty.png", Accept = CommandCreateAllBlocks, Select = CommandSelect, Cancel = CommandCancel });
            #endregion BuildStructureCommands

            Messenger.Default.Register<FilesOpenedMessage>(this, OpenBlueprints);
            Messenger.Default.Register<SaveFileSelectedMessage>(this, SaveBlueprint);
            Messenger.Default.Register<CloseBlueprintMessage>(this, CloseBlueprint);
            Messenger.Default.Register<ProgressUpdateMessage>(this, UpdateProgress);

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