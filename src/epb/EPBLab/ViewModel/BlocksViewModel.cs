using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using EPBLab.Helpers;
using EPBLab.ViewModel.BlockMeshes;
using EPBLab.ViewModel.Tree;
using EPBLib;
using EPBLib.BlockData;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace EPBLab.ViewModel
{
    public class BlocksViewModel : ViewModelBase
    {
        #region Static
        protected static Dictionary<EpbBlock.EpbBlockRotation, Quaternion> Rotation = new Dictionary<EpbBlock.EpbBlockRotation, Quaternion>()
        {
            // FwdUp : P=Positive, N=Negative
            {EpbBlock.EpbBlockRotation.PzPy, Euler2Quaternion(  0,   0,   0)},
            {EpbBlock.EpbBlockRotation.PxPy, Euler2Quaternion(  0, -90,   0)},
            {EpbBlock.EpbBlockRotation.NzPy, Euler2Quaternion(  0, 180,   0)},
            {EpbBlock.EpbBlockRotation.NxPy, Euler2Quaternion(  0,  90,   0)},
            {EpbBlock.EpbBlockRotation.PzPx, Euler2Quaternion(  0,   0, 180)},
            {EpbBlock.EpbBlockRotation.PyPx, Euler2Quaternion(-90, -90, -90)},
            {EpbBlock.EpbBlockRotation.NzPx, Euler2Quaternion(  0, 180, 180)},
            {EpbBlock.EpbBlockRotation.NyPx, Euler2Quaternion( 90,  90, -90)},
            {EpbBlock.EpbBlockRotation.NzNy, Euler2Quaternion( 90, 180,   0)},
            {EpbBlock.EpbBlockRotation.NxNy, Euler2Quaternion( 90, 180,  90)},
            {EpbBlock.EpbBlockRotation.PzNy, Euler2Quaternion( 90, 180, 180)},
            {EpbBlock.EpbBlockRotation.PxNy, Euler2Quaternion( 90, 180, -90)},
            {EpbBlock.EpbBlockRotation.PzNx, Euler2Quaternion(  0,   0,  90)},
            {EpbBlock.EpbBlockRotation.PyNx, Euler2Quaternion(180, -90, -90)},
            {EpbBlock.EpbBlockRotation.NzNx, Euler2Quaternion(180,   0, -90)},
            {EpbBlock.EpbBlockRotation.NyNx, Euler2Quaternion(180,  90, -90)},
            {EpbBlock.EpbBlockRotation.PyNz, Euler2Quaternion( 90,   0,   0)},
            {EpbBlock.EpbBlockRotation.PxNz, Euler2Quaternion( 90,   0, -90)},
            {EpbBlock.EpbBlockRotation.NyNz, Euler2Quaternion( 90,   0, 180)},
            {EpbBlock.EpbBlockRotation.NxNz, Euler2Quaternion( 90,   0,  90)},
            {EpbBlock.EpbBlockRotation.NxPz, Euler2Quaternion(  0,   0, -90)},
            {EpbBlock.EpbBlockRotation.NyPz, Euler2Quaternion(  0, -90, -90)},
            {EpbBlock.EpbBlockRotation.PxPz, Euler2Quaternion(  0, 180, -90)},
            {EpbBlock.EpbBlockRotation.PyPz, Euler2Quaternion(  0,  90, -90)}
        };

        protected static Quaternion Euler2Quaternion(double x, double y, double z)
        {
            return   new Quaternion(new Vector3D(0, 0, 1), z)
                   * new Quaternion(new Vector3D(0, 1, 0), y)
                   * new Quaternion(new Vector3D(1, 0, 0), x);
        }

        #endregion Static

        #region Fields
        protected EpBlueprint Blueprint;
        protected List<BlockNode> BlockNodes = new List<BlockNode>();
        protected BitmapSource PaletteImageSource;
        protected int PaletteResolution = 10;
        #endregion Fields

        #region Properties
        public const string BlocksPropertyName = "BlockCategories";
        private ObservableCollection<GroupNode> _blockCategories = new ObservableCollection<GroupNode>();
        public ObservableCollection<GroupNode> BlockCategories
        {
            get => _blockCategories;
            set => Set(ref _blockCategories, value);
        }

        public const string SelectedBlocksPropertyName = "SelectedBlocks";
        private ObservableCollection<ITreeNode> _selectedBlocks;
        public ObservableCollection<ITreeNode> SelectedBlocks
        {
            get => _selectedBlocks;
            set
            {
                Set(ref _selectedBlocks, value);

                BuildSelectionModel();
            } 
        }

        public const string CameraPositionPropertyName = "CameraPosition";
        private Point3D _cameraPosition;
        public Point3D CameraPosition
        {
            get => _cameraPosition;
            set => Set(ref _cameraPosition, value);
        }

        public const string CameraAimPointPropertyName = "CameraAimPoint";
        private Point3D _cameraAimPoint;
        public Point3D CameraAimPoint
        {
            get => _cameraAimPoint;
            set => Set(ref _cameraAimPoint, value);
        }



        public const string ModelPropertyName = "Model";
        private Model3DGroup _model;
        public Model3DGroup Model
        {
            get => _model;
            set => Set(ref _model, value);
        }

        public const string SelectionModelPropertyName = "SelectionModel";
        private Model3DGroup _selectionModel;
        public Model3DGroup SelectionModel
        {
            get => _selectionModel;
            set => Set(ref _selectionModel, value);
        }
        #endregion Properties

        #region Commands
        #region Command_FocusSelected

        public RelayCommand CommandFocusSelected
        {
            get { return _commandFocusSelected ?? (_commandFocusSelected = new RelayCommand(() => { FocusSelected(); })); }
        }

        private RelayCommand _commandFocusSelected;

        #endregion Command_FocusSelected

        public void GeometryModelClicked(GeometryModel3D model) //TODO: This should probably be a command with a dependency property or something
        {
            Point3D modelOrigin = model.Transform.Transform(new Point3D(0, 0, 0));
            BlockNode node = (from bn in BlockNodes
                where bn.Position.X == modelOrigin.X && bn.Position.Y == modelOrigin.Y && bn.Position.Z == -modelOrigin.Z
                select bn).FirstOrDefault();
            if (node == null)
            {
                return;
            }
            ObservableCollection<ITreeNode> newSelection = new ObservableCollection<ITreeNode>();
            newSelection.Add(node);
            SelectedBlocks = newSelection;
        }


        #endregion Commands

        public BlocksViewModel(EpBlueprint blueprint)
        {
            Blueprint = blueprint;
            Update();
        }

        public void Update()
        {
            if (Blueprint.Blocks == null)
            {
                return;
            }

            // Build block tree:
            BuildTree();

            // Build 3D view:
            CameraAimPoint = new Point3D(Blueprint.Width / 2, Blueprint.Height / 2, Blueprint.Depth / 2);
            CameraPosition = new Point3D(Blueprint.Width / 2, Blueprint.Height / 2, Blueprint.Depth);
            PaletteImageSource = CreateBitmapSource(Blueprint.Palette);

            BuildModel(Blueprint);
        }

        private void BuildTree()
        {
            BlockNodes.Clear();
            BlockCategories.Clear();
            foreach (EpbBlock block in Blueprint.Blocks)
            {
                if (block == null)
                {
                    continue;
                }

                EpbBlock.EpbBlockType t = block.BlockType;

                string categoryName = t.Category;
                GroupNode categoryNode = BlockCategories.FirstOrDefault(x => x.Title == categoryName);
                if (categoryNode == null)
                {
                    categoryNode = new GroupNode() {Title = categoryName};
                    BlockCategories.AddSorted(categoryNode);
                }

                BlockNode blockNode;
                switch (t.Id)
                {
                    case 1095:
                    case 1096:
                    case 1097:
                    case 1098:
                    case 1099:
                    case 1100:
                    case 1101:
                    case 1102:
                    case 1103:
                        blockNode = new LcdNode(block, Blueprint);
                        break;
                    default:
                        blockNode = new BlockNode(block, Blueprint);
                        break;
                }

                GroupNode typeNode = (GroupNode)categoryNode.Children.FirstOrDefault(x => x.Title == blockNode.BlockType);
                if (typeNode == null)
                {
                    typeNode = new GroupNode() { Title = blockNode.BlockType };
                    categoryNode.AddSorted(typeNode);
                }
                typeNode.AddSorted(blockNode);

                BlockNodes.Add(blockNode);
            }
        }

        private void BuildModel(EpBlueprint blueprint)
        {
            Model3DGroup group = new Model3DGroup();
            GeometryModel3D model = null;
            foreach (BlockNode node in BlockNodes)
            {
                EpbBlock block = node.Block;
                Point3D pos = node.Position;

                if (block == null)
                {
                    continue;
                }
                switch (block.BlockType.Id)
                {
                    case 381:  //HullFullSmall
                    case 383:  //HullArmoredFullSmall
                    case 397:  //WoodFull
                    case 400:  //ConcreteFull
                    case 403:  //HullFullLarge
                    case 406:  //HullArmormedFullLarge
                    case 409:  //AlienFull
                    case 412:  //HullCombatFullLarge
                    case 1323: //ConcreteArmoredFull
                    case 1396: //AlienFullLarge
                    case 1479: //PlasticFullSmall
                    case 1482: //PlasticFullLarge
                    case 1595: //HullCombatFullSmall
                        model = CreateBuildingBlock(pos, block, blueprint, MeshGenerators.BuildingBlockVariants_FullLarge_MeshGenerators);
                        break;
                    case 382:  //HullThinSmall
                    case 384:  //HullArmoredThinSmall
                    case 398:  //WoodThin
                    case 401:  //ConcreteThin
                    case 404:  //HullThinLarge
                    case 407:  //HullArmoredThinLarge
                    case 410:  //AlienThin
                    case 413:  //HullCombatThinLarge
                    case 1324: //ConcreteArmoredThin
                    case 1397: //AlienThinLarge
                    case 1480: //PlasticThinSmall
                    case 1483: //PlasticThinLarge
                    case 1596: //HullCombatThinSmall
                        model = CreateBuildingBlock(pos, block, blueprint, MeshGenerators.BuildingBlockVariants_ThinLarge_MeshGenerators);
                        break;
                    case 1782: //WoodExtended
                    case 1783: //ConcreteExtended
                    case 1784: //ConcreteArmoredExtended
                    case 1785: //PlasticExtendedLarge
                    case 1786: //HullExtendedLarge
                    case 1787: //HullArmoredExtendedLarge
                    case 1788: //HullCombatExtendedLarge
                    case 1789: //AlienExtended
                    case 1790: //PlasticExtendedSmall
                    case 1791: //HullExtendedSmall
                    case 1792: //HullArmoredExtendedSmall
                    case 1793: //HullCombatExtendedSmall
                    case 1794: //AlienExtendedLarge
                        model = CreateBuildingBlock(pos, block, blueprint, MeshGenerators.BuildingBlockVariants_ExtendedLarge_MeshGenerators);
                        break;
                    case 1824: //WoodExtended2
                    case 1825: //ConcreteExtended2
                    case 1826: //ConcreteArmoredExtended2
                    case 1827: //PlasticExtendedLarge2
                    case 1828: //HullExtendedLarge2
                    case 1829: //HullArmoredExtendedLarge2
                    case 1830: //HullCombatExtendedLarge2
                    case 1831: //AlienExtended2
                    case 1832: //PlasticExtendedSmall2
                    case 1833: //HullExtendedSmall2
                    case 1834: //HullArmoredExtendedSmall2
                    case 1835: //HullCombatExtendedSmall2
                    case 1836: //AlienExtendedLarge2
                        model = CreateBuildingBlock(pos, block, blueprint, MeshGenerators.BuildingBlockVariants_ExtendedLarge2_MeshGenerators);
                        break;
                    case 1837: //WoodExtended3
                    case 1838: //ConcreteExtended3
                    case 1839: //ConcreteArmoredExtended3
                    case 1840: //PlasticExtendedLarge3
                    case 1841: //HullExtendedLarge3
                    case 1842: //HullArmoredExtendedLarge3
                    case 1843: //HullCombatExtendedLarge3
                    case 1844: //AlienExtended3
                    case 1845: //PlasticExtendedSmall3
                    case 1846: //HullExtendedSmall3
                    case 1847: //HullArmoredExtendedSmall3
                    case 1848: //HullCombatExtendedSmall3
                    case 1849: //AlienExtendedLarge3
                        model = CreateBuildingBlock(pos, block, blueprint, MeshGenerators.BuildingBlockVariants_ExtendedLarge3_MeshGenerators);
                        break;
                    case 1850: //WoodExtended4
                    case 1851: //ConcreteExtended4
                    case 1852: //ConcreteArmoredExtended4
                    case 1853: //PlasticExtendedLarge4
                    case 1854: //HullExtendedLarge4
                    case 1855: //HullArmoredExtendedLarge4
                    case 1856: //HullCombatExtendedLarge4
                    case 1857: //AlienExtended4
                    case 1858: //PlasticExtendedSmall4
                    case 1859: //HullExtendedSmall4
                    case 1860: //HullArmoredExtendedSmall4
                    case 1861: //HullCombatExtendedSmall4
                    case 1862: //AlienExtendedLarge4
                        model = CreateBuildingBlock(pos, block, blueprint, MeshGenerators.BuildingBlockVariants_ExtendedLarge4_MeshGenerators);
                        break;
                    case 1863: //WoodExtended5
                    case 1864: //ConcreteExtended5
                    case 1865: //ConcreteArmoredExtended5
                    case 1866: //PlasticExtendedLarge5
                    case 1867: //HullExtendedLarge5
                    case 1868: //HullArmoredExtendedLarge5
                    case 1869: //HullCombatExtendedLarge5
                    case 1870: //AlienExtended5
                    case 1871: //PlasticExtendedSmall5
                    case 1872: //HullExtendedSmall5
                    case 1873: //HullArmoredExtendedSmall5
                    case 1874: //HullCombatExtendedSmall5
                    case 1875: //AlienExtendedLarge5
                        model = CreateBuildingBlock(pos, block, blueprint, MeshGenerators.BuildingBlockVariants_ExtendedLarge5_MeshGenerators);
                        break;
                    default:
                        EpbColourIndex colourIndex = block.Colours[0];
                        EpbColour colour = blueprint.Palette[colourIndex];
                        Color c = Color.FromArgb(128, colour.R, colour.G, colour.B);

                        model = CreateSelectionBox(pos, 1.0, c);
                        break;
                }
                group.Children.Add(model);
            }
            Model = group;
        }

        private void BuildSelectionModel()
        {
            Model3DGroup group = new Model3DGroup();
            foreach (ITreeNode node in SelectedBlocks)
            {
                switch (node)
                {
                    case LcdNode lcd:
                        group.Children.Add(CreateSelectionBox(lcd.Position, 1.1, Color.FromArgb(128, 255, 0, 0)));
                        break;
                    case BlockNode def:
                        group.Children.Add(CreateSelectionBox(def.Position, 1.1, Color.FromArgb(128, 255, 0, 0)));
                        break;
                }
            }
            SelectionModel = group;
        }

        public void FocusSelected()
        {
            Point3D aimPoint = new Point3D(0, 0, 0);
            int count = 0;
            foreach (ITreeNode node in SelectedBlocks)
            {
                switch (node)
                {
                    case BlockNode def:
                        aimPoint = aimPoint.Add(def.Position);
                        count++;
                        break;
                }
            }

            if (count > 0)
            {
                aimPoint = aimPoint.Scale(count);
                aimPoint.Z *= -1;
                CameraAimPoint = aimPoint;
            }
        }

        private GeometryModel3D CreateBuildingBlock(Point3D pos, EpbBlock block, EpBlueprint blueprint, MeshGenerators.MeshGenerator[] generators)
        {
            GeometryModel3D model = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();

            int faceIndex = 0;
            MeshGenerators.MeshGenerator generator = generators[block.Variant];
            if (block.Variant < generators.Length && generators[block.Variant] != null)
            {
                faceIndex = generators[block.Variant](blueprint, mesh, block.Colours, faceIndex);
            }
            else
            {
                faceIndex = DefaultMeshGenerator(blueprint, mesh, block.Colours, faceIndex);
            }

            model.Geometry = mesh;
            Transform3DGroup tg = new Transform3DGroup();
            tg.Children.Add(new RotateTransform3D(new QuaternionRotation3D(Rotation[block.Rotation])));
            tg.Children.Add(new TranslateTransform3D(pos.X, pos.Y, -pos.Z));
            model.Transform = tg;
            ImageBrush brush = new ImageBrush(PaletteImageSource) { AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, Stretch = Stretch.Fill, ViewportUnits = BrushMappingMode.Absolute };
            var material = new DiffuseMaterial(brush);
            model.Material = material;
            return model;
        }

        private BitmapSource CreateBitmapSource(EpbPalette palette)
        {
            PixelFormat pf = PixelFormats.Bgr24;
            int width = palette.Length * PaletteResolution;
            int height = PaletteResolution;
            int stride = (width * pf.BitsPerPixel + 7 ) / 8;
            byte[] pixels = new byte[height * stride];

            int i = 0;
            for (int y = 0; y < PaletteResolution; y++)
            {
                for (int c = 0; c < palette.Length; c++)
                {
                    EpbColour color = palette[c];
                    for (int x = 0; x < PaletteResolution; x++)
                    {
                        pixels[i++] = color.B;
                        pixels[i++] = color.G;
                        pixels[i++] = color.R;
                    }
                }
            }

            BitmapSource image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                pf,
                null,
                pixels,
                stride);
            //using (var fileStream = new System.IO.FileStream("palette.png", System.IO.FileMode.Create))
            //{
            //    BitmapEncoder encoder = new PngBitmapEncoder();
            //    encoder.Frames.Add(BitmapFrame.Create(image));
            //    encoder.Save(fileStream);
            //}
            return image;
        }

        private GeometryModel3D CreateSelectionBox(Point3D pos, double scale, Color colour)
        {
            GeometryModel3D model = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();

            EpbColourIndex[] colours = new EpbColourIndex[6] {0, 0, 0, 0, 0, 0};
            int faceIndex = 0;
            faceIndex = DefaultMeshGenerator(Blueprint, mesh, colours, faceIndex);

            model.Geometry = mesh;

            Transform3DGroup t = new Transform3DGroup();
            t.Children.Add(new ScaleTransform3D(scale, scale, scale));
            t.Children.Add(new TranslateTransform3D(pos.X, pos.Y, -pos.Z));
            model.Transform = t;

            SolidColorBrush brush = new SolidColorBrush(colour);
            DiffuseMaterial material = new DiffuseMaterial(brush);
            model.Material = material;

            return model;
        }

        private int DefaultMeshGenerator(EpBlueprint blueprint, MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Back], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Back], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 1.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Front], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Front], 1.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Right], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Right], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Left], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Left], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 1.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Bottom], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Bottom], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(MeshGenerators.GetUV(blueprint, colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            MeshGenerators.AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
    }
}
