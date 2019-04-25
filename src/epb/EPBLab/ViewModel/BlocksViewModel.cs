
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using EPBLab.Helpers;
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
        #endregion Commands

        public BlocksViewModel(EpBlueprint blueprint)
        {
            Blueprint = blueprint;

            if (Blueprint.Blocks == null)
            {
                return;
            }

            // Build block tree:
            BuildTree();

            // Build 3D view:
            CameraPosition = new Point3D(0, 0,  3);
            CameraAimPoint = new Point3D(0, 0, -1);
            PaletteImageSource = CreateBitmapSource(blueprint.Palette);

            BuildModel(Blueprint);
        }

        private void BuildTree()
        {
            BlockNodes.Clear();
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
                    case 381: //HullFullSmall
                    case 383: //HullThinSmall
                    case 397: //WoodFull
                    case 400: //ConcreteFull
                    case 403: //HullFullLarge
                    case 406: //HullArmormedFullLarge
                    case 409: //AlienFull
                    case 412: //HullCombatFullLarge
                        model = CreateBuildingBlockFull(pos, block, blueprint);
                        break;
                    case 382: //HullThinSmall
                    case 384: //HullArmoredThinSmall
                    case 398: //WoodThin
                    case 401: //ConcreteThin
                    case 404: //HullThinLarge
                    case 410: //AlienThin
                    case 413: //HullCombatThinLarge
                        model = CreateBuildingBlockThin(pos, block, blueprint);
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
                CameraAimPoint = aimPoint;
            }
        }

        private GeometryModel3D CreateBuildingBlockFull(Point3D pos, EpbBlock block, EpBlueprint blueprint)
        {
            GeometryModel3D model = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();

            int faceIndex = 0;

            Vector3D v1;
            Vector3D v2;
            Vector3D normal;

            switch (block.Variant)
            {
                case 0: //Cube
                    faceIndex = AddGeometry_Cube(mesh, block.Colours, faceIndex);
                    break;
                case 1: //CutCorner
                    faceIndex = AddGeometry_CutCorner(mesh, block.Colours, faceIndex);
                    break;
                case 2: //CornerLongA
                    faceIndex = AddGeometry_CornerLongA(mesh, block.Colours, faceIndex);
                    break;
                case 3: //CornerLongB
                    faceIndex = AddGeometry_CornerLongB(mesh, block.Colours, faceIndex);
                    break;
                case 4: //CornerLongC
                    faceIndex = AddGeometry_CornerLongC(mesh, block.Colours, faceIndex);
                    break;
                case 5: //CornerLongD
                    faceIndex = AddGeometry_CornerLongD(mesh, block.Colours, faceIndex);
                    break;
                case 6: //CornerLargeA
                    faceIndex = AddGeometry_CornerLargeA(mesh, block.Colours, faceIndex);
                    break;
                case 7: //Corner
                    faceIndex = AddGeometry_Corner(mesh, block.Colours, faceIndex);
                    break;
                case 8: //RampBottom
                    faceIndex = AddGeometry_RampBottom(mesh, block.Colours, faceIndex);
                    break;
                case 9: //RampTop
                    faceIndex = AddGeometry_RampTop(mesh, block.Colours, faceIndex);
                    break;
                case 10: //Slope
                    faceIndex = AddGeometry_Slope(mesh, block.Colours, faceIndex);
                    break;
                case 11: //CurvedCorner
                    faceIndex = AddGeometry_CurvedCorner(mesh, block.Colours, faceIndex);
                    break;
                case 12: //RoundCutCorner
                    faceIndex = AddGeometry_RoundCutCorner(mesh, block.Colours, faceIndex);
                    break;
                case 13: //RoundCorner
                    faceIndex = AddGeometry_RoundCorner(mesh, block.Colours, faceIndex);
                    break;
                case 14: //RoundCornerLong
                    faceIndex = AddGeometry_RoundCornerLong(mesh, block.Colours, faceIndex);
                    break;
                case 15: //RoundSlope
                    faceIndex = AddGeometry_RoundSlope(mesh, block.Colours, faceIndex);
                    break;
                case 16: //Cylinder
                    faceIndex = AddGeometry_Cylinder(mesh, block.Colours, faceIndex);
                    break;
                /*
                case 17: //InvardCorner
                    break;
                case 18: //InwardRoundSlope
                    break;
                case 19: //InwardCurvedCorner
                    break;
                case 20: //RoundSlopeEdgeInward
                    break;
                */
                case 21: //CylinderEndA
                    faceIndex = AddGeometry_CylinderEndA(mesh, block.Colours, faceIndex);
                    break;
                case 22: //CylinderEndB
                    faceIndex = AddGeometry_CylinderEndB(mesh, block.Colours, faceIndex);
                    break;
                case 23: //CylinderEndC
                    faceIndex = AddGeometry_CylinderEndC(mesh, block.Colours, faceIndex);
                    break;
                /*
                case 24: //RampWedgeTop
                    break;
                case 25: //Round4WayConnector
                    break;
                case 26: //RoundSlopeEdge
                    break;
                */
                case 27: //CornerLargeB
                    faceIndex = AddGeometry_CornerLargeB(mesh, block.Colours, faceIndex);
                    break;
                case 28: //CornerLargeC
                    faceIndex = AddGeometry_CornerLargeC(mesh, block.Colours, faceIndex);
                    break;
                case 29: //CornerLargeD
                    faceIndex = AddGeometry_CornerLargeD(mesh, block.Colours, faceIndex);
                    break;
                case 30: //CornerLongE
                    faceIndex = AddGeometry_CornerLongE(mesh, block.Colours, faceIndex);
                    break;
                case 31: //PyramidA
                    faceIndex = AddGeometry_PyramidA(mesh, block.Colours, faceIndex);
                    break;
                default:
                    faceIndex = AddGeometry_Cube(mesh, block.Colours, faceIndex);
                    break;
            }

            model.Geometry = mesh;
            Transform3DGroup tg = new Transform3DGroup();
            tg.Children.Add(new RotateTransform3D(new QuaternionRotation3D(Rotation[block.Rotation])));
            tg.Children.Add(new TranslateTransform3D(pos.X, pos.Y, pos.Z));
            model.Transform = tg;
            ImageBrush brush = new ImageBrush(PaletteImageSource) {AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, Stretch = Stretch.Fill, ViewportUnits = BrushMappingMode.Absolute};
            var material = new DiffuseMaterial(brush);
            model.Material = material;
            return model;
        }

        private GeometryModel3D CreateBuildingBlockThin(Point3D pos, EpbBlock block, EpBlueprint blueprint)
        {
            GeometryModel3D model = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();

            int faceIndex = 0;

            Vector3D v1;
            Vector3D v2;
            Vector3D normal;

            switch (block.Variant)
            {
                case 0: //Wall
                    faceIndex = AddGeometry_Wall(mesh, block.Colours, faceIndex);
                    break;
                case 1: //Wall L-shape
                    faceIndex = AddGeometry_WallLShape(mesh, block.Colours, faceIndex);
                    break;
                /*
                case 2: //Thin Slope
                    break;
                case 3: //Thin Corner
                    break;
                */
                case 4: //Sloped Wall
                    faceIndex = AddGeometry_SlopedWall(mesh, block.Colours, faceIndex);
                    break;
                case 5: //Sloped Wall Bottom (right)
                    faceIndex = AddGeometry_SlopedWallBottomRight(mesh, block.Colours, faceIndex);
                    break;
                case 6: //Sloped Wall Top (right)
                    faceIndex = AddGeometry_SlopedWallTopRight(mesh, block.Colours, faceIndex);
                    break;
                case 7: //Sloped Wall Bottom (left)
                    faceIndex = AddGeometry_SlopedWallBottomLeft(mesh, block.Colours, faceIndex);
                    break;
                case 8: //Sloped Wall Top (left)
                    faceIndex = AddGeometry_SlopedWallTopLeft(mesh, block.Colours, faceIndex);
                    break;
                /*
                case 9: //Round Corner Thin
                    break;
                case 10: //Round Slope Thin
                    break;
                case 11: //Round Cut Corner Thin
                    break;
                case 12: //Round Slope Edge Thin
                    break;
                case 13: //Round Corner Long Thin
                    break;
                case 14: //Corner Round Thin 2
                    break;
                case 15: //Corner Thin 2
                    break;
                */
                case 16: //Wall 3 Corner
                    faceIndex = AddGeometry_Wall3Corner(mesh, block.Colours, faceIndex);
                    break;
                case 17: //Wall Half
                    faceIndex = AddGeometry_WallHalf(mesh, block.Colours, faceIndex);
                    break;
                case 18: //Cube Half
                    faceIndex = AddGeometry_CubeHalf(mesh, block.Colours, faceIndex);
                    break;
                case 19: //Ramp Top Double
                    faceIndex = AddGeometry_RampTopDouble(mesh, block.Colours, faceIndex);
                    break;
                case 20: //Ramp Bottom A
                    faceIndex = AddGeometry_RampBottomA(mesh, block.Colours, faceIndex);
                    break;
                case 21: //Ramp Bottom B
                    faceIndex = AddGeometry_RampBottomB(mesh, block.Colours, faceIndex);
                    break;
                case 22: //Ramp Bottom C
                    faceIndex = AddGeometry_RampBottomC(mesh, block.Colours, faceIndex);
                    break;
                case 23: //Ramp Wedge Bottom
                    faceIndex = AddGeometry_RampWedgeBottom(mesh, block.Colours, faceIndex);
                    break;
                case 24: //Beam
                    faceIndex = AddGeometry_Beam(mesh, block.Colours, faceIndex);
                    break;
                case 25: //Cylinder Thin
                    faceIndex = AddGeometry_CylinderThin(mesh, block.Colours, faceIndex);
                    break;
                /*
                case 26: //Cylinder Thin T Joint
                    break;
                case 27: //Cylinder Thin Curved
                    break;
                case 28: //Cylinder Fence Bottom
                    break;
                case 29: //Cylinder Fence Top
                    break;
                */
                case 30: //Slope Half
                    faceIndex = AddGeometry_SlopeHalf(mesh, block.Colours, faceIndex);
                    break;
                default:
                    faceIndex = AddGeometry_Cube(mesh, block.Colours, faceIndex);
                    break;
            }

            model.Geometry = mesh;
            Transform3DGroup tg = new Transform3DGroup();
            tg.Children.Add(new RotateTransform3D(new QuaternionRotation3D(Rotation[block.Rotation])));
            tg.Children.Add(new TranslateTransform3D(pos.X, pos.Y, pos.Z));
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
            faceIndex = AddGeometry_Cube(mesh, colours, faceIndex);

            model.Geometry = mesh;

            Transform3DGroup t = new Transform3DGroup();
            t.Children.Add(new ScaleTransform3D(scale, scale, scale));
            t.Children.Add(new TranslateTransform3D(pos.X, pos.Y, pos.Z));
            model.Transform = t;

            SolidColorBrush brush = new SolidColorBrush(colour);
            DiffuseMaterial material = new DiffuseMaterial(brush);
            model.Material = material;

            return model;
        }

        protected void AddTriangle(Int32Collection triangles, int a, int b, int c)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        private Point GetUV(EpbColourIndex c, double s, double t)
        {
            Point p = new Point((double)c / (Blueprint.Palette.Length - 1), t);
//            Console.WriteLine($"{c}, {s}, {t} => {p.X}, {p.Y}");
            return p;
        }

        #region AddGeometry

        //BuildingBlockFull
        private int AddGeometry_Cube(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 1.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CutCorner(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            mesh.Normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            mesh.Normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            faceIndex += 6;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLongA(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 3, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 5, faceIndex + 7);
            faceIndex += 8;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLongB(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 7, faceIndex + 8);
            faceIndex += 9;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 4);
            return faceIndex;
        }
        private int AddGeometry_CornerLongC(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 7, faceIndex + 8);
            faceIndex += 9;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            return faceIndex;
        }
        private int AddGeometry_CornerLongD(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            mesh.Normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottom
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLargeA(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, 0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, 0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, 0.707107));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.707107, 0.707107, -0.000000));
            mesh.Normals.Add(new Vector3D(0.707107, 0.707107, -0.000000));
            mesh.Normals.Add(new Vector3D(0.707107, 0.707107, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_Corner(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            mesh.Normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            mesh.Normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottom(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampTop(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 3);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 3);
            faceIndex += 6;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_Slope(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CurvedCorner(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Top
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.241192, -0.500000, -0.465949));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.366055));
            mesh.Positions.Add(new Point3D(-0.207069, -0.500000, -0.207146));
            mesh.Positions.Add(new Point3D(-0.365979, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.465873, -0.500000, 0.241115));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.500000));
            mesh.Normals.Add(new Vector3D(-0.476102, 0.739349, -0.476120));
            mesh.Normals.Add(new Vector3D(-0.092632, 0.704067, -0.704067));
            mesh.Normals.Add(new Vector3D(-0.182969, 0.707114, -0.683017));
            mesh.Normals.Add(new Vector3D(-0.353556, 0.707116, -0.612361));
            mesh.Normals.Add(new Vector3D(-0.500045, 0.707107, -0.499955));
            mesh.Normals.Add(new Vector3D(-0.612393, 0.707092, -0.353547));
            mesh.Normals.Add(new Vector3D(-0.683002, 0.707095, -0.183099));
            mesh.Normals.Add(new Vector3D(-0.704055, 0.704055, -0.092810));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.465873, -0.500000, 0.241115));
            mesh.Positions.Add(new Point3D(-0.365979, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.207069, -0.500000, -0.207146));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.366055));
            mesh.Positions.Add(new Point3D(0.241192, -0.500000, -0.465949));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RoundCutCorner(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(-0.464327, -0.241136, -0.500000));
            mesh.Positions.Add(new Point3D(-0.364433, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.205525, 0.207125, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.366034, -0.500000));
            mesh.Positions.Add(new Point3D(0.242734, 0.465928, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.250000, 0.333333));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 7, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 0, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 3, faceIndex + 4);
            faceIndex += 8;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.241136, -0.465948));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.366054));
            mesh.Positions.Add(new Point3D(-0.500000, 0.207125, -0.207145));
            mesh.Positions.Add(new Point3D(-0.500000, 0.366034, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.465928, 0.241116));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.250000, 0.333333));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 7, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 5);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.241136, -0.465948));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.366054));
            mesh.Positions.Add(new Point3D(-0.500000, 0.207125, -0.207145));
            mesh.Positions.Add(new Point3D(-0.500000, 0.366034, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.465928, 0.241116));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.240450, 0.465928, 0.241116));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.366034, 0.000000));
            mesh.Positions.Add(new Point3D(-0.206425, 0.207125, -0.207145));
            mesh.Positions.Add(new Point3D(-0.364842, 0.000000, -0.366054));
            mesh.Positions.Add(new Point3D(-0.464427, -0.241136, -0.465948));
            mesh.Positions.Add(new Point3D(-0.464322, -0.241136, -0.500000));
            mesh.Positions.Add(new Point3D(-0.364428, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.205520, 0.207125, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.366034, -0.500000));
            mesh.Positions.Add(new Point3D(0.242738, 0.465928, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.240450, 0.465928, 0.241116));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.366034, 0.000000));
            mesh.Positions.Add(new Point3D(-0.206425, 0.207125, -0.207145));
            mesh.Positions.Add(new Point3D(-0.364842, 0.000000, -0.366054));
            mesh.Positions.Add(new Point3D(-0.464427, -0.241136, -0.465948));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.579631));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.665637));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.499486));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.430664));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.377855));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.344658));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.420370));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.334364));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.500515));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.569337));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.622146));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.655343));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.300913, -0.953652));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.537460, -0.843289));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.737465, -0.675385));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.887109, -0.461560));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.976345, -0.216220));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.130420, -0.991459));
            mesh.Normals.Add(new Vector3D(0.000000, 0.991450, -0.130486));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.953639, -0.300953));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.991450, -0.130486));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.843301, -0.537442));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.675420, -0.737434));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.461557, -0.887111));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.258789, -0.965934));
            mesh.Normals.Add(new Vector3D(-0.965127, 0.261765, -0.002982));
            mesh.Normals.Add(new Vector3D(-0.887108, 0.461553, -0.002740));
            mesh.Normals.Add(new Vector3D(-0.738282, 0.674488, -0.002280));
            mesh.Normals.Add(new Vector3D(-0.538330, 0.842734, -0.000635));
            mesh.Normals.Add(new Vector3D(-0.300418, 0.953807, -0.000533));
            mesh.Normals.Add(new Vector3D(-0.990631, 0.136533, -0.003061));
            mesh.Normals.Add(new Vector3D(-0.130726, 0.991419, -0.000203));
            mesh.Normals.Add(new Vector3D(-0.216340, 0.976318, -0.000533));
            mesh.Normals.Add(new Vector3D(-0.130157, 0.991493, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.461318, 0.887235, -0.000397));
            mesh.Normals.Add(new Vector3D(-0.676738, 0.736222, -0.001457));
            mesh.Normals.Add(new Vector3D(-0.843287, 0.537456, -0.002604));
            mesh.Normals.Add(new Vector3D(-0.953032, 0.302856, -0.002944));
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 0, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 4, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 8, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 3, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 7, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 2, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 9, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 1, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 10, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 12, faceIndex + 0, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 11, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 25, faceIndex + 13, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 17, faceIndex + 20, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 19, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 16, faceIndex + 22, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 20, faceIndex + 17, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 15, faceIndex + 23, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 22, faceIndex + 16, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 14, faceIndex + 24, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 15, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 13, faceIndex + 25, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 14, faceIndex + 25);
            faceIndex += 26;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RoundCorner(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.466353, -0.241137, 0.500000));
            mesh.Positions.Add(new Point3D(-0.366458, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.207549, 0.207124, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.366033, 0.500000));
            mesh.Positions.Add(new Point3D(0.240712, 0.465928, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 5, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 6, faceIndex + 0);
            faceIndex += 8;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.241137, -0.466429));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.366534));
            mesh.Positions.Add(new Point3D(0.500000, 0.207124, -0.207625));
            mesh.Positions.Add(new Point3D(0.500000, 0.366033, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.465928, 0.240636));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.240712, -0.500000, -0.466429));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.366534));
            mesh.Positions.Add(new Point3D(-0.207549, -0.500000, -0.207625));
            mesh.Positions.Add(new Point3D(-0.366458, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.466353, -0.500000, 0.240636));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.241137, -0.466429));
            mesh.Positions.Add(new Point3D(0.249531, -0.241137, -0.433518));
            mesh.Positions.Add(new Point3D(-0.000000, -0.241137, -0.337027));
            mesh.Positions.Add(new Point3D(-0.183456, -0.241137, -0.183532));
            mesh.Positions.Add(new Point3D(-0.336951, -0.241137, 0.016505));
            mesh.Positions.Add(new Point3D(-0.433441, -0.241137, 0.249455));
            mesh.Positions.Add(new Point3D(-0.466353, -0.241137, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.366534));
            mesh.Positions.Add(new Point3D(0.275385, 0.000000, -0.337027));
            mesh.Positions.Add(new Point3D(0.066529, 0.000000, -0.250516));
            mesh.Positions.Add(new Point3D(-0.112820, -0.000000, -0.112896));
            mesh.Positions.Add(new Point3D(-0.250440, -0.000000, 0.066453));
            mesh.Positions.Add(new Point3D(-0.336951, 0.000000, 0.275309));
            mesh.Positions.Add(new Point3D(-0.366458, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.207124, -0.207625));
            mesh.Positions.Add(new Point3D(0.316514, 0.207124, -0.183532));
            mesh.Positions.Add(new Point3D(0.145984, 0.207124, -0.112896));
            mesh.Positions.Add(new Point3D(-0.000000, 0.207124, -0.000000));
            mesh.Positions.Add(new Point3D(-0.112820, 0.207124, 0.145907));
            mesh.Positions.Add(new Point3D(-0.183456, 0.207124, 0.316438));
            mesh.Positions.Add(new Point3D(-0.207549, 0.207124, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.366033, -0.000000));
            mesh.Positions.Add(new Point3D(0.370114, 0.366033, 0.016505));
            mesh.Positions.Add(new Point3D(0.249531, 0.366033, 0.066453));
            mesh.Positions.Add(new Point3D(0.145984, 0.366033, 0.145907));
            mesh.Positions.Add(new Point3D(0.066529, 0.366033, 0.249455));
            mesh.Positions.Add(new Point3D(-0.000000, 0.366033, 0.370038));
            mesh.Positions.Add(new Point3D(0.000000, 0.366033, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.465928, 0.240636));
            mesh.Positions.Add(new Point3D(0.432533, 0.465928, 0.249455));
            mesh.Positions.Add(new Point3D(0.370114, 0.465928, 0.275309));
            mesh.Positions.Add(new Point3D(0.316514, 0.465928, 0.316438));
            mesh.Positions.Add(new Point3D(0.275385, 0.465928, 0.370038));
            mesh.Positions.Add(new Point3D(0.249531, 0.465928, 0.432456));
            mesh.Positions.Add(new Point3D(0.240712, 0.465928, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.127354, 0.127563, -0.983620));
            mesh.Normals.Add(new Vector3D(-0.213826, 0.129766, -0.968215));
            mesh.Normals.Add(new Vector3D(-0.451810, 0.109324, -0.885390));
            mesh.Normals.Add(new Vector3D(-0.680980, 0.117296, -0.722848));
            mesh.Normals.Add(new Vector3D(-0.836233, 0.129695, -0.532817));
            mesh.Normals.Add(new Vector3D(-0.945649, 0.130127, -0.298018));
            mesh.Normals.Add(new Vector3D(-0.983354, 0.128592, -0.128371));
            mesh.Normals.Add(new Vector3D(-0.126638, 0.213912, -0.968610));
            mesh.Normals.Add(new Vector3D(-0.244326, 0.254967, -0.935573));
            mesh.Normals.Add(new Vector3D(-0.489064, 0.258953, -0.832922));
            mesh.Normals.Add(new Vector3D(-0.692888, 0.264043, -0.670960));
            mesh.Normals.Add(new Vector3D(-0.837244, 0.258569, -0.481834));
            mesh.Normals.Add(new Vector3D(-0.933544, 0.258643, -0.248191));
            mesh.Normals.Add(new Vector3D(-0.946390, 0.298151, -0.124306));
            mesh.Normals.Add(new Vector3D(-0.115758, 0.458544, -0.881100));
            mesh.Normals.Add(new Vector3D(-0.222531, 0.503288, -0.834974));
            mesh.Normals.Add(new Vector3D(-0.438358, 0.508792, -0.740927));
            mesh.Normals.Add(new Vector3D(-0.614452, 0.500712, -0.609702));
            mesh.Normals.Add(new Vector3D(-0.750991, 0.500393, -0.430835));
            mesh.Normals.Add(new Vector3D(-0.837229, 0.500101, -0.221240));
            mesh.Normals.Add(new Vector3D(-0.838115, 0.534278, -0.110045));
            mesh.Normals.Add(new Vector3D(-0.096352, 0.672635, -0.733675));
            mesh.Normals.Add(new Vector3D(-0.186360, 0.707329, -0.681876));
            mesh.Normals.Add(new Vector3D(-0.357611, 0.707487, -0.609571));
            mesh.Normals.Add(new Vector3D(-0.502816, 0.707176, -0.497069));
            mesh.Normals.Add(new Vector3D(-0.613991, 0.707050, -0.350850));
            mesh.Normals.Add(new Vector3D(-0.689729, 0.697874, -0.192992));
            mesh.Normals.Add(new Vector3D(-0.683666, 0.727077, -0.062931));
            mesh.Normals.Add(new Vector3D(-0.068992, 0.841302, -0.536144));
            mesh.Normals.Add(new Vector3D(-0.133406, 0.866053, -0.481825));
            mesh.Normals.Add(new Vector3D(-0.254736, 0.865942, -0.430412));
            mesh.Normals.Add(new Vector3D(-0.357057, 0.865807, -0.350554));
            mesh.Normals.Add(new Vector3D(-0.432472, 0.861710, -0.265375));
            mesh.Normals.Add(new Vector3D(-0.484999, 0.865897, -0.122470));
            mesh.Normals.Add(new Vector3D(-0.457867, 0.888862, -0.016797));
            mesh.Normals.Add(new Vector3D(-0.038852, 0.952889, -0.300819));
            mesh.Normals.Add(new Vector3D(-0.083204, 0.958788, -0.271666));
            mesh.Normals.Add(new Vector3D(-0.150760, 0.958790, -0.240816));
            mesh.Normals.Add(new Vector3D(-0.207950, 0.958791, -0.193589));
            mesh.Normals.Add(new Vector3D(-0.242842, 0.960044, -0.139081));
            mesh.Normals.Add(new Vector3D(-0.271078, 0.961159, -0.051866));
            mesh.Normals.Add(new Vector3D(-0.258806, 0.965338, -0.033790));
            mesh.Normals.Add(new Vector3D(-0.083948, 0.992929, -0.083937));
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 40, faceIndex + 41);
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 39, faceIndex + 40);
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 38, faceIndex + 39);
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 37, faceIndex + 38);
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 36, faceIndex + 37);
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 35, faceIndex + 36);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 7, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 8, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 9, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 10, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 5, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 12, faceIndex + 11, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 13, faceIndex + 12, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 8, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 15, faceIndex + 14, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 9, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 16, faceIndex + 15, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 10, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 17, faceIndex + 16, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 11, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 18, faceIndex + 17, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 12, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 19, faceIndex + 18, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 12, faceIndex + 13, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 20, faceIndex + 19, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 14, faceIndex + 15, faceIndex + 21);
            AddTriangle(mesh.TriangleIndices, faceIndex + 22, faceIndex + 21, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 15, faceIndex + 16, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 22, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 16, faceIndex + 17, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 23, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 17, faceIndex + 18, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 25, faceIndex + 24, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 18, faceIndex + 19, faceIndex + 25);
            AddTriangle(mesh.TriangleIndices, faceIndex + 26, faceIndex + 25, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 19, faceIndex + 20, faceIndex + 26);
            AddTriangle(mesh.TriangleIndices, faceIndex + 27, faceIndex + 26, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 22, faceIndex + 28);
            AddTriangle(mesh.TriangleIndices, faceIndex + 29, faceIndex + 28, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 22, faceIndex + 23, faceIndex + 29);
            AddTriangle(mesh.TriangleIndices, faceIndex + 30, faceIndex + 29, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 24, faceIndex + 30);
            AddTriangle(mesh.TriangleIndices, faceIndex + 31, faceIndex + 30, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 25, faceIndex + 31);
            AddTriangle(mesh.TriangleIndices, faceIndex + 32, faceIndex + 31, faceIndex + 25);
            AddTriangle(mesh.TriangleIndices, faceIndex + 25, faceIndex + 26, faceIndex + 32);
            AddTriangle(mesh.TriangleIndices, faceIndex + 33, faceIndex + 32, faceIndex + 26);
            AddTriangle(mesh.TriangleIndices, faceIndex + 26, faceIndex + 27, faceIndex + 33);
            AddTriangle(mesh.TriangleIndices, faceIndex + 34, faceIndex + 33, faceIndex + 27);
            AddTriangle(mesh.TriangleIndices, faceIndex + 28, faceIndex + 29, faceIndex + 35);
            AddTriangle(mesh.TriangleIndices, faceIndex + 36, faceIndex + 35, faceIndex + 29);
            AddTriangle(mesh.TriangleIndices, faceIndex + 29, faceIndex + 30, faceIndex + 36);
            AddTriangle(mesh.TriangleIndices, faceIndex + 37, faceIndex + 36, faceIndex + 30);
            AddTriangle(mesh.TriangleIndices, faceIndex + 30, faceIndex + 31, faceIndex + 37);
            AddTriangle(mesh.TriangleIndices, faceIndex + 38, faceIndex + 37, faceIndex + 31);
            AddTriangle(mesh.TriangleIndices, faceIndex + 31, faceIndex + 32, faceIndex + 38);
            AddTriangle(mesh.TriangleIndices, faceIndex + 39, faceIndex + 38, faceIndex + 32);
            AddTriangle(mesh.TriangleIndices, faceIndex + 32, faceIndex + 33, faceIndex + 39);
            AddTriangle(mesh.TriangleIndices, faceIndex + 40, faceIndex + 39, faceIndex + 33);
            AddTriangle(mesh.TriangleIndices, faceIndex + 33, faceIndex + 34, faceIndex + 40);
            AddTriangle(mesh.TriangleIndices, faceIndex + 41, faceIndex + 40, faceIndex + 34);
            faceIndex += 43;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.466353, -0.500000, 0.240636));
            mesh.Positions.Add(new Point3D(-0.366458, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.207549, -0.500000, -0.207625));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.366534));
            mesh.Positions.Add(new Point3D(0.240712, -0.500000, -0.466429));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            return faceIndex;
        }
        private int AddGeometry_RoundCornerLong(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.466353, -0.241137, 0.500000));
            mesh.Positions.Add(new Point3D(-0.366458, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.207549, 0.207124, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.366033, 0.500000));
            mesh.Positions.Add(new Point3D(0.240712, 0.465928, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 5, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 6, faceIndex + 0);
            faceIndex += 8;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.241137, -0.466429));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.366534));
            mesh.Positions.Add(new Point3D(0.500000, 0.207124, -0.207625));
            mesh.Positions.Add(new Point3D(0.500000, 0.366033, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.465928, 0.240636));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(-0.466353, -0.241137, 0.500000));
            mesh.Positions.Add(new Point3D(-0.366458, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.207549, 0.207124, 0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.366033, 0.500000));
            mesh.Positions.Add(new Point3D(0.240712, 0.465928, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.466353, -0.241137, -0.465654));
            mesh.Positions.Add(new Point3D(-0.366458, 0.000000, -0.365760));
            mesh.Positions.Add(new Point3D(-0.207549, 0.207124, -0.206851));
            mesh.Positions.Add(new Point3D(0.000000, 0.366033, -0.000000));
            mesh.Positions.Add(new Point3D(0.240712, 0.465928, 0.241410));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.466259, 0.243217));
            mesh.Positions.Add(new Point3D(0.500000, 0.366365, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.207456, -0.205044));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.363953));
            mesh.Positions.Add(new Point3D(0.500000, -0.240806, -0.463848));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.240712, 0.466259, 0.243217));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.366365, -0.000000));
            mesh.Positions.Add(new Point3D(-0.207549, 0.207456, -0.205044));
            mesh.Positions.Add(new Point3D(-0.366458, 0.000000, -0.363953));
            mesh.Positions.Add(new Point3D(-0.466353, -0.240806, -0.463848));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.656050));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.622647));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.569509));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.500259));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.419615));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.499875, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.420738));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500416, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.501381));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.570631));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.623769));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.657172));
            mesh.Normals.Add(new Vector3D(-0.953805, 0.300428, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.843289, 0.537461, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.674920, 0.737891, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.461637, 0.887069, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.259025, 0.965871, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.991658, 0.128897, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.991658, 0.128897, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.976578, 0.215163, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.887111, 0.461557, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.737223, 0.675650, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.537076, 0.843534, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.301268, 0.953540, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.130287, 0.991476, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.954274, -0.298934));
            mesh.Normals.Add(new Vector3D(0.000000, 0.842069, -0.539370));
            mesh.Normals.Add(new Vector3D(0.000000, 0.673835, -0.738881));
            mesh.Normals.Add(new Vector3D(0.000000, 0.461619, -0.887078));
            mesh.Normals.Add(new Vector3D(0.000000, 0.221412, -0.975180));
            mesh.Normals.Add(new Vector3D(0.000000, 0.138143, -0.990412));
            mesh.Normals.Add(new Vector3D(0.000000, 0.138143, -0.990412));
            mesh.Normals.Add(new Vector3D(0.000000, 0.966343, -0.257257));
            mesh.Normals.Add(new Vector3D(0.000000, 0.991477, -0.130278));
            mesh.Normals.Add(new Vector3D(0.000000, 0.887301, -0.461191));
            mesh.Normals.Add(new Vector3D(0.000000, 0.735060, -0.678002));
            mesh.Normals.Add(new Vector3D(0.000000, 0.537161, -0.843480));
            mesh.Normals.Add(new Vector3D(0.000000, 0.303680, -0.952774));
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 4, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 7, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 8, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 0, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 9, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 1, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 10, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 2, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 11, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 3, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 14, faceIndex + 22, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 20, faceIndex + 13, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 15, faceIndex + 23, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 22, faceIndex + 14, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 16, faceIndex + 24, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 15, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 17, faceIndex + 25, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 16, faceIndex + 25);
            AddTriangle(mesh.TriangleIndices, faceIndex + 18, faceIndex + 19, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 25, faceIndex + 17, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 13, faceIndex + 20);
            faceIndex += 26;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RoundSlope(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 3);
            faceIndex += 6;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, 0.465928, 0.241115));
            mesh.Positions.Add(new Point3D(0.500000, 0.366033, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.207124, -0.207146));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.366055));
            mesh.Positions.Add(new Point3D(0.500000, -0.241137, -0.465949));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.041667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.083333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.125000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.166667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.208333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.250000, 0.333333));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 5);
            faceIndex += 8;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.465928, 0.241115));
            mesh.Positions.Add(new Point3D(-0.500000, 0.366033, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.207124, -0.207146));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.366055));
            mesh.Positions.Add(new Point3D(-0.500000, -0.241137, -0.465949));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.041667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.083333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.125000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.166667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.208333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.666667));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 1, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 5, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 0);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, 0.465928, 0.241115));
            mesh.Positions.Add(new Point3D(0.500000, 0.366033, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.207124, -0.207146));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.366055));
            mesh.Positions.Add(new Point3D(0.500000, -0.241137, -0.465949));
            mesh.Positions.Add(new Point3D(-0.500000, 0.465928, 0.241115));
            mesh.Positions.Add(new Point3D(-0.500000, 0.366033, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.207124, -0.207146));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.366055));
            mesh.Positions.Add(new Point3D(-0.500000, -0.241137, -0.465949));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.333333));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.976345, -0.216220));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.887109, -0.461561));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.737466, -0.675384));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.537461, -0.843289));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.300911, -0.953652));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.953639, -0.300953));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.843301, -0.537441));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.675421, -0.737432));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.461557, -0.887111));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.216163, -0.976357));
            mesh.Normals.Add(new Vector3D(0.000000, 0.130415, -0.991459));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.130415, -0.991459));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.991450, -0.130486));
            mesh.Normals.Add(new Vector3D(0.000000, 0.991450, -0.130486));
            AddTriangle(mesh.TriangleIndices, faceIndex + 13, faceIndex + 0, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 12, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 5, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 6, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 7, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 8, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 11, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 9, faceIndex + 11);
            faceIndex += 14;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            return faceIndex;
        }
        private int AddGeometry_Cylinder(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Front
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(0.482959, 0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.433012, 0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.353557, 0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.250010, 0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.129427, 0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.129377, 0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.249960, 0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.353507, 0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.432962, 0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.482909, 0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.482909, 0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.432962, 0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.353507, 0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.249960, 0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.129377, 0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.129427, 0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.250010, 0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.353557, 0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.433012, 0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.482959, 0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.041667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.083333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.125000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.166667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.208333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.541667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.583333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.625000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.666667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.708333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.791667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.833333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.875000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.916667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.958333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.041667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.083333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.125000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.166667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.208333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.291667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.333333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.375000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.416667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.458333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.541667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.583333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.625000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.666667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.708333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.791667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.833333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.875000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.916667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.958333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.750000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.750000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.500000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.500000, 0.333333));
            mesh.Normals.Add(new Vector3D(0.976346, 0.000000, -0.216212));
            mesh.Normals.Add(new Vector3D(0.887114, 0.000000, -0.461551));
            mesh.Normals.Add(new Vector3D(0.737428, 0.000000, -0.675426));
            mesh.Normals.Add(new Vector3D(0.537487, 0.000000, -0.843272));
            mesh.Normals.Add(new Vector3D(0.300859, 0.000000, -0.953669));
            mesh.Normals.Add(new Vector3D(-0.216131, 0.000000, -0.976364));
            mesh.Normals.Add(new Vector3D(-0.461551, 0.000000, -0.887114));
            mesh.Normals.Add(new Vector3D(-0.675426, 0.000000, -0.737428));
            mesh.Normals.Add(new Vector3D(-0.843272, 0.000000, -0.537487));
            mesh.Normals.Add(new Vector3D(-0.953614, 0.000000, -0.301033));
            mesh.Normals.Add(new Vector3D(-0.976277, -0.000000, 0.216527));
            mesh.Normals.Add(new Vector3D(-0.887114, -0.000000, 0.461551));
            mesh.Normals.Add(new Vector3D(-0.737428, -0.000000, 0.675426));
            mesh.Normals.Add(new Vector3D(-0.537487, -0.000000, 0.843272));
            mesh.Normals.Add(new Vector3D(-0.301120, -0.000000, 0.953586));
            mesh.Normals.Add(new Vector3D(0.216607, -0.000000, 0.976259));
            mesh.Normals.Add(new Vector3D(0.461551, -0.000000, 0.887114));
            mesh.Normals.Add(new Vector3D(0.675426, -0.000000, 0.737428));
            mesh.Normals.Add(new Vector3D(0.843272, -0.000000, 0.537487));
            mesh.Normals.Add(new Vector3D(0.953641, -0.000000, 0.300946));
            mesh.Normals.Add(new Vector3D(0.953651, 0.000000, -0.300913));
            mesh.Normals.Add(new Vector3D(0.843272, 0.000000, -0.537487));
            mesh.Normals.Add(new Vector3D(0.675426, 0.000000, -0.737428));
            mesh.Normals.Add(new Vector3D(0.461551, 0.000000, -0.887114));
            mesh.Normals.Add(new Vector3D(0.216099, 0.000000, -0.976372));
            mesh.Normals.Add(new Vector3D(-0.300874, 0.000000, -0.953664));
            mesh.Normals.Add(new Vector3D(-0.537487, 0.000000, -0.843272));
            mesh.Normals.Add(new Vector3D(-0.737428, 0.000000, -0.675426));
            mesh.Normals.Add(new Vector3D(-0.887114, 0.000000, -0.461551));
            mesh.Normals.Add(new Vector3D(-0.976292, 0.000000, -0.216460));
            mesh.Normals.Add(new Vector3D(-0.953603, -0.000000, 0.301066));
            mesh.Normals.Add(new Vector3D(-0.843272, -0.000000, 0.537487));
            mesh.Normals.Add(new Vector3D(-0.675426, -0.000000, 0.737428));
            mesh.Normals.Add(new Vector3D(-0.461551, -0.000000, 0.887114));
            mesh.Normals.Add(new Vector3D(-0.216640, -0.000000, 0.976251));
            mesh.Normals.Add(new Vector3D(0.301104, -0.000000, 0.953591));
            mesh.Normals.Add(new Vector3D(0.537487, -0.000000, 0.843272));
            mesh.Normals.Add(new Vector3D(0.737428, -0.000000, 0.675426));
            mesh.Normals.Add(new Vector3D(0.887114, -0.000000, 0.461551));
            mesh.Normals.Add(new Vector3D(0.976332, -0.000000, 0.216279));
            mesh.Normals.Add(new Vector3D(-0.044075, -0.000000, 0.999028));
            mesh.Normals.Add(new Vector3D(0.044025, -0.000000, 0.999030));
            mesh.Normals.Add(new Vector3D(-0.043812, 0.000000, -0.999040));
            mesh.Normals.Add(new Vector3D(0.043762, 0.000000, -0.999042));
            mesh.Normals.Add(new Vector3D(-0.999030, -0.000000, 0.044034));
            mesh.Normals.Add(new Vector3D(-0.999035, 0.000000, -0.043931));
            mesh.Normals.Add(new Vector3D(0.999040, 0.000000, -0.043803));
            mesh.Normals.Add(new Vector3D(0.999036, -0.000000, 0.043905));
            AddTriangle(mesh.TriangleIndices, faceIndex + 47, faceIndex + 0, faceIndex + 46);
            AddTriangle(mesh.TriangleIndices, faceIndex + 20, faceIndex + 46, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 20, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 21);
            AddTriangle(mesh.TriangleIndices, faceIndex + 22, faceIndex + 21, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 22, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 23, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 43, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 24, faceIndex + 43);
            AddTriangle(mesh.TriangleIndices, faceIndex + 43, faceIndex + 5, faceIndex + 42);
            AddTriangle(mesh.TriangleIndices, faceIndex + 25, faceIndex + 42, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 25);
            AddTriangle(mesh.TriangleIndices, faceIndex + 26, faceIndex + 25, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 7, faceIndex + 26);
            AddTriangle(mesh.TriangleIndices, faceIndex + 27, faceIndex + 26, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 8, faceIndex + 27);
            AddTriangle(mesh.TriangleIndices, faceIndex + 28, faceIndex + 27, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 9, faceIndex + 28);
            AddTriangle(mesh.TriangleIndices, faceIndex + 29, faceIndex + 28, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 45, faceIndex + 29);
            AddTriangle(mesh.TriangleIndices, faceIndex + 44, faceIndex + 29, faceIndex + 45);
            AddTriangle(mesh.TriangleIndices, faceIndex + 45, faceIndex + 10, faceIndex + 44);
            AddTriangle(mesh.TriangleIndices, faceIndex + 30, faceIndex + 44, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 11, faceIndex + 30);
            AddTriangle(mesh.TriangleIndices, faceIndex + 31, faceIndex + 30, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 12, faceIndex + 31);
            AddTriangle(mesh.TriangleIndices, faceIndex + 32, faceIndex + 31, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 12, faceIndex + 13, faceIndex + 32);
            AddTriangle(mesh.TriangleIndices, faceIndex + 33, faceIndex + 32, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 13, faceIndex + 14, faceIndex + 33);
            AddTriangle(mesh.TriangleIndices, faceIndex + 34, faceIndex + 33, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 14, faceIndex + 40, faceIndex + 34);
            AddTriangle(mesh.TriangleIndices, faceIndex + 41, faceIndex + 34, faceIndex + 40);
            AddTriangle(mesh.TriangleIndices, faceIndex + 40, faceIndex + 15, faceIndex + 41);
            AddTriangle(mesh.TriangleIndices, faceIndex + 35, faceIndex + 41, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 15, faceIndex + 16, faceIndex + 35);
            AddTriangle(mesh.TriangleIndices, faceIndex + 36, faceIndex + 35, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 16, faceIndex + 17, faceIndex + 36);
            AddTriangle(mesh.TriangleIndices, faceIndex + 37, faceIndex + 36, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 17, faceIndex + 18, faceIndex + 37);
            AddTriangle(mesh.TriangleIndices, faceIndex + 38, faceIndex + 37, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 18, faceIndex + 19, faceIndex + 38);
            AddTriangle(mesh.TriangleIndices, faceIndex + 39, faceIndex + 38, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 19, faceIndex + 47, faceIndex + 39);
            AddTriangle(mesh.TriangleIndices, faceIndex + 46, faceIndex + 39, faceIndex + 47);
            faceIndex += 48;
            // Top
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.482959, 0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.433012, 0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.353557, 0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.250010, 0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.129427, 0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.129377, 0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.249960, 0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.353507, 0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.432962, 0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.482909, 0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.482909, 0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.432962, 0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.353507, 0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.249960, 0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.129377, 0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.129427, 0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.250010, 0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.353557, 0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.433012, 0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.482959, 0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.666667));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 0, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 1, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 15, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 6, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 7, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 8, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 9, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 10, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 22, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 22, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 24, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 5, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 21, faceIndex + 0);
            faceIndex += 25;
            // Bottom
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.041667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.083333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.125000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.166667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.208333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.541667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.583333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.625000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.666667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.708333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.791667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.833333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.875000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.916667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.958333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.750000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.250000, 0.333333));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 19, faceIndex + 20, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 9, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 22, faceIndex + 14, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 4, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 24, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 5, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 6, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 7, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 8, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 21, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 10, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 12, faceIndex + 11, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 13, faceIndex + 12, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 14, faceIndex + 13, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 15, faceIndex + 22, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 16, faceIndex + 15, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 17, faceIndex + 16, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 18, faceIndex + 17, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 19, faceIndex + 18, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 23, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 20);
            return faceIndex;
        }
        private int AddGeometry_CylinderEndA(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Top
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(0.482959, -0.370539, -0.000000));
            mesh.Positions.Add(new Point3D(0.466504, -0.370539, -0.125044));
            mesh.Positions.Add(new Point3D(0.418258, -0.370539, -0.241518));
            mesh.Positions.Add(new Point3D(0.341511, -0.370539, -0.341537));
            mesh.Positions.Add(new Point3D(0.241492, -0.370539, -0.418285));
            mesh.Positions.Add(new Point3D(0.125018, -0.370539, -0.466530));
            mesh.Positions.Add(new Point3D(-0.000000, -0.370539, -0.482986));
            mesh.Positions.Add(new Point3D(-0.124968, -0.370539, -0.466530));
            mesh.Positions.Add(new Point3D(-0.241442, -0.370539, -0.418285));
            mesh.Positions.Add(new Point3D(-0.341461, -0.370539, -0.341537));
            mesh.Positions.Add(new Point3D(-0.418208, -0.370539, -0.241518));
            mesh.Positions.Add(new Point3D(-0.466454, -0.370539, -0.125044));
            mesh.Positions.Add(new Point3D(-0.482909, -0.370539, -0.000000));
            mesh.Positions.Add(new Point3D(-0.466454, -0.370539, 0.124941));
            mesh.Positions.Add(new Point3D(-0.418208, -0.370539, 0.241416));
            mesh.Positions.Add(new Point3D(-0.341461, -0.370539, 0.341435));
            mesh.Positions.Add(new Point3D(-0.241442, -0.370539, 0.418182));
            mesh.Positions.Add(new Point3D(-0.124968, -0.370539, 0.466427));
            mesh.Positions.Add(new Point3D(-0.000000, -0.370539, 0.482883));
            mesh.Positions.Add(new Point3D(0.125018, -0.370539, 0.466427));
            mesh.Positions.Add(new Point3D(0.241492, -0.370539, 0.418182));
            mesh.Positions.Add(new Point3D(0.341511, -0.370539, 0.341435));
            mesh.Positions.Add(new Point3D(0.418258, -0.370539, 0.241416));
            mesh.Positions.Add(new Point3D(0.466504, -0.370539, 0.124941));
            mesh.Positions.Add(new Point3D(0.433012, -0.249955, -0.000000));
            mesh.Positions.Add(new Point3D(0.418258, -0.249955, -0.112117));
            mesh.Positions.Add(new Point3D(0.375003, -0.249955, -0.216545));
            mesh.Positions.Add(new Point3D(0.306193, -0.249955, -0.306219));
            mesh.Positions.Add(new Point3D(0.216518, -0.249955, -0.375029));
            mesh.Positions.Add(new Point3D(0.112090, -0.249955, -0.418285));
            mesh.Positions.Add(new Point3D(-0.000000, -0.249955, -0.433038));
            mesh.Positions.Add(new Point3D(-0.112040, -0.249955, -0.418285));
            mesh.Positions.Add(new Point3D(-0.216468, -0.249955, -0.375029));
            mesh.Positions.Add(new Point3D(-0.306143, -0.249955, -0.306219));
            mesh.Positions.Add(new Point3D(-0.374953, -0.249955, -0.216545));
            mesh.Positions.Add(new Point3D(-0.418208, -0.249955, -0.112117));
            mesh.Positions.Add(new Point3D(-0.432962, -0.249955, -0.000000));
            mesh.Positions.Add(new Point3D(-0.418208, -0.249955, 0.112014));
            mesh.Positions.Add(new Point3D(-0.374953, -0.249955, 0.216442));
            mesh.Positions.Add(new Point3D(-0.306143, -0.249955, 0.306117));
            mesh.Positions.Add(new Point3D(-0.216468, -0.249955, 0.374926));
            mesh.Positions.Add(new Point3D(-0.112040, -0.249955, 0.418182));
            mesh.Positions.Add(new Point3D(-0.000000, -0.249955, 0.432936));
            mesh.Positions.Add(new Point3D(0.112090, -0.249955, 0.418182));
            mesh.Positions.Add(new Point3D(0.216518, -0.249955, 0.374926));
            mesh.Positions.Add(new Point3D(0.306193, -0.249955, 0.306117));
            mesh.Positions.Add(new Point3D(0.375003, -0.249955, 0.216442));
            mesh.Positions.Add(new Point3D(0.418258, -0.249955, 0.112014));
            mesh.Positions.Add(new Point3D(0.353557, -0.146408, 0.000000));
            mesh.Positions.Add(new Point3D(0.341511, -0.146408, -0.091552));
            mesh.Positions.Add(new Point3D(0.306193, -0.146408, -0.176817));
            mesh.Positions.Add(new Point3D(0.250010, -0.146408, -0.250036));
            mesh.Positions.Add(new Point3D(0.176791, -0.146408, -0.306219));
            mesh.Positions.Add(new Point3D(0.091526, -0.146408, -0.341537));
            mesh.Positions.Add(new Point3D(-0.000000, -0.146408, -0.353584));
            mesh.Positions.Add(new Point3D(-0.091476, -0.146408, -0.341537));
            mesh.Positions.Add(new Point3D(-0.176741, -0.146408, -0.306219));
            mesh.Positions.Add(new Point3D(-0.249960, -0.146408, -0.250036));
            mesh.Positions.Add(new Point3D(-0.306143, -0.146408, -0.176817));
            mesh.Positions.Add(new Point3D(-0.341461, -0.146408, -0.091552));
            mesh.Positions.Add(new Point3D(-0.353507, -0.146408, -0.000000));
            mesh.Positions.Add(new Point3D(-0.341461, -0.146408, 0.091450));
            mesh.Positions.Add(new Point3D(-0.306143, -0.146408, 0.176715));
            mesh.Positions.Add(new Point3D(-0.249960, -0.146408, 0.249934));
            mesh.Positions.Add(new Point3D(-0.176741, -0.146408, 0.306117));
            mesh.Positions.Add(new Point3D(-0.091476, -0.146408, 0.341435));
            mesh.Positions.Add(new Point3D(-0.000000, -0.146408, 0.353481));
            mesh.Positions.Add(new Point3D(0.091526, -0.146408, 0.341435));
            mesh.Positions.Add(new Point3D(0.176791, -0.146408, 0.306117));
            mesh.Positions.Add(new Point3D(0.250010, -0.146408, 0.249934));
            mesh.Positions.Add(new Point3D(0.306193, -0.146408, 0.176715));
            mesh.Positions.Add(new Point3D(0.341511, -0.146408, 0.091450));
            mesh.Positions.Add(new Point3D(0.250010, -0.066954, 0.000000));
            mesh.Positions.Add(new Point3D(0.241492, -0.066954, -0.064752));
            mesh.Positions.Add(new Point3D(0.216518, -0.066954, -0.125044));
            mesh.Positions.Add(new Point3D(0.176791, -0.066954, -0.176817));
            mesh.Positions.Add(new Point3D(0.125018, -0.066954, -0.216545));
            mesh.Positions.Add(new Point3D(0.064726, -0.066954, -0.241518));
            mesh.Positions.Add(new Point3D(0.000000, -0.066954, -0.250036));
            mesh.Positions.Add(new Point3D(-0.064676, -0.066954, -0.241518));
            mesh.Positions.Add(new Point3D(-0.124968, -0.066954, -0.216545));
            mesh.Positions.Add(new Point3D(-0.176741, -0.066954, -0.176817));
            mesh.Positions.Add(new Point3D(-0.216468, -0.066954, -0.125044));
            mesh.Positions.Add(new Point3D(-0.241442, -0.066954, -0.064752));
            mesh.Positions.Add(new Point3D(-0.249960, -0.066954, -0.000000));
            mesh.Positions.Add(new Point3D(-0.241442, -0.066954, 0.064650));
            mesh.Positions.Add(new Point3D(-0.216468, -0.066954, 0.124941));
            mesh.Positions.Add(new Point3D(-0.176741, -0.066954, 0.176715));
            mesh.Positions.Add(new Point3D(-0.124968, -0.066954, 0.216442));
            mesh.Positions.Add(new Point3D(-0.064676, -0.066954, 0.241416));
            mesh.Positions.Add(new Point3D(0.000000, -0.066954, 0.249934));
            mesh.Positions.Add(new Point3D(0.064726, -0.066954, 0.241416));
            mesh.Positions.Add(new Point3D(0.125018, -0.066954, 0.216442));
            mesh.Positions.Add(new Point3D(0.176791, -0.066954, 0.176715));
            mesh.Positions.Add(new Point3D(0.216518, -0.066954, 0.124941));
            mesh.Positions.Add(new Point3D(0.241492, -0.066954, 0.064650));
            mesh.Positions.Add(new Point3D(0.129427, -0.017006, 0.000000));
            mesh.Positions.Add(new Point3D(0.125018, -0.017006, -0.033543));
            mesh.Positions.Add(new Point3D(0.112090, -0.017006, -0.064752));
            mesh.Positions.Add(new Point3D(0.091526, -0.017006, -0.091552));
            mesh.Positions.Add(new Point3D(0.064726, -0.017006, -0.112117));
            mesh.Positions.Add(new Point3D(0.033517, -0.017006, -0.125044));
            mesh.Positions.Add(new Point3D(-0.000000, -0.017006, -0.129453));
            mesh.Positions.Add(new Point3D(-0.000000, -0.017006, -0.125044));
            mesh.Positions.Add(new Point3D(-0.064676, -0.017006, -0.112117));
            mesh.Positions.Add(new Point3D(-0.091476, -0.017006, -0.091552));
            mesh.Positions.Add(new Point3D(-0.112040, -0.017006, -0.064752));
            mesh.Positions.Add(new Point3D(-0.124968, -0.017006, -0.033543));
            mesh.Positions.Add(new Point3D(-0.129377, -0.017006, 0.000000));
            mesh.Positions.Add(new Point3D(-0.124968, -0.017006, 0.033440));
            mesh.Positions.Add(new Point3D(-0.112040, -0.017006, 0.064650));
            mesh.Positions.Add(new Point3D(-0.091476, -0.017006, 0.091450));
            mesh.Positions.Add(new Point3D(-0.064676, -0.017006, 0.112014));
            mesh.Positions.Add(new Point3D(-0.000000, -0.017006, 0.124941));
            mesh.Positions.Add(new Point3D(-0.000000, -0.017006, 0.129351));
            mesh.Positions.Add(new Point3D(0.033517, -0.017006, 0.124941));
            mesh.Positions.Add(new Point3D(0.064726, -0.017006, 0.112014));
            mesh.Positions.Add(new Point3D(0.091526, -0.017006, 0.091450));
            mesh.Positions.Add(new Point3D(0.112090, -0.017006, 0.064650));
            mesh.Positions.Add(new Point3D(0.125018, -0.017006, 0.033440));
            mesh.Positions.Add(new Point3D(-0.000000, 0.000000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.583333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.750000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.833333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.916667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.990510, 0.130368, 0.043520));
            mesh.Normals.Add(new Vector3D(0.968018, 0.130356, -0.214355));
            mesh.Normals.Add(new Vector3D(0.879546, 0.130344, -0.457613));
            mesh.Normals.Add(new Vector3D(0.731137, 0.130344, -0.669664));
            mesh.Normals.Add(new Vector3D(0.532902, 0.130344, -0.836078));
            mesh.Normals.Add(new Vector3D(0.298292, 0.130344, -0.945533));
            mesh.Normals.Add(new Vector3D(0.043442, 0.130234, -0.990531));
            mesh.Normals.Add(new Vector3D(-0.214342, 0.130291, -0.968030));
            mesh.Normals.Add(new Vector3D(-0.457613, 0.130344, -0.879546));
            mesh.Normals.Add(new Vector3D(-0.669664, 0.130344, -0.731137));
            mesh.Normals.Add(new Vector3D(-0.836078, 0.130344, -0.532902));
            mesh.Normals.Add(new Vector3D(-0.945478, 0.130343, -0.298465));
            mesh.Normals.Add(new Vector3D(-0.990481, 0.130623, -0.043418));
            mesh.Normals.Add(new Vector3D(-0.967960, 0.130477, 0.214544));
            mesh.Normals.Add(new Vector3D(-0.879546, 0.130344, 0.457613));
            mesh.Normals.Add(new Vector3D(-0.731137, 0.130344, 0.669664));
            mesh.Normals.Add(new Vector3D(-0.532902, 0.130344, 0.836078));
            mesh.Normals.Add(new Vector3D(-0.298551, 0.130343, 0.945451));
            mesh.Normals.Add(new Vector3D(-0.043495, 0.130757, 0.990460));
            mesh.Normals.Add(new Vector3D(0.214557, 0.130541, 0.967949));
            mesh.Normals.Add(new Vector3D(0.457613, 0.130344, 0.879546));
            mesh.Normals.Add(new Vector3D(0.669664, 0.130344, 0.731137));
            mesh.Normals.Add(new Vector3D(0.836078, 0.130344, 0.532902));
            mesh.Normals.Add(new Vector3D(0.945506, 0.130344, 0.298378));
            mesh.Normals.Add(new Vector3D(0.965911, 0.258872, -0.001426));
            mesh.Normals.Add(new Vector3D(0.932627, 0.258861, -0.251393));
            mesh.Normals.Add(new Vector3D(0.835770, 0.258860, -0.484232));
            mesh.Normals.Add(new Vector3D(0.681963, 0.258860, -0.684045));
            mesh.Normals.Add(new Vector3D(0.481682, 0.258860, -0.837242));
            mesh.Normals.Add(new Vector3D(0.248538, 0.258835, -0.933399));
            mesh.Normals.Add(new Vector3D(-0.001470, 0.258807, -0.965928));
            mesh.Normals.Add(new Vector3D(-0.251431, 0.258860, -0.932617));
            mesh.Normals.Add(new Vector3D(-0.484232, 0.258860, -0.835770));
            mesh.Normals.Add(new Vector3D(-0.684045, 0.258860, -0.681963));
            mesh.Normals.Add(new Vector3D(-0.837242, 0.258860, -0.481682));
            mesh.Normals.Add(new Vector3D(-0.933353, 0.258926, -0.248614));
            mesh.Normals.Add(new Vector3D(-0.965877, 0.258995, 0.001593));
            mesh.Normals.Add(new Vector3D(-0.932613, 0.258860, 0.251444));
            mesh.Normals.Add(new Vector3D(-0.835770, 0.258860, 0.484232));
            mesh.Normals.Add(new Vector3D(-0.681963, 0.258860, 0.684045));
            mesh.Normals.Add(new Vector3D(-0.481682, 0.258860, 0.837242));
            mesh.Normals.Add(new Vector3D(-0.248681, 0.258957, 0.933327));
            mesh.Normals.Add(new Vector3D(0.001549, 0.259060, 0.965860));
            mesh.Normals.Add(new Vector3D(0.251406, 0.258861, 0.932623));
            mesh.Normals.Add(new Vector3D(0.484232, 0.258860, 0.835770));
            mesh.Normals.Add(new Vector3D(0.684045, 0.258860, 0.681963));
            mesh.Normals.Add(new Vector3D(0.837242, 0.258860, 0.481682));
            mesh.Normals.Add(new Vector3D(0.933372, 0.258866, 0.248605));
            mesh.Normals.Add(new Vector3D(0.865959, 0.500107, -0.002800));
            mesh.Normals.Add(new Vector3D(0.835720, 0.500108, -0.226856));
            mesh.Normals.Add(new Vector3D(0.748516, 0.500107, -0.435450));
            mesh.Normals.Add(new Vector3D(0.610308, 0.500107, -0.614342));
            mesh.Normals.Add(new Vector3D(0.430509, 0.500107, -0.751368));
            mesh.Normals.Add(new Vector3D(0.221360, 0.500108, -0.837193));
            mesh.Normals.Add(new Vector3D(-0.002878, 0.500107, -0.865959));
            mesh.Normals.Add(new Vector3D(-0.226894, 0.500107, -0.835711));
            mesh.Normals.Add(new Vector3D(-0.435450, 0.500107, -0.748516));
            mesh.Normals.Add(new Vector3D(-0.614342, 0.500107, -0.610308));
            mesh.Normals.Add(new Vector3D(-0.751368, 0.500107, -0.430509));
            mesh.Normals.Add(new Vector3D(-0.837196, 0.500108, -0.221347));
            mesh.Normals.Add(new Vector3D(-0.865959, 0.500107, 0.002904));
            mesh.Normals.Add(new Vector3D(-0.835707, 0.500106, 0.226908));
            mesh.Normals.Add(new Vector3D(-0.748516, 0.500107, 0.435450));
            mesh.Normals.Add(new Vector3D(-0.610308, 0.500107, 0.614342));
            mesh.Normals.Add(new Vector3D(-0.430509, 0.500107, 0.751368));
            mesh.Normals.Add(new Vector3D(-0.221384, 0.500107, 0.837187));
            mesh.Normals.Add(new Vector3D(0.002827, 0.500107, 0.865959));
            mesh.Normals.Add(new Vector3D(0.226869, 0.500108, 0.835717));
            mesh.Normals.Add(new Vector3D(0.435450, 0.500107, 0.748516));
            mesh.Normals.Add(new Vector3D(0.614342, 0.500107, 0.610308));
            mesh.Normals.Add(new Vector3D(0.751368, 0.500107, 0.430509));
            mesh.Normals.Add(new Vector3D(0.837184, 0.500106, 0.221396));
            mesh.Normals.Add(new Vector3D(0.706998, 0.707204, -0.003998));
            mesh.Normals.Add(new Vector3D(0.681865, 0.707206, -0.186870));
            mesh.Normals.Add(new Vector3D(0.610252, 0.707204, -0.357007));
            mesh.Normals.Add(new Vector3D(0.497058, 0.707204, -0.502787));
            mesh.Normals.Add(new Vector3D(0.349991, 0.707204, -0.614303));
            mesh.Normals.Add(new Vector3D(0.179060, 0.707205, -0.683958));
            mesh.Normals.Add(new Vector3D(-0.004076, 0.707204, -0.706997));
            mesh.Normals.Add(new Vector3D(-0.186910, 0.707204, -0.681856));
            mesh.Normals.Add(new Vector3D(-0.357007, 0.707204, -0.610252));
            mesh.Normals.Add(new Vector3D(-0.502787, 0.707204, -0.497058));
            mesh.Normals.Add(new Vector3D(-0.614303, 0.707204, -0.349991));
            mesh.Normals.Add(new Vector3D(-0.683961, 0.707206, -0.179047));
            mesh.Normals.Add(new Vector3D(-0.706997, 0.707204, 0.004103));
            mesh.Normals.Add(new Vector3D(-0.681852, 0.707203, 0.186924));
            mesh.Normals.Add(new Vector3D(-0.610252, 0.707204, 0.357007));
            mesh.Normals.Add(new Vector3D(-0.497058, 0.707204, 0.502787));
            mesh.Normals.Add(new Vector3D(-0.349991, 0.707204, 0.614303));
            mesh.Normals.Add(new Vector3D(-0.179084, 0.707204, 0.683953));
            mesh.Normals.Add(new Vector3D(0.004025, 0.707204, 0.706998));
            mesh.Normals.Add(new Vector3D(0.186884, 0.707205, 0.681862));
            mesh.Normals.Add(new Vector3D(0.357007, 0.707204, 0.610252));
            mesh.Normals.Add(new Vector3D(0.502787, 0.707204, 0.497058));
            mesh.Normals.Add(new Vector3D(0.614303, 0.707204, 0.349991));
            mesh.Normals.Add(new Vector3D(0.683950, 0.707203, 0.179096));
            mesh.Normals.Add(new Vector3D(0.499884, 0.866078, -0.004928));
            mesh.Normals.Add(new Vector3D(0.481567, 0.866079, -0.134162));
            mesh.Normals.Add(new Vector3D(0.430421, 0.866078, -0.254256));
            mesh.Normals.Add(new Vector3D(0.349948, 0.866078, -0.356994));
            mesh.Normals.Add(new Vector3D(0.245627, 0.866078, -0.435403));
            mesh.Normals.Add(new Vector3D(0.124555, 0.866079, -0.484142));
            mesh.Normals.Add(new Vector3D(-0.005009, 0.866078, -0.499883));
            mesh.Normals.Add(new Vector3D(-0.234085, 0.874054, -0.425715));
            mesh.Normals.Add(new Vector3D(-0.241015, 0.868868, -0.432412));
            mesh.Normals.Add(new Vector3D(-0.356994, 0.866078, -0.349948));
            mesh.Normals.Add(new Vector3D(-0.435403, 0.866078, -0.245627));
            mesh.Normals.Add(new Vector3D(-0.484144, 0.866079, -0.124543));
            mesh.Normals.Add(new Vector3D(-0.499883, 0.866078, 0.005037));
            mesh.Normals.Add(new Vector3D(-0.481554, 0.866077, 0.134221));
            mesh.Normals.Add(new Vector3D(-0.430421, 0.866078, 0.254256));
            mesh.Normals.Add(new Vector3D(-0.349948, 0.866078, 0.356994));
            mesh.Normals.Add(new Vector3D(-0.245627, 0.866078, 0.435403));
            mesh.Normals.Add(new Vector3D(-0.111607, 0.868541, 0.482887));
            mesh.Normals.Add(new Vector3D(-0.182661, 0.839565, 0.511630));
            mesh.Normals.Add(new Vector3D(0.134177, 0.866079, 0.481564));
            mesh.Normals.Add(new Vector3D(0.254256, 0.866078, 0.430421));
            mesh.Normals.Add(new Vector3D(0.356994, 0.866078, 0.349948));
            mesh.Normals.Add(new Vector3D(0.435403, 0.866078, 0.245627));
            mesh.Normals.Add(new Vector3D(0.484136, 0.866077, 0.124591));
            mesh.Normals.Add(new Vector3D(0.283979, 0.958777, -0.010106));
            mesh.Normals.Add(new Vector3D(0.271672, 0.958779, -0.083285));
            mesh.Normals.Add(new Vector3D(0.240845, 0.958780, -0.150778));
            mesh.Normals.Add(new Vector3D(0.193613, 0.958781, -0.207974));
            mesh.Normals.Add(new Vector3D(0.133187, 0.958781, -0.250997));
            mesh.Normals.Add(new Vector3D(0.063673, 0.958782, -0.276918));
            mesh.Normals.Add(new Vector3D(-0.392385, 0.893053, -0.220205));
            mesh.Normals.Add(new Vector3D(-0.444410, 0.872076, -0.204900));
            mesh.Normals.Add(new Vector3D(-0.132352, 0.958805, -0.251350));
            mesh.Normals.Add(new Vector3D(-0.207984, 0.958777, -0.193622));
            mesh.Normals.Add(new Vector3D(-0.251015, 0.958775, -0.133197));
            mesh.Normals.Add(new Vector3D(-0.276945, 0.958774, -0.063664));
            mesh.Normals.Add(new Vector3D(-0.283991, 0.958773, 0.010204));
            mesh.Normals.Add(new Vector3D(-0.271689, 0.958770, 0.083334));
            mesh.Normals.Add(new Vector3D(-0.240877, 0.958770, 0.150796));
            mesh.Normals.Add(new Vector3D(-0.193642, 0.958769, 0.208003));
            mesh.Normals.Add(new Vector3D(-0.113242, 0.960444, 0.254410));
            mesh.Normals.Add(new Vector3D(-0.585063, 0.776027, 0.235549));
            mesh.Normals.Add(new Vector3D(-0.534733, 0.805885, 0.254186));
            mesh.Normals.Add(new Vector3D(0.083304, 0.958770, 0.271699));
            mesh.Normals.Add(new Vector3D(0.150793, 0.958771, 0.240872));
            mesh.Normals.Add(new Vector3D(0.207993, 0.958773, 0.193633));
            mesh.Normals.Add(new Vector3D(0.251017, 0.958774, 0.133199));
            mesh.Normals.Add(new Vector3D(0.276929, 0.958775, 0.063720));
            mesh.Normals.Add(new Vector3D(-0.087672, 0.996149, 0.000029));
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 142, faceIndex + 143);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 141, faceIndex + 142);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 140, faceIndex + 141);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 139, faceIndex + 140);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 138, faceIndex + 139);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 137, faceIndex + 138);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 136, faceIndex + 137);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 135, faceIndex + 136);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 134, faceIndex + 135);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 133, faceIndex + 134);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 132, faceIndex + 133);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 131, faceIndex + 132);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 130, faceIndex + 131);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 129, faceIndex + 130);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 128, faceIndex + 129);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 127, faceIndex + 128);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 126, faceIndex + 127);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 125, faceIndex + 126);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 124, faceIndex + 125);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 123, faceIndex + 124);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 122, faceIndex + 123);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 121, faceIndex + 122);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 120, faceIndex + 121);
            AddTriangle(mesh.TriangleIndices, faceIndex + 144, faceIndex + 143, faceIndex + 120);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 25, faceIndex + 24, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 25);
            AddTriangle(mesh.TriangleIndices, faceIndex + 26, faceIndex + 25, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 26);
            AddTriangle(mesh.TriangleIndices, faceIndex + 27, faceIndex + 26, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 27);
            AddTriangle(mesh.TriangleIndices, faceIndex + 28, faceIndex + 27, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 5, faceIndex + 28);
            AddTriangle(mesh.TriangleIndices, faceIndex + 29, faceIndex + 28, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 29);
            AddTriangle(mesh.TriangleIndices, faceIndex + 30, faceIndex + 29, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 7, faceIndex + 30);
            AddTriangle(mesh.TriangleIndices, faceIndex + 31, faceIndex + 30, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 8, faceIndex + 31);
            AddTriangle(mesh.TriangleIndices, faceIndex + 32, faceIndex + 31, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 9, faceIndex + 32);
            AddTriangle(mesh.TriangleIndices, faceIndex + 33, faceIndex + 32, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 10, faceIndex + 33);
            AddTriangle(mesh.TriangleIndices, faceIndex + 34, faceIndex + 33, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 11, faceIndex + 34);
            AddTriangle(mesh.TriangleIndices, faceIndex + 35, faceIndex + 34, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 12, faceIndex + 35);
            AddTriangle(mesh.TriangleIndices, faceIndex + 36, faceIndex + 35, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 12, faceIndex + 13, faceIndex + 36);
            AddTriangle(mesh.TriangleIndices, faceIndex + 37, faceIndex + 36, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 13, faceIndex + 14, faceIndex + 37);
            AddTriangle(mesh.TriangleIndices, faceIndex + 38, faceIndex + 37, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 14, faceIndex + 15, faceIndex + 38);
            AddTriangle(mesh.TriangleIndices, faceIndex + 39, faceIndex + 38, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 15, faceIndex + 16, faceIndex + 39);
            AddTriangle(mesh.TriangleIndices, faceIndex + 40, faceIndex + 39, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 16, faceIndex + 17, faceIndex + 40);
            AddTriangle(mesh.TriangleIndices, faceIndex + 41, faceIndex + 40, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 17, faceIndex + 18, faceIndex + 41);
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 41, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 18, faceIndex + 19, faceIndex + 42);
            AddTriangle(mesh.TriangleIndices, faceIndex + 43, faceIndex + 42, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 19, faceIndex + 20, faceIndex + 43);
            AddTriangle(mesh.TriangleIndices, faceIndex + 44, faceIndex + 43, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 20, faceIndex + 21, faceIndex + 44);
            AddTriangle(mesh.TriangleIndices, faceIndex + 45, faceIndex + 44, faceIndex + 21);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 22, faceIndex + 45);
            AddTriangle(mesh.TriangleIndices, faceIndex + 46, faceIndex + 45, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 22, faceIndex + 23, faceIndex + 46);
            AddTriangle(mesh.TriangleIndices, faceIndex + 47, faceIndex + 46, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 0, faceIndex + 47);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 47, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 25, faceIndex + 48);
            AddTriangle(mesh.TriangleIndices, faceIndex + 49, faceIndex + 48, faceIndex + 25);
            AddTriangle(mesh.TriangleIndices, faceIndex + 25, faceIndex + 26, faceIndex + 49);
            AddTriangle(mesh.TriangleIndices, faceIndex + 50, faceIndex + 49, faceIndex + 26);
            AddTriangle(mesh.TriangleIndices, faceIndex + 26, faceIndex + 27, faceIndex + 50);
            AddTriangle(mesh.TriangleIndices, faceIndex + 51, faceIndex + 50, faceIndex + 27);
            AddTriangle(mesh.TriangleIndices, faceIndex + 27, faceIndex + 28, faceIndex + 51);
            AddTriangle(mesh.TriangleIndices, faceIndex + 52, faceIndex + 51, faceIndex + 28);
            AddTriangle(mesh.TriangleIndices, faceIndex + 28, faceIndex + 29, faceIndex + 52);
            AddTriangle(mesh.TriangleIndices, faceIndex + 53, faceIndex + 52, faceIndex + 29);
            AddTriangle(mesh.TriangleIndices, faceIndex + 29, faceIndex + 30, faceIndex + 53);
            AddTriangle(mesh.TriangleIndices, faceIndex + 54, faceIndex + 53, faceIndex + 30);
            AddTriangle(mesh.TriangleIndices, faceIndex + 30, faceIndex + 31, faceIndex + 54);
            AddTriangle(mesh.TriangleIndices, faceIndex + 55, faceIndex + 54, faceIndex + 31);
            AddTriangle(mesh.TriangleIndices, faceIndex + 31, faceIndex + 32, faceIndex + 55);
            AddTriangle(mesh.TriangleIndices, faceIndex + 56, faceIndex + 55, faceIndex + 32);
            AddTriangle(mesh.TriangleIndices, faceIndex + 32, faceIndex + 33, faceIndex + 56);
            AddTriangle(mesh.TriangleIndices, faceIndex + 57, faceIndex + 56, faceIndex + 33);
            AddTriangle(mesh.TriangleIndices, faceIndex + 33, faceIndex + 34, faceIndex + 57);
            AddTriangle(mesh.TriangleIndices, faceIndex + 58, faceIndex + 57, faceIndex + 34);
            AddTriangle(mesh.TriangleIndices, faceIndex + 34, faceIndex + 35, faceIndex + 58);
            AddTriangle(mesh.TriangleIndices, faceIndex + 59, faceIndex + 58, faceIndex + 35);
            AddTriangle(mesh.TriangleIndices, faceIndex + 35, faceIndex + 36, faceIndex + 59);
            AddTriangle(mesh.TriangleIndices, faceIndex + 60, faceIndex + 59, faceIndex + 36);
            AddTriangle(mesh.TriangleIndices, faceIndex + 36, faceIndex + 37, faceIndex + 60);
            AddTriangle(mesh.TriangleIndices, faceIndex + 61, faceIndex + 60, faceIndex + 37);
            AddTriangle(mesh.TriangleIndices, faceIndex + 37, faceIndex + 38, faceIndex + 61);
            AddTriangle(mesh.TriangleIndices, faceIndex + 62, faceIndex + 61, faceIndex + 38);
            AddTriangle(mesh.TriangleIndices, faceIndex + 38, faceIndex + 39, faceIndex + 62);
            AddTriangle(mesh.TriangleIndices, faceIndex + 63, faceIndex + 62, faceIndex + 39);
            AddTriangle(mesh.TriangleIndices, faceIndex + 39, faceIndex + 40, faceIndex + 63);
            AddTriangle(mesh.TriangleIndices, faceIndex + 64, faceIndex + 63, faceIndex + 40);
            AddTriangle(mesh.TriangleIndices, faceIndex + 40, faceIndex + 41, faceIndex + 64);
            AddTriangle(mesh.TriangleIndices, faceIndex + 65, faceIndex + 64, faceIndex + 41);
            AddTriangle(mesh.TriangleIndices, faceIndex + 41, faceIndex + 42, faceIndex + 65);
            AddTriangle(mesh.TriangleIndices, faceIndex + 66, faceIndex + 65, faceIndex + 42);
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 43, faceIndex + 66);
            AddTriangle(mesh.TriangleIndices, faceIndex + 67, faceIndex + 66, faceIndex + 43);
            AddTriangle(mesh.TriangleIndices, faceIndex + 43, faceIndex + 44, faceIndex + 67);
            AddTriangle(mesh.TriangleIndices, faceIndex + 68, faceIndex + 67, faceIndex + 44);
            AddTriangle(mesh.TriangleIndices, faceIndex + 44, faceIndex + 45, faceIndex + 68);
            AddTriangle(mesh.TriangleIndices, faceIndex + 69, faceIndex + 68, faceIndex + 45);
            AddTriangle(mesh.TriangleIndices, faceIndex + 45, faceIndex + 46, faceIndex + 69);
            AddTriangle(mesh.TriangleIndices, faceIndex + 70, faceIndex + 69, faceIndex + 46);
            AddTriangle(mesh.TriangleIndices, faceIndex + 46, faceIndex + 47, faceIndex + 70);
            AddTriangle(mesh.TriangleIndices, faceIndex + 71, faceIndex + 70, faceIndex + 47);
            AddTriangle(mesh.TriangleIndices, faceIndex + 47, faceIndex + 24, faceIndex + 71);
            AddTriangle(mesh.TriangleIndices, faceIndex + 48, faceIndex + 71, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 48, faceIndex + 49, faceIndex + 72);
            AddTriangle(mesh.TriangleIndices, faceIndex + 73, faceIndex + 72, faceIndex + 49);
            AddTriangle(mesh.TriangleIndices, faceIndex + 49, faceIndex + 50, faceIndex + 73);
            AddTriangle(mesh.TriangleIndices, faceIndex + 74, faceIndex + 73, faceIndex + 50);
            AddTriangle(mesh.TriangleIndices, faceIndex + 50, faceIndex + 51, faceIndex + 74);
            AddTriangle(mesh.TriangleIndices, faceIndex + 75, faceIndex + 74, faceIndex + 51);
            AddTriangle(mesh.TriangleIndices, faceIndex + 51, faceIndex + 52, faceIndex + 75);
            AddTriangle(mesh.TriangleIndices, faceIndex + 76, faceIndex + 75, faceIndex + 52);
            AddTriangle(mesh.TriangleIndices, faceIndex + 52, faceIndex + 53, faceIndex + 76);
            AddTriangle(mesh.TriangleIndices, faceIndex + 77, faceIndex + 76, faceIndex + 53);
            AddTriangle(mesh.TriangleIndices, faceIndex + 53, faceIndex + 54, faceIndex + 77);
            AddTriangle(mesh.TriangleIndices, faceIndex + 78, faceIndex + 77, faceIndex + 54);
            AddTriangle(mesh.TriangleIndices, faceIndex + 54, faceIndex + 55, faceIndex + 78);
            AddTriangle(mesh.TriangleIndices, faceIndex + 79, faceIndex + 78, faceIndex + 55);
            AddTriangle(mesh.TriangleIndices, faceIndex + 55, faceIndex + 56, faceIndex + 79);
            AddTriangle(mesh.TriangleIndices, faceIndex + 80, faceIndex + 79, faceIndex + 56);
            AddTriangle(mesh.TriangleIndices, faceIndex + 56, faceIndex + 57, faceIndex + 80);
            AddTriangle(mesh.TriangleIndices, faceIndex + 81, faceIndex + 80, faceIndex + 57);
            AddTriangle(mesh.TriangleIndices, faceIndex + 57, faceIndex + 58, faceIndex + 81);
            AddTriangle(mesh.TriangleIndices, faceIndex + 82, faceIndex + 81, faceIndex + 58);
            AddTriangle(mesh.TriangleIndices, faceIndex + 58, faceIndex + 59, faceIndex + 82);
            AddTriangle(mesh.TriangleIndices, faceIndex + 83, faceIndex + 82, faceIndex + 59);
            AddTriangle(mesh.TriangleIndices, faceIndex + 59, faceIndex + 60, faceIndex + 83);
            AddTriangle(mesh.TriangleIndices, faceIndex + 84, faceIndex + 83, faceIndex + 60);
            AddTriangle(mesh.TriangleIndices, faceIndex + 60, faceIndex + 61, faceIndex + 84);
            AddTriangle(mesh.TriangleIndices, faceIndex + 85, faceIndex + 84, faceIndex + 61);
            AddTriangle(mesh.TriangleIndices, faceIndex + 61, faceIndex + 62, faceIndex + 85);
            AddTriangle(mesh.TriangleIndices, faceIndex + 86, faceIndex + 85, faceIndex + 62);
            AddTriangle(mesh.TriangleIndices, faceIndex + 62, faceIndex + 63, faceIndex + 86);
            AddTriangle(mesh.TriangleIndices, faceIndex + 87, faceIndex + 86, faceIndex + 63);
            AddTriangle(mesh.TriangleIndices, faceIndex + 63, faceIndex + 64, faceIndex + 87);
            AddTriangle(mesh.TriangleIndices, faceIndex + 88, faceIndex + 87, faceIndex + 64);
            AddTriangle(mesh.TriangleIndices, faceIndex + 64, faceIndex + 65, faceIndex + 88);
            AddTriangle(mesh.TriangleIndices, faceIndex + 89, faceIndex + 88, faceIndex + 65);
            AddTriangle(mesh.TriangleIndices, faceIndex + 65, faceIndex + 66, faceIndex + 89);
            AddTriangle(mesh.TriangleIndices, faceIndex + 90, faceIndex + 89, faceIndex + 66);
            AddTriangle(mesh.TriangleIndices, faceIndex + 66, faceIndex + 67, faceIndex + 90);
            AddTriangle(mesh.TriangleIndices, faceIndex + 91, faceIndex + 90, faceIndex + 67);
            AddTriangle(mesh.TriangleIndices, faceIndex + 67, faceIndex + 68, faceIndex + 91);
            AddTriangle(mesh.TriangleIndices, faceIndex + 92, faceIndex + 91, faceIndex + 68);
            AddTriangle(mesh.TriangleIndices, faceIndex + 68, faceIndex + 69, faceIndex + 92);
            AddTriangle(mesh.TriangleIndices, faceIndex + 93, faceIndex + 92, faceIndex + 69);
            AddTriangle(mesh.TriangleIndices, faceIndex + 69, faceIndex + 70, faceIndex + 93);
            AddTriangle(mesh.TriangleIndices, faceIndex + 94, faceIndex + 93, faceIndex + 70);
            AddTriangle(mesh.TriangleIndices, faceIndex + 70, faceIndex + 71, faceIndex + 94);
            AddTriangle(mesh.TriangleIndices, faceIndex + 95, faceIndex + 94, faceIndex + 71);
            AddTriangle(mesh.TriangleIndices, faceIndex + 71, faceIndex + 48, faceIndex + 95);
            AddTriangle(mesh.TriangleIndices, faceIndex + 72, faceIndex + 95, faceIndex + 48);
            AddTriangle(mesh.TriangleIndices, faceIndex + 72, faceIndex + 73, faceIndex + 96);
            AddTriangle(mesh.TriangleIndices, faceIndex + 97, faceIndex + 96, faceIndex + 73);
            AddTriangle(mesh.TriangleIndices, faceIndex + 73, faceIndex + 74, faceIndex + 97);
            AddTriangle(mesh.TriangleIndices, faceIndex + 98, faceIndex + 97, faceIndex + 74);
            AddTriangle(mesh.TriangleIndices, faceIndex + 74, faceIndex + 75, faceIndex + 98);
            AddTriangle(mesh.TriangleIndices, faceIndex + 99, faceIndex + 98, faceIndex + 75);
            AddTriangle(mesh.TriangleIndices, faceIndex + 75, faceIndex + 76, faceIndex + 99);
            AddTriangle(mesh.TriangleIndices, faceIndex + 100, faceIndex + 99, faceIndex + 76);
            AddTriangle(mesh.TriangleIndices, faceIndex + 76, faceIndex + 77, faceIndex + 100);
            AddTriangle(mesh.TriangleIndices, faceIndex + 101, faceIndex + 100, faceIndex + 77);
            AddTriangle(mesh.TriangleIndices, faceIndex + 77, faceIndex + 78, faceIndex + 101);
            AddTriangle(mesh.TriangleIndices, faceIndex + 102, faceIndex + 101, faceIndex + 78);
            AddTriangle(mesh.TriangleIndices, faceIndex + 78, faceIndex + 79, faceIndex + 102);
            AddTriangle(mesh.TriangleIndices, faceIndex + 103, faceIndex + 102, faceIndex + 79);
            AddTriangle(mesh.TriangleIndices, faceIndex + 79, faceIndex + 80, faceIndex + 103);
            AddTriangle(mesh.TriangleIndices, faceIndex + 104, faceIndex + 103, faceIndex + 80);
            AddTriangle(mesh.TriangleIndices, faceIndex + 80, faceIndex + 81, faceIndex + 104);
            AddTriangle(mesh.TriangleIndices, faceIndex + 105, faceIndex + 104, faceIndex + 81);
            AddTriangle(mesh.TriangleIndices, faceIndex + 81, faceIndex + 82, faceIndex + 105);
            AddTriangle(mesh.TriangleIndices, faceIndex + 106, faceIndex + 105, faceIndex + 82);
            AddTriangle(mesh.TriangleIndices, faceIndex + 82, faceIndex + 83, faceIndex + 106);
            AddTriangle(mesh.TriangleIndices, faceIndex + 107, faceIndex + 106, faceIndex + 83);
            AddTriangle(mesh.TriangleIndices, faceIndex + 83, faceIndex + 84, faceIndex + 107);
            AddTriangle(mesh.TriangleIndices, faceIndex + 108, faceIndex + 107, faceIndex + 84);
            AddTriangle(mesh.TriangleIndices, faceIndex + 84, faceIndex + 85, faceIndex + 108);
            AddTriangle(mesh.TriangleIndices, faceIndex + 109, faceIndex + 108, faceIndex + 85);
            AddTriangle(mesh.TriangleIndices, faceIndex + 85, faceIndex + 86, faceIndex + 109);
            AddTriangle(mesh.TriangleIndices, faceIndex + 110, faceIndex + 109, faceIndex + 86);
            AddTriangle(mesh.TriangleIndices, faceIndex + 86, faceIndex + 87, faceIndex + 110);
            AddTriangle(mesh.TriangleIndices, faceIndex + 111, faceIndex + 110, faceIndex + 87);
            AddTriangle(mesh.TriangleIndices, faceIndex + 87, faceIndex + 88, faceIndex + 111);
            AddTriangle(mesh.TriangleIndices, faceIndex + 112, faceIndex + 111, faceIndex + 88);
            AddTriangle(mesh.TriangleIndices, faceIndex + 88, faceIndex + 89, faceIndex + 112);
            AddTriangle(mesh.TriangleIndices, faceIndex + 113, faceIndex + 112, faceIndex + 89);
            AddTriangle(mesh.TriangleIndices, faceIndex + 89, faceIndex + 90, faceIndex + 113);
            AddTriangle(mesh.TriangleIndices, faceIndex + 114, faceIndex + 113, faceIndex + 90);
            AddTriangle(mesh.TriangleIndices, faceIndex + 90, faceIndex + 91, faceIndex + 114);
            AddTriangle(mesh.TriangleIndices, faceIndex + 115, faceIndex + 114, faceIndex + 91);
            AddTriangle(mesh.TriangleIndices, faceIndex + 91, faceIndex + 92, faceIndex + 115);
            AddTriangle(mesh.TriangleIndices, faceIndex + 116, faceIndex + 115, faceIndex + 92);
            AddTriangle(mesh.TriangleIndices, faceIndex + 92, faceIndex + 93, faceIndex + 116);
            AddTriangle(mesh.TriangleIndices, faceIndex + 117, faceIndex + 116, faceIndex + 93);
            AddTriangle(mesh.TriangleIndices, faceIndex + 93, faceIndex + 94, faceIndex + 117);
            AddTriangle(mesh.TriangleIndices, faceIndex + 118, faceIndex + 117, faceIndex + 94);
            AddTriangle(mesh.TriangleIndices, faceIndex + 94, faceIndex + 95, faceIndex + 118);
            AddTriangle(mesh.TriangleIndices, faceIndex + 119, faceIndex + 118, faceIndex + 95);
            AddTriangle(mesh.TriangleIndices, faceIndex + 95, faceIndex + 72, faceIndex + 119);
            AddTriangle(mesh.TriangleIndices, faceIndex + 96, faceIndex + 119, faceIndex + 72);
            AddTriangle(mesh.TriangleIndices, faceIndex + 96, faceIndex + 97, faceIndex + 120);
            AddTriangle(mesh.TriangleIndices, faceIndex + 121, faceIndex + 120, faceIndex + 97);
            AddTriangle(mesh.TriangleIndices, faceIndex + 97, faceIndex + 98, faceIndex + 121);
            AddTriangle(mesh.TriangleIndices, faceIndex + 122, faceIndex + 121, faceIndex + 98);
            AddTriangle(mesh.TriangleIndices, faceIndex + 98, faceIndex + 99, faceIndex + 122);
            AddTriangle(mesh.TriangleIndices, faceIndex + 123, faceIndex + 122, faceIndex + 99);
            AddTriangle(mesh.TriangleIndices, faceIndex + 99, faceIndex + 100, faceIndex + 123);
            AddTriangle(mesh.TriangleIndices, faceIndex + 124, faceIndex + 123, faceIndex + 100);
            AddTriangle(mesh.TriangleIndices, faceIndex + 100, faceIndex + 101, faceIndex + 124);
            AddTriangle(mesh.TriangleIndices, faceIndex + 125, faceIndex + 124, faceIndex + 101);
            AddTriangle(mesh.TriangleIndices, faceIndex + 101, faceIndex + 102, faceIndex + 125);
            AddTriangle(mesh.TriangleIndices, faceIndex + 126, faceIndex + 125, faceIndex + 102);
            AddTriangle(mesh.TriangleIndices, faceIndex + 102, faceIndex + 103, faceIndex + 126);
            AddTriangle(mesh.TriangleIndices, faceIndex + 127, faceIndex + 126, faceIndex + 103);
            AddTriangle(mesh.TriangleIndices, faceIndex + 103, faceIndex + 104, faceIndex + 127);
            AddTriangle(mesh.TriangleIndices, faceIndex + 128, faceIndex + 127, faceIndex + 104);
            AddTriangle(mesh.TriangleIndices, faceIndex + 104, faceIndex + 105, faceIndex + 128);
            AddTriangle(mesh.TriangleIndices, faceIndex + 129, faceIndex + 128, faceIndex + 105);
            AddTriangle(mesh.TriangleIndices, faceIndex + 105, faceIndex + 106, faceIndex + 129);
            AddTriangle(mesh.TriangleIndices, faceIndex + 130, faceIndex + 129, faceIndex + 106);
            AddTriangle(mesh.TriangleIndices, faceIndex + 106, faceIndex + 107, faceIndex + 130);
            AddTriangle(mesh.TriangleIndices, faceIndex + 131, faceIndex + 130, faceIndex + 107);
            AddTriangle(mesh.TriangleIndices, faceIndex + 107, faceIndex + 108, faceIndex + 131);
            AddTriangle(mesh.TriangleIndices, faceIndex + 132, faceIndex + 131, faceIndex + 108);
            AddTriangle(mesh.TriangleIndices, faceIndex + 108, faceIndex + 109, faceIndex + 132);
            AddTriangle(mesh.TriangleIndices, faceIndex + 133, faceIndex + 132, faceIndex + 109);
            AddTriangle(mesh.TriangleIndices, faceIndex + 109, faceIndex + 110, faceIndex + 133);
            AddTriangle(mesh.TriangleIndices, faceIndex + 134, faceIndex + 133, faceIndex + 110);
            AddTriangle(mesh.TriangleIndices, faceIndex + 110, faceIndex + 111, faceIndex + 134);
            AddTriangle(mesh.TriangleIndices, faceIndex + 135, faceIndex + 134, faceIndex + 111);
            AddTriangle(mesh.TriangleIndices, faceIndex + 111, faceIndex + 112, faceIndex + 135);
            AddTriangle(mesh.TriangleIndices, faceIndex + 136, faceIndex + 135, faceIndex + 112);
            AddTriangle(mesh.TriangleIndices, faceIndex + 112, faceIndex + 113, faceIndex + 136);
            AddTriangle(mesh.TriangleIndices, faceIndex + 137, faceIndex + 136, faceIndex + 113);
            AddTriangle(mesh.TriangleIndices, faceIndex + 113, faceIndex + 114, faceIndex + 137);
            AddTriangle(mesh.TriangleIndices, faceIndex + 138, faceIndex + 137, faceIndex + 114);
            AddTriangle(mesh.TriangleIndices, faceIndex + 114, faceIndex + 115, faceIndex + 138);
            AddTriangle(mesh.TriangleIndices, faceIndex + 139, faceIndex + 138, faceIndex + 115);
            AddTriangle(mesh.TriangleIndices, faceIndex + 115, faceIndex + 116, faceIndex + 139);
            AddTriangle(mesh.TriangleIndices, faceIndex + 140, faceIndex + 139, faceIndex + 116);
            AddTriangle(mesh.TriangleIndices, faceIndex + 116, faceIndex + 117, faceIndex + 140);
            AddTriangle(mesh.TriangleIndices, faceIndex + 141, faceIndex + 140, faceIndex + 117);
            AddTriangle(mesh.TriangleIndices, faceIndex + 117, faceIndex + 118, faceIndex + 141);
            AddTriangle(mesh.TriangleIndices, faceIndex + 142, faceIndex + 141, faceIndex + 118);
            AddTriangle(mesh.TriangleIndices, faceIndex + 118, faceIndex + 119, faceIndex + 142);
            AddTriangle(mesh.TriangleIndices, faceIndex + 143, faceIndex + 142, faceIndex + 119);
            AddTriangle(mesh.TriangleIndices, faceIndex + 119, faceIndex + 96, faceIndex + 143);
            AddTriangle(mesh.TriangleIndices, faceIndex + 120, faceIndex + 143, faceIndex + 96);
            faceIndex += 145;
            // Bottom
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, 0.482883));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 24, faceIndex + 1);
            return faceIndex;
        }
        private int AddGeometry_CylinderEndB(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Top
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, 0.129351));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.500000));
            mesh.Normals.Add(new Vector3D(-0.000008, 1.000000, 0.000017));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, 0.000046));
            mesh.Normals.Add(new Vector3D(0.863950, 0.447219, -0.231487));
            mesh.Normals.Add(new Vector3D(0.774591, 0.447226, -0.447210));
            mesh.Normals.Add(new Vector3D(0.632449, 0.447231, -0.632449));
            mesh.Normals.Add(new Vector3D(0.447209, 0.447233, -0.774588));
            mesh.Normals.Add(new Vector3D(0.231410, 0.447232, -0.863963));
            mesh.Normals.Add(new Vector3D(-0.000022, 0.447214, -0.894427));
            mesh.Normals.Add(new Vector3D(-0.231434, 0.447223, -0.863962));
            mesh.Normals.Add(new Vector3D(-0.447213, 0.447215, -0.774596));
            mesh.Normals.Add(new Vector3D(-0.632459, 0.447206, -0.632458));
            mesh.Normals.Add(new Vector3D(-0.774605, 0.447195, -0.447218));
            mesh.Normals.Add(new Vector3D(-0.863922, 0.447185, -0.231656));
            mesh.Normals.Add(new Vector3D(-0.894427, 0.447214, 0.000046));
            mesh.Normals.Add(new Vector3D(-0.863919, 0.447166, 0.231704));
            mesh.Normals.Add(new Vector3D(-0.774620, 0.447159, 0.447228));
            mesh.Normals.Add(new Vector3D(-0.632477, 0.447154, 0.632477));
            mesh.Normals.Add(new Vector3D(-0.447229, 0.447152, 0.774624));
            mesh.Normals.Add(new Vector3D(-0.231780, 0.447152, 0.863906));
            mesh.Normals.Add(new Vector3D(-0.000023, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(0.231757, 0.447162, 0.863907));
            mesh.Normals.Add(new Vector3D(0.447224, 0.447170, 0.774616));
            mesh.Normals.Add(new Vector3D(0.632468, 0.447179, 0.632468));
            mesh.Normals.Add(new Vector3D(0.774607, 0.447189, 0.447220));
            mesh.Normals.Add(new Vector3D(0.863947, 0.447200, 0.231535));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 24, faceIndex + 1);
            faceIndex += 25;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, 0.482883));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 24, faceIndex + 1);
            return faceIndex;
        }
        private int AddGeometry_CylinderEndC(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Top
            mesh.Positions.Add(new Point3D(0.000000, 0.000000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, 0.129351));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.500000));
            mesh.Normals.Add(new Vector3D(-0.000011, 1.000000, 0.000022));
            mesh.Normals.Add(new Vector3D(0.707107, 0.707107, 0.000037));
            mesh.Normals.Add(new Vector3D(0.683009, 0.707112, -0.183005));
            mesh.Normals.Add(new Vector3D(0.612362, 0.707119, -0.353547));
            mesh.Normals.Add(new Vector3D(0.499988, 0.707124, -0.499988));
            mesh.Normals.Add(new Vector3D(0.353544, 0.707126, -0.612356));
            mesh.Normals.Add(new Vector3D(0.182943, 0.707125, -0.683012));
            mesh.Normals.Add(new Vector3D(-0.000018, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.182963, 0.707116, -0.683016));
            mesh.Normals.Add(new Vector3D(-0.353553, 0.707108, -0.612371));
            mesh.Normals.Add(new Vector3D(-0.500006, 0.707099, -0.500005));
            mesh.Normals.Add(new Vector3D(-0.612389, 0.707089, -0.353562));
            mesh.Normals.Add(new Vector3D(-0.683007, 0.707078, -0.183145));
            mesh.Normals.Add(new Vector3D(-0.707107, 0.707107, 0.000037));
            mesh.Normals.Add(new Vector3D(-0.683015, 0.707059, 0.183186));
            mesh.Normals.Add(new Vector3D(-0.612419, 0.707052, 0.353581));
            mesh.Normals.Add(new Vector3D(-0.500042, 0.707048, 0.500042));
            mesh.Normals.Add(new Vector3D(-0.353584, 0.707046, 0.612425));
            mesh.Normals.Add(new Vector3D(-0.183249, 0.707046, 0.683012));
            mesh.Normals.Add(new Vector3D(-0.000018, 0.707107, 0.707107));
            mesh.Normals.Add(new Vector3D(0.183229, 0.707055, 0.683008));
            mesh.Normals.Add(new Vector3D(0.353575, 0.707063, 0.612410));
            mesh.Normals.Add(new Vector3D(0.500024, 0.707073, 0.500025));
            mesh.Normals.Add(new Vector3D(0.612393, 0.707083, 0.353566));
            mesh.Normals.Add(new Vector3D(0.683017, 0.707093, 0.183047));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 24, faceIndex + 1);
            faceIndex += 25;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, 0.482883));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482909, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(-0.432962, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.353507, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(-0.129377, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, -0.482986));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, -0.433038));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, -0.353584));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, -0.129453));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.482959, -0.500000, 0.129351));
            mesh.Positions.Add(new Point3D(0.433012, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.353557, -0.500000, 0.353481));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.432936));
            mesh.Positions.Add(new Point3D(0.129427, -0.500000, 0.482883));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 24, faceIndex + 1);
            return faceIndex;
        }
        private int AddGeometry_CornerLargeB(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            faceIndex += 5;
            // Right
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            faceIndex += 5;
            // Left
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, 0.447214));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, 0.447214));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, 0.447214));
            mesh.Normals.Add(new Vector3D(0.447214, 0.894427, -0.000000));
            mesh.Normals.Add(new Vector3D(0.447214, 0.894427, -0.000000));
            mesh.Normals.Add(new Vector3D(0.447214, 0.894427, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 5);
            faceIndex += 6;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLargeC(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Front
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.707107, 0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, 0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, 0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, 0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, 0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, 0.000000, 0.707107));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            faceIndex += 6;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.408248, 0.816497, 0.408248));
            mesh.Normals.Add(new Vector3D(0.408248, 0.816497, 0.408248));
            mesh.Normals.Add(new Vector3D(0.408248, 0.816497, 0.408248));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            faceIndex += 3;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            return faceIndex;
        }
        private int AddGeometry_CornerLargeD(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Front
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, 0.894427));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLongE(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Front
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Right
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Bottom
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_PyramidA(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, 0.894427));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);

            //BuildingBlockThin
            return faceIndex;
        }
        private int AddGeometry_Wall(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354361));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_WallLShape(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.354335, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 1, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 0, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 6;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 4, faceIndex + 5);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.354335, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 2, faceIndex + 1);
            faceIndex += 6;
            // Bottom
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 2, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 0, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 3);
            return faceIndex;
        }
        private int AddGeometry_SlopedWall(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(-0.354285, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, -0.707107));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallBottomRight(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(-0.354285, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallTopRight(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354285, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallBottomLeft(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(0.354335, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallTopLeft(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(0.000000, 0.894427, -0.447214));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            return faceIndex;
        }
        private int AddGeometry_Wall3Corner(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.250015, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250015, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, 0.250015, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250015, 0.354258));
            mesh.Positions.Add(new Point3D(0.354335, 0.250015, 0.354258));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250015, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.707107, -0.707107, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.577350, -0.577350, -0.577350));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 5, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 7, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 8, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 10, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 12, faceIndex + 9, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 6, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 10, faceIndex + 11);
            faceIndex += 13;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250015, 0.354258));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250015, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 6;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354335, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 5);
            return faceIndex;
        }
        private int AddGeometry_WallHalf(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354361));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354361));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CubeHalf(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampTopDouble(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.382683, -0.923880));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.382683, -0.923880));
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 0, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 6, faceIndex + 4);
            faceIndex += 7;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            faceIndex += 5;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 3);
            faceIndex += 5;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottomA(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.350000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.350000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.350000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.350000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.350000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.350000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.988936, -0.148340));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.988936, -0.148340));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.988936, -0.148340));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.988936, -0.148340));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottomB(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.350000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.350000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.350000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.350000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.350000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.350000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.943858, -0.330350));
            mesh.Normals.Add(new Vector3D(0.000000, 0.943858, -0.330350));
            mesh.Normals.Add(new Vector3D(0.000000, 0.943858, -0.330350));
            mesh.Normals.Add(new Vector3D(0.000000, 0.943858, -0.330350));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottomC(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, -0.707107));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampWedgeBottom(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.408248, 0.816497, -0.408248));
            mesh.Normals.Add(new Vector3D(-0.408248, 0.816497, -0.408248));
            mesh.Normals.Add(new Vector3D(-0.408248, 0.816497, -0.408248));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            faceIndex += 6;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_Beam(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391338, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391338, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.391338, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.391338, -0.500000, 0.354258));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.354258));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(0.391338, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391338, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.391338, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391338, -0.500000, 0.354258));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(0.391338, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391338, 0.500000, 0.354258));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(0.391338, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.354258));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391338, -0.500000, 0.354258));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CylinderThin(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Front
            mesh.Positions.Add(new Point3D(0.241492, -0.500000, -0.064752));
            mesh.Positions.Add(new Point3D(0.216518, -0.500000, -0.125044));
            mesh.Positions.Add(new Point3D(0.176791, -0.500000, -0.176817));
            mesh.Positions.Add(new Point3D(0.125018, -0.500000, -0.216545));
            mesh.Positions.Add(new Point3D(0.064726, -0.500000, -0.241518));
            mesh.Positions.Add(new Point3D(-0.064676, -0.500000, -0.241518));
            mesh.Positions.Add(new Point3D(-0.124968, -0.500000, -0.216545));
            mesh.Positions.Add(new Point3D(-0.176741, -0.500000, -0.176817));
            mesh.Positions.Add(new Point3D(-0.216468, -0.500000, -0.125044));
            mesh.Positions.Add(new Point3D(-0.241442, -0.500000, -0.064752));
            mesh.Positions.Add(new Point3D(-0.241442, -0.500000, 0.064650));
            mesh.Positions.Add(new Point3D(-0.216468, -0.500000, 0.124941));
            mesh.Positions.Add(new Point3D(-0.176741, -0.500000, 0.176715));
            mesh.Positions.Add(new Point3D(-0.124968, -0.500000, 0.216442));
            mesh.Positions.Add(new Point3D(-0.064676, -0.500000, 0.241416));
            mesh.Positions.Add(new Point3D(0.064726, -0.500000, 0.241416));
            mesh.Positions.Add(new Point3D(0.125018, -0.500000, 0.216442));
            mesh.Positions.Add(new Point3D(0.176791, -0.500000, 0.176715));
            mesh.Positions.Add(new Point3D(0.216518, -0.500000, 0.124941));
            mesh.Positions.Add(new Point3D(0.241492, -0.500000, 0.064650));
            mesh.Positions.Add(new Point3D(0.241492, 0.500000, -0.064752));
            mesh.Positions.Add(new Point3D(0.216518, 0.500000, -0.125044));
            mesh.Positions.Add(new Point3D(0.176791, 0.500000, -0.176817));
            mesh.Positions.Add(new Point3D(0.125018, 0.500000, -0.216545));
            mesh.Positions.Add(new Point3D(0.064726, 0.500000, -0.241518));
            mesh.Positions.Add(new Point3D(-0.064676, 0.500000, -0.241518));
            mesh.Positions.Add(new Point3D(-0.124968, 0.500000, -0.216545));
            mesh.Positions.Add(new Point3D(-0.176741, 0.500000, -0.176817));
            mesh.Positions.Add(new Point3D(-0.216468, 0.500000, -0.125044));
            mesh.Positions.Add(new Point3D(-0.241442, 0.500000, -0.064752));
            mesh.Positions.Add(new Point3D(-0.241442, 0.500000, 0.064650));
            mesh.Positions.Add(new Point3D(-0.216468, 0.500000, 0.124941));
            mesh.Positions.Add(new Point3D(-0.176741, 0.500000, 0.176715));
            mesh.Positions.Add(new Point3D(-0.124968, 0.500000, 0.216442));
            mesh.Positions.Add(new Point3D(-0.064676, 0.500000, 0.241416));
            mesh.Positions.Add(new Point3D(0.064726, 0.500000, 0.241416));
            mesh.Positions.Add(new Point3D(0.125018, 0.500000, 0.216442));
            mesh.Positions.Add(new Point3D(0.176791, 0.500000, 0.176715));
            mesh.Positions.Add(new Point3D(0.216518, 0.500000, 0.124941));
            mesh.Positions.Add(new Point3D(0.241492, 0.500000, 0.064650));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.249960, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.250010, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.041667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.083333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.125000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.166667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.208333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.541667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.583333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.625000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.666667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.708333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.791667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.833333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.875000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.916667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.958333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.041667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.083333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.125000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.166667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.208333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.291667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.333333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.375000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.416667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.458333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.541667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.583333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.625000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.666667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.708333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.791667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.833333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.875000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.916667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.958333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.750000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.750000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.500000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.500000, 0.333333));
            mesh.Normals.Add(new Vector3D(0.976359, 0.000000, -0.216155));
            mesh.Normals.Add(new Vector3D(0.887114, 0.000000, -0.461551));
            mesh.Normals.Add(new Vector3D(0.737428, 0.000000, -0.675426));
            mesh.Normals.Add(new Vector3D(0.537487, 0.000000, -0.843272));
            mesh.Normals.Add(new Vector3D(0.300903, 0.000000, -0.953655));
            mesh.Normals.Add(new Vector3D(-0.216255, 0.000000, -0.976337));
            mesh.Normals.Add(new Vector3D(-0.461551, 0.000000, -0.887114));
            mesh.Normals.Add(new Vector3D(-0.675426, 0.000000, -0.737428));
            mesh.Normals.Add(new Vector3D(-0.843272, 0.000000, -0.537487));
            mesh.Normals.Add(new Vector3D(-0.953660, 0.000000, -0.300886));
            mesh.Normals.Add(new Vector3D(-0.976329, -0.000000, 0.216289));
            mesh.Normals.Add(new Vector3D(-0.887114, -0.000000, 0.461551));
            mesh.Normals.Add(new Vector3D(-0.737428, -0.000000, 0.675426));
            mesh.Normals.Add(new Vector3D(-0.537487, -0.000000, 0.843272));
            mesh.Normals.Add(new Vector3D(-0.300934, -0.000000, 0.953645));
            mesh.Normals.Add(new Vector3D(0.216189, -0.000000, 0.976351));
            mesh.Normals.Add(new Vector3D(0.461551, -0.000000, 0.887114));
            mesh.Normals.Add(new Vector3D(0.675426, -0.000000, 0.737428));
            mesh.Normals.Add(new Vector3D(0.843272, -0.000000, 0.537487));
            mesh.Normals.Add(new Vector3D(0.953640, -0.000000, 0.300951));
            mesh.Normals.Add(new Vector3D(0.953660, 0.000000, -0.300886));
            mesh.Normals.Add(new Vector3D(0.843272, 0.000000, -0.537487));
            mesh.Normals.Add(new Vector3D(0.675426, 0.000000, -0.737428));
            mesh.Normals.Add(new Vector3D(0.461551, 0.000000, -0.887114));
            mesh.Normals.Add(new Vector3D(0.216189, 0.000000, -0.976351));
            mesh.Normals.Add(new Vector3D(-0.300934, 0.000000, -0.953645));
            mesh.Normals.Add(new Vector3D(-0.537487, 0.000000, -0.843272));
            mesh.Normals.Add(new Vector3D(-0.737428, 0.000000, -0.675426));
            mesh.Normals.Add(new Vector3D(-0.887114, 0.000000, -0.461551));
            mesh.Normals.Add(new Vector3D(-0.976359, 0.000000, -0.216155));
            mesh.Normals.Add(new Vector3D(-0.953640, -0.000000, 0.300951));
            mesh.Normals.Add(new Vector3D(-0.843272, -0.000000, 0.537487));
            mesh.Normals.Add(new Vector3D(-0.675426, -0.000000, 0.737428));
            mesh.Normals.Add(new Vector3D(-0.461551, -0.000000, 0.887114));
            mesh.Normals.Add(new Vector3D(-0.216255, -0.000000, 0.976337));
            mesh.Normals.Add(new Vector3D(0.300903, -0.000000, 0.953655));
            mesh.Normals.Add(new Vector3D(0.537487, -0.000000, 0.843272));
            mesh.Normals.Add(new Vector3D(0.737428, -0.000000, 0.675426));
            mesh.Normals.Add(new Vector3D(0.887114, -0.000000, 0.461551));
            mesh.Normals.Add(new Vector3D(0.976329, -0.000000, 0.216289));
            mesh.Normals.Add(new Vector3D(-0.043892, -0.000000, 0.999036));
            mesh.Normals.Add(new Vector3D(0.043792, -0.000000, 0.999041));
            mesh.Normals.Add(new Vector3D(-0.043892, 0.000000, -0.999036));
            mesh.Normals.Add(new Vector3D(0.043792, 0.000000, -0.999041));
            mesh.Normals.Add(new Vector3D(-0.999034, -0.000000, 0.043944));
            mesh.Normals.Add(new Vector3D(-0.999043, 0.000000, -0.043740));
            mesh.Normals.Add(new Vector3D(0.999043, 0.000000, -0.043740));
            mesh.Normals.Add(new Vector3D(0.999034, -0.000000, 0.043944));
            AddTriangle(mesh.TriangleIndices, faceIndex + 47, faceIndex + 0, faceIndex + 46);
            AddTriangle(mesh.TriangleIndices, faceIndex + 20, faceIndex + 46, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 20, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 21);
            AddTriangle(mesh.TriangleIndices, faceIndex + 22, faceIndex + 21, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 22);
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 22, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 23, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 43, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 42, faceIndex + 24, faceIndex + 43);
            AddTriangle(mesh.TriangleIndices, faceIndex + 43, faceIndex + 5, faceIndex + 42);
            AddTriangle(mesh.TriangleIndices, faceIndex + 25, faceIndex + 42, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 25);
            AddTriangle(mesh.TriangleIndices, faceIndex + 26, faceIndex + 25, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 7, faceIndex + 26);
            AddTriangle(mesh.TriangleIndices, faceIndex + 27, faceIndex + 26, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 8, faceIndex + 27);
            AddTriangle(mesh.TriangleIndices, faceIndex + 28, faceIndex + 27, faceIndex + 8);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 9, faceIndex + 28);
            AddTriangle(mesh.TriangleIndices, faceIndex + 29, faceIndex + 28, faceIndex + 9);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 45, faceIndex + 29);
            AddTriangle(mesh.TriangleIndices, faceIndex + 44, faceIndex + 29, faceIndex + 45);
            AddTriangle(mesh.TriangleIndices, faceIndex + 45, faceIndex + 10, faceIndex + 44);
            AddTriangle(mesh.TriangleIndices, faceIndex + 30, faceIndex + 44, faceIndex + 10);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 11, faceIndex + 30);
            AddTriangle(mesh.TriangleIndices, faceIndex + 31, faceIndex + 30, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 12, faceIndex + 31);
            AddTriangle(mesh.TriangleIndices, faceIndex + 32, faceIndex + 31, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 12, faceIndex + 13, faceIndex + 32);
            AddTriangle(mesh.TriangleIndices, faceIndex + 33, faceIndex + 32, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 13, faceIndex + 14, faceIndex + 33);
            AddTriangle(mesh.TriangleIndices, faceIndex + 34, faceIndex + 33, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 14, faceIndex + 40, faceIndex + 34);
            AddTriangle(mesh.TriangleIndices, faceIndex + 41, faceIndex + 34, faceIndex + 40);
            AddTriangle(mesh.TriangleIndices, faceIndex + 40, faceIndex + 15, faceIndex + 41);
            AddTriangle(mesh.TriangleIndices, faceIndex + 35, faceIndex + 41, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 15, faceIndex + 16, faceIndex + 35);
            AddTriangle(mesh.TriangleIndices, faceIndex + 36, faceIndex + 35, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 16, faceIndex + 17, faceIndex + 36);
            AddTriangle(mesh.TriangleIndices, faceIndex + 37, faceIndex + 36, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 17, faceIndex + 18, faceIndex + 37);
            AddTriangle(mesh.TriangleIndices, faceIndex + 38, faceIndex + 37, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 18, faceIndex + 19, faceIndex + 38);
            AddTriangle(mesh.TriangleIndices, faceIndex + 39, faceIndex + 38, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 19, faceIndex + 47, faceIndex + 39);
            AddTriangle(mesh.TriangleIndices, faceIndex + 46, faceIndex + 39, faceIndex + 47);
            faceIndex += 48;
            // Top
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.241492, 0.500000, -0.064752));
            mesh.Positions.Add(new Point3D(0.216518, 0.500000, -0.125044));
            mesh.Positions.Add(new Point3D(0.176791, 0.500000, -0.176817));
            mesh.Positions.Add(new Point3D(0.125018, 0.500000, -0.216545));
            mesh.Positions.Add(new Point3D(0.064726, 0.500000, -0.241518));
            mesh.Positions.Add(new Point3D(-0.064676, 0.500000, -0.241518));
            mesh.Positions.Add(new Point3D(-0.124968, 0.500000, -0.216545));
            mesh.Positions.Add(new Point3D(-0.176741, 0.500000, -0.176817));
            mesh.Positions.Add(new Point3D(-0.216468, 0.500000, -0.125044));
            mesh.Positions.Add(new Point3D(-0.241442, 0.500000, -0.064752));
            mesh.Positions.Add(new Point3D(-0.241442, 0.500000, 0.064650));
            mesh.Positions.Add(new Point3D(-0.216468, 0.500000, 0.124941));
            mesh.Positions.Add(new Point3D(-0.176741, 0.500000, 0.176715));
            mesh.Positions.Add(new Point3D(-0.124968, 0.500000, 0.216442));
            mesh.Positions.Add(new Point3D(-0.064676, 0.500000, 0.241416));
            mesh.Positions.Add(new Point3D(0.064726, 0.500000, 0.241416));
            mesh.Positions.Add(new Point3D(0.125018, 0.500000, 0.216442));
            mesh.Positions.Add(new Point3D(0.176791, 0.500000, 0.176715));
            mesh.Positions.Add(new Point3D(0.216518, 0.500000, 0.124941));
            mesh.Positions.Add(new Point3D(0.241492, 0.500000, 0.064650));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.250036));
            mesh.Positions.Add(new Point3D(-0.249960, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.250010, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.249934));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.291667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.333333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.375000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.416667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.458333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.541667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.583333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.625000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.666667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.708333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.791667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.833333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.875000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.916667, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.958333, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.500000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.666667));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.750000, 0.666667));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 0, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 23, faceIndex + 1, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 15, faceIndex + 24);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 6, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 7, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 8, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 9, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 10, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 22, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 22, faceIndex + 11);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 24, faceIndex + 16);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 5, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 21, faceIndex + 0);
            faceIndex += 25;
            // Bottom
            mesh.Positions.Add(new Point3D(0.241492, -0.500000, -0.064752));
            mesh.Positions.Add(new Point3D(0.216518, -0.500000, -0.125044));
            mesh.Positions.Add(new Point3D(0.176791, -0.500000, -0.176817));
            mesh.Positions.Add(new Point3D(0.125018, -0.500000, -0.216545));
            mesh.Positions.Add(new Point3D(0.064726, -0.500000, -0.241518));
            mesh.Positions.Add(new Point3D(-0.064676, -0.500000, -0.241518));
            mesh.Positions.Add(new Point3D(-0.124968, -0.500000, -0.216545));
            mesh.Positions.Add(new Point3D(-0.176741, -0.500000, -0.176817));
            mesh.Positions.Add(new Point3D(-0.216468, -0.500000, -0.125044));
            mesh.Positions.Add(new Point3D(-0.241442, -0.500000, -0.064752));
            mesh.Positions.Add(new Point3D(-0.241442, -0.500000, 0.064650));
            mesh.Positions.Add(new Point3D(-0.216468, -0.500000, 0.124941));
            mesh.Positions.Add(new Point3D(-0.176741, -0.500000, 0.176715));
            mesh.Positions.Add(new Point3D(-0.124968, -0.500000, 0.216442));
            mesh.Positions.Add(new Point3D(-0.064676, -0.500000, 0.241416));
            mesh.Positions.Add(new Point3D(0.064726, -0.500000, 0.241416));
            mesh.Positions.Add(new Point3D(0.125018, -0.500000, 0.216442));
            mesh.Positions.Add(new Point3D(0.176791, -0.500000, 0.176715));
            mesh.Positions.Add(new Point3D(0.216518, -0.500000, 0.124941));
            mesh.Positions.Add(new Point3D(0.241492, -0.500000, 0.064650));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.249960, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.249934));
            mesh.Positions.Add(new Point3D(0.250010, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, -0.250036));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.041667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.083333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.125000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.166667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.208333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.541667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.583333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.625000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.666667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.708333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.791667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.833333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.875000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.916667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.958333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.750000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.250000, 0.333333));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 19, faceIndex + 20, faceIndex + 23);
            AddTriangle(mesh.TriangleIndices, faceIndex + 21, faceIndex + 9, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 22, faceIndex + 14, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 24, faceIndex + 4, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 24, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 5, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 6, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 8, faceIndex + 7, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 9, faceIndex + 8, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 10, faceIndex + 21, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 11, faceIndex + 10, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 12, faceIndex + 11, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 13, faceIndex + 12, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 14, faceIndex + 13, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 15, faceIndex + 22, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 16, faceIndex + 15, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 17, faceIndex + 16, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 18, faceIndex + 17, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 19, faceIndex + 18, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 23, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 20);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 20);
            return faceIndex;
        }
        private int AddGeometry_SlopeHalf(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.707107, -0.000000, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.707107, -0.000000, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.707107, -0.000000, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.707107, -0.000000, -0.707107));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        #endregion AddGeometry
    }
}
