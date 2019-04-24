
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

        public const string CameraLookDirectionPropertyName = "CameraLookDirection";
        private string _cameraLookDirection;
        public string CameraLookDirection
        {
            get => _cameraLookDirection;
            set => Set(ref _cameraLookDirection, value);
        }

        public const string CameraPositionPropertyName = "CameraPosition";
        private string _cameraPosition;
        public string CameraPosition
        {
            get => _cameraPosition;
            set => Set(ref _cameraPosition, value);
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

            CameraLookDirection = "0,0,-1";
            CameraPosition = "0,0,3";
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

                        model = CreateBox(pos, c);
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
                        group.Children.Add(CreateBox(lcd.Position, Color.FromRgb(255, 0, 0)));
                        break;
                    case BlockNode def:
                        group.Children.Add(CreateBox(def.Position, Color.FromRgb(255, 0, 0)));
                        break;
                }
            }
            SelectionModel = group;
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

        private GeometryModel3D CreateBox(Point3D pos, Color colour)
        {
            GeometryModel3D model = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();

            EpbColourIndex[] colours = new EpbColourIndex[6] {0, 0, 0, 0, 0, 0};
            int faceIndex = 0;
            faceIndex = AddGeometry_Cube(mesh, colours, faceIndex);

            model.Geometry = mesh;
            model.Transform = new TranslateTransform3D(pos.X, pos.Y, pos.Z);

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
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 1.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 1.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
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
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
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
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
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
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
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
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
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
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
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
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
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
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 4);
            return faceIndex;
        }
        private int AddGeometry_CornerLongC(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
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
            // Top
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottom(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 3);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
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
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
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
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
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
            mesh.Positions.Add(new Point3D(0.241181, -0.500000, -0.465926));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.366025));
            mesh.Positions.Add(new Point3D(-0.207107, -0.500000, -0.207107));
            mesh.Positions.Add(new Point3D(-0.366025, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.465926, -0.500000, 0.241181));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 1.000000, 1.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.041667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.083333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.125000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.166667, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.208333, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.250000, 0.500000));
            mesh.Normals.Add(new Vector3D(-0.476107, 0.739354, -0.476107));
            mesh.Normals.Add(new Vector3D(-0.092692, 0.704063, -0.704063));
            mesh.Normals.Add(new Vector3D(-0.183013, 0.707107, -0.683013));
            mesh.Normals.Add(new Vector3D(-0.353553, 0.707107, -0.612372));
            mesh.Normals.Add(new Vector3D(-0.500000, 0.707107, -0.500000));
            mesh.Normals.Add(new Vector3D(-0.612372, 0.707107, -0.353553));
            mesh.Normals.Add(new Vector3D(-0.683013, 0.707107, -0.183013));
            mesh.Normals.Add(new Vector3D(-0.704063, 0.704063, -0.092692));
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
            mesh.Positions.Add(new Point3D(-0.465926, -0.500000, 0.241181));
            mesh.Positions.Add(new Point3D(-0.366025, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.207107, -0.500000, -0.207107));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.366025));
            mesh.Positions.Add(new Point3D(0.241181, -0.500000, -0.465926));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(-0.464379, -0.241180, -0.499894));
            mesh.Positions.Add(new Point3D(-0.364479, 0.000001, -0.499585));
            mesh.Positions.Add(new Point3D(-0.205562, 0.207107, -0.499092));
            mesh.Positions.Add(new Point3D(0.001544, 0.366026, -0.498451));
            mesh.Positions.Add(new Point3D(0.242723, 0.465926, -0.497704));
            mesh.Positions.Add(new Point3D(0.501541, -0.499999, -0.496902));
            mesh.Positions.Add(new Point3D(0.501541, 0.500000, -0.496902));
            mesh.Positions.Add(new Point3D(-0.498453, -0.499999, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.250000, 0.333333));
            mesh.Normals.Add(new Vector3D(0.003097, 0.000000, -0.999995));
            mesh.Normals.Add(new Vector3D(0.003097, 0.000000, -0.999995));
            mesh.Normals.Add(new Vector3D(0.003097, 0.000000, -0.999995));
            mesh.Normals.Add(new Vector3D(0.003097, 0.000000, -0.999995));
            mesh.Normals.Add(new Vector3D(0.003097, 0.000000, -0.999995));
            mesh.Normals.Add(new Vector3D(0.003097, 0.000000, -0.999995));
            mesh.Normals.Add(new Vector3D(0.003097, 0.000000, -0.999995));
            mesh.Normals.Add(new Vector3D(0.003097, 0.000000, -0.999995));
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
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.498454, -0.241180, -0.465924));
            mesh.Positions.Add(new Point3D(-0.498454, 0.000001, -0.366024));
            mesh.Positions.Add(new Point3D(-0.498454, 0.207107, -0.207106));
            mesh.Positions.Add(new Point3D(-0.498454, 0.366026, 0.000001));
            mesh.Positions.Add(new Point3D(-0.498454, 0.465926, 0.241181));
            mesh.Positions.Add(new Point3D(-0.498454, -0.499999, 0.500000));
            mesh.Positions.Add(new Point3D(-0.498454, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.498454, -0.499999, -0.499999));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.291667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.333333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.375000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.416667, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.458333, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.250000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.500000, 0.333333));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.250000, 0.333333));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 6, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 7, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 5);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(-0.498454, -0.241180, -0.465924));
            mesh.Positions.Add(new Point3D(-0.498454, 0.000001, -0.366024));
            mesh.Positions.Add(new Point3D(-0.498454, 0.207107, -0.207106));
            mesh.Positions.Add(new Point3D(-0.498454, 0.366026, 0.000001));
            mesh.Positions.Add(new Point3D(-0.498454, 0.465926, 0.241181));
            mesh.Positions.Add(new Point3D(-0.498454, -0.499999, -0.499999));
            mesh.Positions.Add(new Point3D(-0.498454, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.240439, 0.465926, 0.241181));
            mesh.Positions.Add(new Point3D(0.498457, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000004, 0.366026, 0.000001));
            mesh.Positions.Add(new Point3D(-0.206462, 0.207107, -0.207106));
            mesh.Positions.Add(new Point3D(-0.364889, 0.000001, -0.366024));
            mesh.Positions.Add(new Point3D(-0.464480, -0.241180, -0.465924));
            mesh.Positions.Add(new Point3D(-0.464375, -0.241180, -0.499894));
            mesh.Positions.Add(new Point3D(-0.364475, 0.000001, -0.499585));
            mesh.Positions.Add(new Point3D(-0.205557, 0.207107, -0.499092));
            mesh.Positions.Add(new Point3D(0.001548, 0.366026, -0.498451));
            mesh.Positions.Add(new Point3D(0.242728, 0.465926, -0.497704));
            mesh.Positions.Add(new Point3D(-0.498449, -0.499999, -0.500000));
            mesh.Positions.Add(new Point3D(0.501545, 0.500000, -0.496902));
            mesh.Positions.Add(new Point3D(0.240439, 0.465926, 0.241181));
            mesh.Positions.Add(new Point3D(0.498457, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000004, 0.366026, 0.000001));
            mesh.Positions.Add(new Point3D(-0.206462, 0.207107, -0.207106));
            mesh.Positions.Add(new Point3D(-0.364889, 0.000001, -0.366024));
            mesh.Positions.Add(new Point3D(-0.464480, -0.241180, -0.465924));
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
            mesh.Normals.Add(new Vector3D(-0.000000, 0.300918, -0.953650));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.537487, -0.843272));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.737428, -0.675426));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.887114, -0.461551));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.976344, -0.216222));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.130526, -0.991445));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.991445, -0.130526));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.953650, -0.300918));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.991445, -0.130526));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.843272, -0.537487));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.675426, -0.737428));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.461551, -0.887114));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.258819, -0.965926));
            mesh.Normals.Add(new Vector3D(-0.965921, 0.258819, -0.002992));
            mesh.Normals.Add(new Vector3D(-0.887109, 0.461551, -0.002748));
            mesh.Normals.Add(new Vector3D(-0.737424, 0.675426, -0.002284));
            mesh.Normals.Add(new Vector3D(-0.537485, 0.843272, -0.001665));
            mesh.Normals.Add(new Vector3D(-0.300917, 0.953650, -0.000932));
            mesh.Normals.Add(new Vector3D(-0.991440, 0.130526, -0.003071));
            mesh.Normals.Add(new Vector3D(-0.130526, 0.991445, -0.000404));
            mesh.Normals.Add(new Vector3D(-0.216221, 0.976344, -0.000670));
            mesh.Normals.Add(new Vector3D(-0.130526, 0.991445, -0.000404));
            mesh.Normals.Add(new Vector3D(-0.461549, 0.887114, -0.001430));
            mesh.Normals.Add(new Vector3D(-0.675423, 0.737428, -0.002092));
            mesh.Normals.Add(new Vector3D(-0.843268, 0.537487, -0.002612));
            mesh.Normals.Add(new Vector3D(-0.953645, 0.300918, -0.002954));
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
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.500479, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.466405, -0.241181, 0.499521));
            mesh.Positions.Add(new Point3D(-0.366505, -0.000000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.207586, 0.207107, 0.499521));
            mesh.Positions.Add(new Point3D(-0.000479, 0.366025, 0.499521));
            mesh.Positions.Add(new Point3D(0.240702, 0.465926, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, 0.500000, 0.499521));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
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
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, -0.500479));
            mesh.Positions.Add(new Point3D(0.499521, -0.241181, -0.466405));
            mesh.Positions.Add(new Point3D(0.499521, -0.000000, -0.366505));
            mesh.Positions.Add(new Point3D(0.499521, 0.207107, -0.207586));
            mesh.Positions.Add(new Point3D(0.499521, 0.366025, -0.000479));
            mesh.Positions.Add(new Point3D(0.499521, 0.465926, 0.240702));
            mesh.Positions.Add(new Point3D(0.499521, 0.500000, 0.499521));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, -0.500479));
            mesh.Positions.Add(new Point3D(0.240702, -0.500000, -0.466405));
            mesh.Positions.Add(new Point3D(-0.000479, -0.500000, -0.366505));
            mesh.Positions.Add(new Point3D(-0.207586, -0.500000, -0.207586));
            mesh.Positions.Add(new Point3D(-0.366505, -0.500000, -0.000479));
            mesh.Positions.Add(new Point3D(-0.466405, -0.500000, 0.240702));
            mesh.Positions.Add(new Point3D(-0.500479, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, -0.241181, -0.466405));
            mesh.Positions.Add(new Point3D(0.249521, -0.241181, -0.433492));
            mesh.Positions.Add(new Point3D(0.016558, -0.241181, -0.336996));
            mesh.Positions.Add(new Point3D(-0.183492, -0.241181, -0.183492));
            mesh.Positions.Add(new Point3D(-0.336996, -0.241181, 0.016558));
            mesh.Positions.Add(new Point3D(-0.433492, -0.241181, 0.249521));
            mesh.Positions.Add(new Point3D(-0.466405, -0.241181, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, -0.000000, -0.366505));
            mesh.Positions.Add(new Point3D(0.275377, -0.000000, -0.336996));
            mesh.Positions.Add(new Point3D(0.066508, -0.000000, -0.250479));
            mesh.Positions.Add(new Point3D(-0.112852, -0.000000, -0.112852));
            mesh.Positions.Add(new Point3D(-0.250479, -0.000000, 0.066508));
            mesh.Positions.Add(new Point3D(-0.336996, -0.000000, 0.275377));
            mesh.Positions.Add(new Point3D(-0.366505, -0.000000, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, 0.207107, -0.207586));
            mesh.Positions.Add(new Point3D(0.316508, 0.207107, -0.183492));
            mesh.Positions.Add(new Point3D(0.145967, 0.207107, -0.112852));
            mesh.Positions.Add(new Point3D(-0.000479, 0.207107, -0.000479));
            mesh.Positions.Add(new Point3D(-0.112852, 0.207107, 0.145967));
            mesh.Positions.Add(new Point3D(-0.183492, 0.207107, 0.316508));
            mesh.Positions.Add(new Point3D(-0.207586, 0.207107, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, 0.366025, -0.000479));
            mesh.Positions.Add(new Point3D(0.370111, 0.366025, 0.016558));
            mesh.Positions.Add(new Point3D(0.249521, 0.366025, 0.066508));
            mesh.Positions.Add(new Point3D(0.145967, 0.366025, 0.145967));
            mesh.Positions.Add(new Point3D(0.066508, 0.366025, 0.249521));
            mesh.Positions.Add(new Point3D(0.016558, 0.366025, 0.370111));
            mesh.Positions.Add(new Point3D(-0.000479, 0.366025, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, 0.465926, 0.240702));
            mesh.Positions.Add(new Point3D(0.432533, 0.465926, 0.249521));
            mesh.Positions.Add(new Point3D(0.370111, 0.465926, 0.275377));
            mesh.Positions.Add(new Point3D(0.316508, 0.465926, 0.316508));
            mesh.Positions.Add(new Point3D(0.275377, 0.465926, 0.370111));
            mesh.Positions.Add(new Point3D(0.249521, 0.465926, 0.432533));
            mesh.Positions.Add(new Point3D(0.240702, 0.465926, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, 0.500000, 0.499521));
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
            mesh.Normals.Add(new Vector3D(-0.129428, 0.129428, -0.983106));
            mesh.Normals.Add(new Vector3D(-0.214376, 0.130403, -0.968007));
            mesh.Normals.Add(new Vector3D(-0.457610, 0.130403, -0.879539));
            mesh.Normals.Add(new Vector3D(-0.669659, 0.130403, -0.731131));
            mesh.Normals.Add(new Vector3D(-0.836071, 0.130403, -0.532898));
            mesh.Normals.Add(new Vector3D(-0.945507, 0.130403, -0.298349));
            mesh.Normals.Add(new Vector3D(-0.983106, 0.129428, -0.129428));
            mesh.Normals.Add(new Vector3D(-0.127487, 0.214520, -0.968364));
            mesh.Normals.Add(new Vector3D(-0.251417, 0.258889, -0.932613));
            mesh.Normals.Add(new Vector3D(-0.484228, 0.258889, -0.835763));
            mesh.Normals.Add(new Vector3D(-0.684039, 0.258889, -0.681958));
            mesh.Normals.Add(new Vector3D(-0.837235, 0.258889, -0.481678));
            mesh.Normals.Add(new Vector3D(-0.933374, 0.258889, -0.248573));
            mesh.Normals.Add(new Vector3D(-0.946203, 0.298635, -0.124570));
            mesh.Normals.Add(new Vector3D(-0.115996, 0.458534, -0.881074));
            mesh.Normals.Add(new Vector3D(-0.226882, 0.500107, -0.835714));
            mesh.Normals.Add(new Vector3D(-0.435450, 0.500107, -0.748516));
            mesh.Normals.Add(new Vector3D(-0.614342, 0.500107, -0.610308));
            mesh.Normals.Add(new Vector3D(-0.751368, 0.500107, -0.430509));
            mesh.Normals.Add(new Vector3D(-0.837190, 0.500107, -0.221372));
            mesh.Normals.Add(new Vector3D(-0.838063, 0.534300, -0.110333));
            mesh.Normals.Add(new Vector3D(-0.096618, 0.672361, -0.733890));
            mesh.Normals.Add(new Vector3D(-0.186897, 0.707204, -0.681859));
            mesh.Normals.Add(new Vector3D(-0.357007, 0.707204, -0.610252));
            mesh.Normals.Add(new Vector3D(-0.502787, 0.707204, -0.497058));
            mesh.Normals.Add(new Vector3D(-0.614303, 0.707204, -0.349991));
            mesh.Normals.Add(new Vector3D(-0.683956, 0.707204, -0.179072));
            mesh.Normals.Add(new Vector3D(-0.672677, 0.734618, -0.088560));
            mesh.Normals.Add(new Vector3D(-0.070573, 0.841230, -0.536052));
            mesh.Normals.Add(new Vector3D(-0.134192, 0.866078, -0.481561));
            mesh.Normals.Add(new Vector3D(-0.254256, 0.866078, -0.430421));
            mesh.Normals.Add(new Vector3D(-0.356994, 0.866078, -0.349948));
            mesh.Normals.Add(new Vector3D(-0.435403, 0.866078, -0.245627));
            mesh.Normals.Add(new Vector3D(-0.484140, 0.866078, -0.124567));
            mesh.Normals.Add(new Vector3D(-0.460602, 0.885533, -0.060639));
            mesh.Normals.Add(new Vector3D(-0.039577, 0.952922, -0.300620));
            mesh.Normals.Add(new Vector3D(-0.083332, 0.958749, -0.271764));
            mesh.Normals.Add(new Vector3D(-0.150830, 0.958749, -0.240936));
            mesh.Normals.Add(new Vector3D(-0.208049, 0.958749, -0.193688));
            mesh.Normals.Add(new Vector3D(-0.251090, 0.958749, -0.133242));
            mesh.Normals.Add(new Vector3D(-0.277020, 0.958749, -0.063714));
            mesh.Normals.Add(new Vector3D(-0.258598, 0.965385, -0.034045));
            mesh.Normals.Add(new Vector3D(-0.084175, 0.992889, -0.084175));
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
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.500479, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.466405, -0.500000, 0.240702));
            mesh.Positions.Add(new Point3D(-0.366505, -0.500000, -0.000479));
            mesh.Positions.Add(new Point3D(-0.207586, -0.500000, -0.207586));
            mesh.Positions.Add(new Point3D(-0.000479, -0.500000, -0.366505));
            mesh.Positions.Add(new Point3D(0.240702, -0.500000, -0.466405));
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, -0.500479));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
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
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.500479, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.466405, -0.241181, 0.499521));
            mesh.Positions.Add(new Point3D(-0.366505, -0.000000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.207586, 0.207107, 0.499521));
            mesh.Positions.Add(new Point3D(-0.000479, 0.366025, 0.499521));
            mesh.Positions.Add(new Point3D(0.240702, 0.465926, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, 0.500000, 0.499521));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
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
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, -0.500479));
            mesh.Positions.Add(new Point3D(0.499521, -0.241181, -0.466405));
            mesh.Positions.Add(new Point3D(0.499521, -0.000000, -0.366505));
            mesh.Positions.Add(new Point3D(0.499521, 0.207107, -0.207586));
            mesh.Positions.Add(new Point3D(0.499521, 0.366025, -0.000479));
            mesh.Positions.Add(new Point3D(0.499521, 0.465926, 0.240702));
            mesh.Positions.Add(new Point3D(0.499521, 0.500000, 0.499521));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(-0.466405, -0.241181, 0.499521));
            mesh.Positions.Add(new Point3D(-0.366505, -0.000000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.207586, 0.207107, 0.499521));
            mesh.Positions.Add(new Point3D(-0.000479, 0.366025, 0.499521));
            mesh.Positions.Add(new Point3D(0.240702, 0.465926, 0.499521));
            mesh.Positions.Add(new Point3D(-0.500479, -0.499984, -0.500504));
            mesh.Positions.Add(new Point3D(-0.500479, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.466405, -0.241181, -0.465631));
            mesh.Positions.Add(new Point3D(-0.366505, 0.000000, -0.365730));
            mesh.Positions.Add(new Point3D(-0.207586, 0.207107, -0.206812));
            mesh.Positions.Add(new Point3D(-0.000479, 0.366025, 0.000295));
            mesh.Positions.Add(new Point3D(0.240702, 0.465926, 0.241476));
            mesh.Positions.Add(new Point3D(0.498746, 0.499898, 0.499521));
            mesh.Positions.Add(new Point3D(0.499456, 0.466257, 0.243283));
            mesh.Positions.Add(new Point3D(0.499456, 0.366357, 0.002102));
            mesh.Positions.Add(new Point3D(0.499456, 0.207438, -0.205005));
            mesh.Positions.Add(new Point3D(0.499456, 0.000331, -0.363923));
            mesh.Positions.Add(new Point3D(0.499456, -0.240850, -0.463824));
            mesh.Positions.Add(new Point3D(0.499456, -0.499890, -0.500651));
            mesh.Positions.Add(new Point3D(-0.500479, -0.499999, -0.500651));
            mesh.Positions.Add(new Point3D(0.240702, 0.466257, 0.243283));
            mesh.Positions.Add(new Point3D(0.499456, 0.499992, 0.499521));
            mesh.Positions.Add(new Point3D(-0.000479, 0.366357, 0.002102));
            mesh.Positions.Add(new Point3D(-0.207586, 0.207438, -0.205005));
            mesh.Positions.Add(new Point3D(-0.366505, 0.000331, -0.363923));
            mesh.Positions.Add(new Point3D(-0.466405, -0.240850, -0.463824));
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
            mesh.Normals.Add(new Vector3D(-0.953650, 0.300918, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.843272, 0.537487, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.675426, 0.737428, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.461551, 0.887114, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.258819, 0.965926, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.991444, 0.130534, 0.000002));
            mesh.Normals.Add(new Vector3D(-0.991444, 0.130530, 0.000001));
            mesh.Normals.Add(new Vector3D(-0.976344, 0.216225, 0.000001));
            mesh.Normals.Add(new Vector3D(-0.887114, 0.461551, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.737428, 0.675426, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.537487, 0.843272, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.300918, 0.953650, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.130526, 0.991445, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.953650, -0.300918));
            mesh.Normals.Add(new Vector3D(0.000000, 0.843272, -0.537487));
            mesh.Normals.Add(new Vector3D(0.000000, 0.675426, -0.737428));
            mesh.Normals.Add(new Vector3D(0.000000, 0.461551, -0.887114));
            mesh.Normals.Add(new Vector3D(-0.000005, 0.222942, -0.974832));
            mesh.Normals.Add(new Vector3D(-0.000015, 0.140754, -0.990045));
            mesh.Normals.Add(new Vector3D(-0.000008, 0.140725, -0.990049));
            mesh.Normals.Add(new Vector3D(0.000000, 0.965926, -0.258819));
            mesh.Normals.Add(new Vector3D(0.000000, 0.991445, -0.130526));
            mesh.Normals.Add(new Vector3D(0.000000, 0.887114, -0.461551));
            mesh.Normals.Add(new Vector3D(0.000000, 0.737428, -0.675426));
            mesh.Normals.Add(new Vector3D(0.000000, 0.537487, -0.843272));
            mesh.Normals.Add(new Vector3D(0.000000, 0.304155, -0.952622));
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
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, 0.499521));
            mesh.Positions.Add(new Point3D(-0.500479, -0.500000, -0.500479));
            mesh.Positions.Add(new Point3D(0.499521, -0.500000, -0.500479));
            mesh.Positions.Add(new Point3D(-0.500479, -0.500000, 0.499521));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 3);
            faceIndex += 6;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, 0.465926, 0.241181));
            mesh.Positions.Add(new Point3D(0.500000, 0.366025, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.207107, -0.207107));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.366025));
            mesh.Positions.Add(new Point3D(0.500000, -0.241181, -0.465926));
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
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 4, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 6, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 2, faceIndex + 5);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 3, faceIndex + 5);
            faceIndex += 8;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.465926, 0.241181));
            mesh.Positions.Add(new Point3D(-0.500000, 0.366025, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.207107, -0.207107));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.366025));
            mesh.Positions.Add(new Point3D(-0.500000, -0.241181, -0.465926));
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
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 1, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 2, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 4, faceIndex + 5, faceIndex + 0);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 0);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, 0.465926, 0.241181));
            mesh.Positions.Add(new Point3D(0.500000, 0.366025, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.207107, -0.207107));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.366025));
            mesh.Positions.Add(new Point3D(0.500000, -0.241181, -0.465926));
            mesh.Positions.Add(new Point3D(-0.500000, 0.465926, 0.241181));
            mesh.Positions.Add(new Point3D(-0.500000, 0.366025, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.207107, -0.207107));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.366025));
            mesh.Positions.Add(new Point3D(-0.500000, -0.241181, -0.465926));
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
            mesh.Normals.Add(new Vector3D(-0.000000, 0.976344, -0.216222));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.887114, -0.461551));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.737428, -0.675426));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.537487, -0.843272));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.300918, -0.953650));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.953650, -0.300918));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.843272, -0.537487));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.675426, -0.737428));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.461551, -0.887114));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.216222, -0.976344));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.130526, -0.991445));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.130526, -0.991445));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.991445, -0.130526));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.991445, -0.130526));
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
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(0.482963, 0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.433013, 0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.353553, 0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.250000, 0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.129410, 0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.129410, 0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, 0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, 0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, 0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, 0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.482963, 0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, 0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, 0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, 0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, 0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.129410, 0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.250000, 0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.353553, 0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.433013, 0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.482963, 0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(0.976344, 0.000000, -0.216222));
            mesh.Normals.Add(new Vector3D(0.887114, 0.000000, -0.461551));
            mesh.Normals.Add(new Vector3D(0.737428, 0.000000, -0.675426));
            mesh.Normals.Add(new Vector3D(0.537487, 0.000000, -0.843272));
            mesh.Normals.Add(new Vector3D(0.300918, 0.000000, -0.953650));
            mesh.Normals.Add(new Vector3D(-0.216222, 0.000000, -0.976344));
            mesh.Normals.Add(new Vector3D(-0.461551, 0.000000, -0.887114));
            mesh.Normals.Add(new Vector3D(-0.675426, 0.000000, -0.737428));
            mesh.Normals.Add(new Vector3D(-0.843272, 0.000000, -0.537487));
            mesh.Normals.Add(new Vector3D(-0.953650, 0.000000, -0.300918));
            mesh.Normals.Add(new Vector3D(-0.976344, -0.000000, 0.216222));
            mesh.Normals.Add(new Vector3D(-0.887114, -0.000000, 0.461551));
            mesh.Normals.Add(new Vector3D(-0.737428, -0.000000, 0.675426));
            mesh.Normals.Add(new Vector3D(-0.537487, -0.000000, 0.843272));
            mesh.Normals.Add(new Vector3D(-0.300918, -0.000000, 0.953650));
            mesh.Normals.Add(new Vector3D(0.216222, -0.000000, 0.976344));
            mesh.Normals.Add(new Vector3D(0.461551, -0.000000, 0.887114));
            mesh.Normals.Add(new Vector3D(0.675426, -0.000000, 0.737428));
            mesh.Normals.Add(new Vector3D(0.843272, -0.000000, 0.537487));
            mesh.Normals.Add(new Vector3D(0.953650, -0.000000, 0.300918));
            mesh.Normals.Add(new Vector3D(0.953650, 0.000000, -0.300918));
            mesh.Normals.Add(new Vector3D(0.843272, 0.000000, -0.537487));
            mesh.Normals.Add(new Vector3D(0.675426, 0.000000, -0.737428));
            mesh.Normals.Add(new Vector3D(0.461551, 0.000000, -0.887114));
            mesh.Normals.Add(new Vector3D(0.216222, 0.000000, -0.976344));
            mesh.Normals.Add(new Vector3D(-0.300918, 0.000000, -0.953650));
            mesh.Normals.Add(new Vector3D(-0.537487, 0.000000, -0.843272));
            mesh.Normals.Add(new Vector3D(-0.737428, 0.000000, -0.675426));
            mesh.Normals.Add(new Vector3D(-0.887114, 0.000000, -0.461551));
            mesh.Normals.Add(new Vector3D(-0.976344, 0.000000, -0.216222));
            mesh.Normals.Add(new Vector3D(-0.953650, -0.000000, 0.300918));
            mesh.Normals.Add(new Vector3D(-0.843272, -0.000000, 0.537487));
            mesh.Normals.Add(new Vector3D(-0.675426, -0.000000, 0.737428));
            mesh.Normals.Add(new Vector3D(-0.461551, -0.000000, 0.887114));
            mesh.Normals.Add(new Vector3D(-0.216222, -0.000000, 0.976344));
            mesh.Normals.Add(new Vector3D(0.300918, -0.000000, 0.953650));
            mesh.Normals.Add(new Vector3D(0.537487, -0.000000, 0.843272));
            mesh.Normals.Add(new Vector3D(0.737428, -0.000000, 0.675426));
            mesh.Normals.Add(new Vector3D(0.887114, -0.000000, 0.461551));
            mesh.Normals.Add(new Vector3D(0.976344, -0.000000, 0.216222));
            mesh.Normals.Add(new Vector3D(-0.043842, -0.000000, 0.999038));
            mesh.Normals.Add(new Vector3D(0.043842, -0.000000, 0.999038));
            mesh.Normals.Add(new Vector3D(-0.043842, 0.000000, -0.999038));
            mesh.Normals.Add(new Vector3D(0.043842, 0.000000, -0.999038));
            mesh.Normals.Add(new Vector3D(-0.999038, -0.000000, 0.043842));
            mesh.Normals.Add(new Vector3D(-0.999038, 0.000000, -0.043842));
            mesh.Normals.Add(new Vector3D(0.999038, 0.000000, -0.043842));
            mesh.Normals.Add(new Vector3D(0.999038, -0.000000, 0.043842));
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
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.482963, 0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.433013, 0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.353553, 0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.250000, 0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.129410, 0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.129410, 0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, 0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, 0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, 0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, 0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.482963, 0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, 0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, 0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, 0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, 0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.129410, 0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.250000, 0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.353553, 0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.433013, 0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.482963, 0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.500000));
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
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
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
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
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
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(0.482963, -0.370590, -0.000000));
            mesh.Positions.Add(new Point3D(0.466506, -0.370590, -0.125000));
            mesh.Positions.Add(new Point3D(0.418258, -0.370590, -0.241481));
            mesh.Positions.Add(new Point3D(0.341506, -0.370590, -0.341506));
            mesh.Positions.Add(new Point3D(0.241481, -0.370590, -0.418258));
            mesh.Positions.Add(new Point3D(0.125000, -0.370590, -0.466506));
            mesh.Positions.Add(new Point3D(-0.000000, -0.370590, -0.482963));
            mesh.Positions.Add(new Point3D(-0.125000, -0.370590, -0.466506));
            mesh.Positions.Add(new Point3D(-0.241481, -0.370590, -0.418258));
            mesh.Positions.Add(new Point3D(-0.341506, -0.370590, -0.341506));
            mesh.Positions.Add(new Point3D(-0.418258, -0.370590, -0.241481));
            mesh.Positions.Add(new Point3D(-0.466506, -0.370590, -0.125000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.370590, 0.000000));
            mesh.Positions.Add(new Point3D(-0.466506, -0.370590, 0.125000));
            mesh.Positions.Add(new Point3D(-0.418258, -0.370590, 0.241481));
            mesh.Positions.Add(new Point3D(-0.341506, -0.370590, 0.341506));
            mesh.Positions.Add(new Point3D(-0.241481, -0.370590, 0.418258));
            mesh.Positions.Add(new Point3D(-0.125000, -0.370590, 0.466506));
            mesh.Positions.Add(new Point3D(0.000000, -0.370590, 0.482963));
            mesh.Positions.Add(new Point3D(0.125000, -0.370590, 0.466506));
            mesh.Positions.Add(new Point3D(0.241481, -0.370590, 0.418258));
            mesh.Positions.Add(new Point3D(0.341506, -0.370590, 0.341506));
            mesh.Positions.Add(new Point3D(0.418258, -0.370590, 0.241481));
            mesh.Positions.Add(new Point3D(0.466506, -0.370590, 0.125000));
            mesh.Positions.Add(new Point3D(0.433013, -0.250000, -0.000000));
            mesh.Positions.Add(new Point3D(0.418258, -0.250000, -0.112072));
            mesh.Positions.Add(new Point3D(0.375000, -0.250000, -0.216506));
            mesh.Positions.Add(new Point3D(0.306186, -0.250000, -0.306186));
            mesh.Positions.Add(new Point3D(0.216506, -0.250000, -0.375000));
            mesh.Positions.Add(new Point3D(0.112072, -0.250000, -0.418258));
            mesh.Positions.Add(new Point3D(-0.000000, -0.250000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.112072, -0.250000, -0.418258));
            mesh.Positions.Add(new Point3D(-0.216506, -0.250000, -0.375000));
            mesh.Positions.Add(new Point3D(-0.306186, -0.250000, -0.306186));
            mesh.Positions.Add(new Point3D(-0.375000, -0.250000, -0.216506));
            mesh.Positions.Add(new Point3D(-0.418258, -0.250000, -0.112072));
            mesh.Positions.Add(new Point3D(-0.433013, -0.250000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.418258, -0.250000, 0.112072));
            mesh.Positions.Add(new Point3D(-0.375000, -0.250000, 0.216506));
            mesh.Positions.Add(new Point3D(-0.306186, -0.250000, 0.306186));
            mesh.Positions.Add(new Point3D(-0.216506, -0.250000, 0.375000));
            mesh.Positions.Add(new Point3D(-0.112072, -0.250000, 0.418258));
            mesh.Positions.Add(new Point3D(0.000000, -0.250000, 0.433013));
            mesh.Positions.Add(new Point3D(0.112072, -0.250000, 0.418258));
            mesh.Positions.Add(new Point3D(0.216506, -0.250000, 0.375000));
            mesh.Positions.Add(new Point3D(0.306186, -0.250000, 0.306186));
            mesh.Positions.Add(new Point3D(0.375000, -0.250000, 0.216506));
            mesh.Positions.Add(new Point3D(0.418258, -0.250000, 0.112072));
            mesh.Positions.Add(new Point3D(0.353553, -0.146447, -0.000000));
            mesh.Positions.Add(new Point3D(0.341506, -0.146447, -0.091506));
            mesh.Positions.Add(new Point3D(0.306186, -0.146447, -0.176777));
            mesh.Positions.Add(new Point3D(0.250000, -0.146447, -0.250000));
            mesh.Positions.Add(new Point3D(0.176777, -0.146447, -0.306186));
            mesh.Positions.Add(new Point3D(0.091506, -0.146447, -0.341506));
            mesh.Positions.Add(new Point3D(-0.000000, -0.146447, -0.353553));
            mesh.Positions.Add(new Point3D(-0.091506, -0.146447, -0.341506));
            mesh.Positions.Add(new Point3D(-0.176777, -0.146447, -0.306186));
            mesh.Positions.Add(new Point3D(-0.250000, -0.146447, -0.250000));
            mesh.Positions.Add(new Point3D(-0.306186, -0.146447, -0.176777));
            mesh.Positions.Add(new Point3D(-0.341506, -0.146447, -0.091506));
            mesh.Positions.Add(new Point3D(-0.353553, -0.146447, 0.000000));
            mesh.Positions.Add(new Point3D(-0.341506, -0.146447, 0.091506));
            mesh.Positions.Add(new Point3D(-0.306186, -0.146447, 0.176777));
            mesh.Positions.Add(new Point3D(-0.250000, -0.146447, 0.250000));
            mesh.Positions.Add(new Point3D(-0.176777, -0.146447, 0.306186));
            mesh.Positions.Add(new Point3D(-0.091506, -0.146447, 0.341506));
            mesh.Positions.Add(new Point3D(0.000000, -0.146447, 0.353553));
            mesh.Positions.Add(new Point3D(0.091506, -0.146447, 0.341506));
            mesh.Positions.Add(new Point3D(0.176777, -0.146447, 0.306186));
            mesh.Positions.Add(new Point3D(0.250000, -0.146447, 0.250000));
            mesh.Positions.Add(new Point3D(0.306186, -0.146447, 0.176777));
            mesh.Positions.Add(new Point3D(0.341506, -0.146447, 0.091506));
            mesh.Positions.Add(new Point3D(0.250000, -0.066987, -0.000000));
            mesh.Positions.Add(new Point3D(0.241481, -0.066987, -0.064705));
            mesh.Positions.Add(new Point3D(0.216506, -0.066987, -0.125000));
            mesh.Positions.Add(new Point3D(0.176777, -0.066987, -0.176777));
            mesh.Positions.Add(new Point3D(0.125000, -0.066987, -0.216506));
            mesh.Positions.Add(new Point3D(0.064705, -0.066987, -0.241481));
            mesh.Positions.Add(new Point3D(-0.000000, -0.066987, -0.250000));
            mesh.Positions.Add(new Point3D(-0.064705, -0.066987, -0.241481));
            mesh.Positions.Add(new Point3D(-0.125000, -0.066987, -0.216506));
            mesh.Positions.Add(new Point3D(-0.176777, -0.066987, -0.176777));
            mesh.Positions.Add(new Point3D(-0.216506, -0.066987, -0.125000));
            mesh.Positions.Add(new Point3D(-0.241481, -0.066987, -0.064705));
            mesh.Positions.Add(new Point3D(-0.250000, -0.066987, 0.000000));
            mesh.Positions.Add(new Point3D(-0.241481, -0.066987, 0.064705));
            mesh.Positions.Add(new Point3D(-0.216506, -0.066987, 0.125000));
            mesh.Positions.Add(new Point3D(-0.176777, -0.066987, 0.176777));
            mesh.Positions.Add(new Point3D(-0.125000, -0.066987, 0.216506));
            mesh.Positions.Add(new Point3D(-0.064705, -0.066987, 0.241481));
            mesh.Positions.Add(new Point3D(0.000000, -0.066987, 0.250000));
            mesh.Positions.Add(new Point3D(0.064705, -0.066987, 0.241481));
            mesh.Positions.Add(new Point3D(0.125000, -0.066987, 0.216506));
            mesh.Positions.Add(new Point3D(0.176777, -0.066987, 0.176777));
            mesh.Positions.Add(new Point3D(0.216506, -0.066987, 0.125000));
            mesh.Positions.Add(new Point3D(0.241481, -0.066987, 0.064705));
            mesh.Positions.Add(new Point3D(0.129410, -0.017037, -0.000000));
            mesh.Positions.Add(new Point3D(0.125000, -0.017037, -0.033494));
            mesh.Positions.Add(new Point3D(0.112072, -0.017037, -0.064705));
            mesh.Positions.Add(new Point3D(0.091506, -0.017037, -0.091506));
            mesh.Positions.Add(new Point3D(0.064705, -0.017037, -0.112072));
            mesh.Positions.Add(new Point3D(0.033494, -0.017037, -0.125000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.017037, -0.129410));
            mesh.Positions.Add(new Point3D(-0.033494, -0.017037, -0.125000));
            mesh.Positions.Add(new Point3D(-0.064705, -0.017037, -0.112072));
            mesh.Positions.Add(new Point3D(-0.091506, -0.017037, -0.091506));
            mesh.Positions.Add(new Point3D(-0.112072, -0.017037, -0.064705));
            mesh.Positions.Add(new Point3D(-0.125000, -0.017037, -0.033494));
            mesh.Positions.Add(new Point3D(-0.129410, -0.017037, 0.000000));
            mesh.Positions.Add(new Point3D(-0.125000, -0.017037, 0.033494));
            mesh.Positions.Add(new Point3D(-0.112072, -0.017037, 0.064705));
            mesh.Positions.Add(new Point3D(-0.091506, -0.017037, 0.091506));
            mesh.Positions.Add(new Point3D(-0.064705, -0.017037, 0.112072));
            mesh.Positions.Add(new Point3D(-0.033494, -0.017037, 0.125000));
            mesh.Positions.Add(new Point3D(0.000000, -0.017037, 0.129410));
            mesh.Positions.Add(new Point3D(0.033494, -0.017037, 0.125000));
            mesh.Positions.Add(new Point3D(0.064705, -0.017037, 0.112072));
            mesh.Positions.Add(new Point3D(0.091506, -0.017037, 0.091506));
            mesh.Positions.Add(new Point3D(0.112072, -0.017037, 0.064705));
            mesh.Positions.Add(new Point3D(0.125000, -0.017037, 0.033494));
            mesh.Positions.Add(new Point3D(-0.000000, -0.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(0.990508, 0.130403, 0.043468));
            mesh.Normals.Add(new Vector3D(0.968007, 0.130403, -0.214376));
            mesh.Normals.Add(new Vector3D(0.879539, 0.130403, -0.457610));
            mesh.Normals.Add(new Vector3D(0.731131, 0.130403, -0.669659));
            mesh.Normals.Add(new Vector3D(0.532898, 0.130403, -0.836071));
            mesh.Normals.Add(new Vector3D(0.298349, 0.130403, -0.945507));
            mesh.Normals.Add(new Vector3D(0.043468, 0.130403, -0.990508));
            mesh.Normals.Add(new Vector3D(-0.214376, 0.130403, -0.968007));
            mesh.Normals.Add(new Vector3D(-0.457610, 0.130403, -0.879539));
            mesh.Normals.Add(new Vector3D(-0.669659, 0.130403, -0.731131));
            mesh.Normals.Add(new Vector3D(-0.836071, 0.130403, -0.532898));
            mesh.Normals.Add(new Vector3D(-0.945507, 0.130403, -0.298349));
            mesh.Normals.Add(new Vector3D(-0.990508, 0.130403, -0.043468));
            mesh.Normals.Add(new Vector3D(-0.968007, 0.130403, 0.214376));
            mesh.Normals.Add(new Vector3D(-0.879539, 0.130403, 0.457610));
            mesh.Normals.Add(new Vector3D(-0.731131, 0.130403, 0.669659));
            mesh.Normals.Add(new Vector3D(-0.532898, 0.130403, 0.836071));
            mesh.Normals.Add(new Vector3D(-0.298349, 0.130403, 0.945507));
            mesh.Normals.Add(new Vector3D(-0.043468, 0.130403, 0.990508));
            mesh.Normals.Add(new Vector3D(0.214376, 0.130403, 0.968007));
            mesh.Normals.Add(new Vector3D(0.457610, 0.130403, 0.879539));
            mesh.Normals.Add(new Vector3D(0.669659, 0.130403, 0.731131));
            mesh.Normals.Add(new Vector3D(0.836071, 0.130403, 0.532898));
            mesh.Normals.Add(new Vector3D(0.945507, 0.130403, 0.298349));
            mesh.Normals.Add(new Vector3D(0.965906, 0.258889, -0.001472));
            mesh.Normals.Add(new Vector3D(0.932613, 0.258889, -0.251417));
            mesh.Normals.Add(new Vector3D(0.835763, 0.258889, -0.484228));
            mesh.Normals.Add(new Vector3D(0.681958, 0.258889, -0.684039));
            mesh.Normals.Add(new Vector3D(0.481678, 0.258889, -0.837235));
            mesh.Normals.Add(new Vector3D(0.248573, 0.258889, -0.933374));
            mesh.Normals.Add(new Vector3D(-0.001472, 0.258889, -0.965906));
            mesh.Normals.Add(new Vector3D(-0.251417, 0.258889, -0.932613));
            mesh.Normals.Add(new Vector3D(-0.484228, 0.258889, -0.835763));
            mesh.Normals.Add(new Vector3D(-0.684039, 0.258889, -0.681958));
            mesh.Normals.Add(new Vector3D(-0.837235, 0.258889, -0.481678));
            mesh.Normals.Add(new Vector3D(-0.933374, 0.258889, -0.248573));
            mesh.Normals.Add(new Vector3D(-0.965906, 0.258889, 0.001472));
            mesh.Normals.Add(new Vector3D(-0.932613, 0.258889, 0.251417));
            mesh.Normals.Add(new Vector3D(-0.835763, 0.258889, 0.484228));
            mesh.Normals.Add(new Vector3D(-0.681958, 0.258889, 0.684039));
            mesh.Normals.Add(new Vector3D(-0.481678, 0.258889, 0.837235));
            mesh.Normals.Add(new Vector3D(-0.248573, 0.258889, 0.933374));
            mesh.Normals.Add(new Vector3D(0.001472, 0.258889, 0.965906));
            mesh.Normals.Add(new Vector3D(0.251417, 0.258889, 0.932613));
            mesh.Normals.Add(new Vector3D(0.484228, 0.258889, 0.835763));
            mesh.Normals.Add(new Vector3D(0.684039, 0.258889, 0.681958));
            mesh.Normals.Add(new Vector3D(0.837235, 0.258889, 0.481678));
            mesh.Normals.Add(new Vector3D(0.933374, 0.258889, 0.248573));
            mesh.Normals.Add(new Vector3D(0.865959, 0.500107, -0.002852));
            mesh.Normals.Add(new Vector3D(0.835714, 0.500107, -0.226882));
            mesh.Normals.Add(new Vector3D(0.748516, 0.500107, -0.435450));
            mesh.Normals.Add(new Vector3D(0.610308, 0.500107, -0.614342));
            mesh.Normals.Add(new Vector3D(0.430509, 0.500107, -0.751368));
            mesh.Normals.Add(new Vector3D(0.221372, 0.500107, -0.837190));
            mesh.Normals.Add(new Vector3D(-0.002852, 0.500107, -0.865959));
            mesh.Normals.Add(new Vector3D(-0.226882, 0.500107, -0.835714));
            mesh.Normals.Add(new Vector3D(-0.435450, 0.500107, -0.748516));
            mesh.Normals.Add(new Vector3D(-0.614342, 0.500107, -0.610308));
            mesh.Normals.Add(new Vector3D(-0.751368, 0.500107, -0.430509));
            mesh.Normals.Add(new Vector3D(-0.837190, 0.500107, -0.221372));
            mesh.Normals.Add(new Vector3D(-0.865959, 0.500107, 0.002852));
            mesh.Normals.Add(new Vector3D(-0.835714, 0.500107, 0.226882));
            mesh.Normals.Add(new Vector3D(-0.748516, 0.500107, 0.435450));
            mesh.Normals.Add(new Vector3D(-0.610308, 0.500107, 0.614342));
            mesh.Normals.Add(new Vector3D(-0.430509, 0.500107, 0.751368));
            mesh.Normals.Add(new Vector3D(-0.221372, 0.500107, 0.837190));
            mesh.Normals.Add(new Vector3D(0.002852, 0.500107, 0.865959));
            mesh.Normals.Add(new Vector3D(0.226882, 0.500107, 0.835714));
            mesh.Normals.Add(new Vector3D(0.435450, 0.500107, 0.748516));
            mesh.Normals.Add(new Vector3D(0.614342, 0.500107, 0.610308));
            mesh.Normals.Add(new Vector3D(0.751368, 0.500107, 0.430509));
            mesh.Normals.Add(new Vector3D(0.837190, 0.500107, 0.221372));
            mesh.Normals.Add(new Vector3D(0.706997, 0.707204, -0.004051));
            mesh.Normals.Add(new Vector3D(0.681859, 0.707204, -0.186897));
            mesh.Normals.Add(new Vector3D(0.610252, 0.707204, -0.357007));
            mesh.Normals.Add(new Vector3D(0.497058, 0.707204, -0.502787));
            mesh.Normals.Add(new Vector3D(0.349991, 0.707204, -0.614303));
            mesh.Normals.Add(new Vector3D(0.179072, 0.707204, -0.683956));
            mesh.Normals.Add(new Vector3D(-0.004051, 0.707204, -0.706997));
            mesh.Normals.Add(new Vector3D(-0.186897, 0.707204, -0.681859));
            mesh.Normals.Add(new Vector3D(-0.357007, 0.707204, -0.610252));
            mesh.Normals.Add(new Vector3D(-0.502787, 0.707204, -0.497058));
            mesh.Normals.Add(new Vector3D(-0.614303, 0.707204, -0.349991));
            mesh.Normals.Add(new Vector3D(-0.683956, 0.707204, -0.179072));
            mesh.Normals.Add(new Vector3D(-0.706997, 0.707204, 0.004051));
            mesh.Normals.Add(new Vector3D(-0.681859, 0.707204, 0.186897));
            mesh.Normals.Add(new Vector3D(-0.610252, 0.707204, 0.357007));
            mesh.Normals.Add(new Vector3D(-0.497058, 0.707204, 0.502787));
            mesh.Normals.Add(new Vector3D(-0.349991, 0.707204, 0.614303));
            mesh.Normals.Add(new Vector3D(-0.179072, 0.707204, 0.683956));
            mesh.Normals.Add(new Vector3D(0.004051, 0.707204, 0.706997));
            mesh.Normals.Add(new Vector3D(0.186897, 0.707204, 0.681859));
            mesh.Normals.Add(new Vector3D(0.357007, 0.707204, 0.610252));
            mesh.Normals.Add(new Vector3D(0.502787, 0.707204, 0.497058));
            mesh.Normals.Add(new Vector3D(0.614303, 0.707204, 0.349991));
            mesh.Normals.Add(new Vector3D(0.683956, 0.707204, 0.179072));
            mesh.Normals.Add(new Vector3D(0.499884, 0.866078, -0.004982));
            mesh.Normals.Add(new Vector3D(0.481561, 0.866078, -0.134192));
            mesh.Normals.Add(new Vector3D(0.430421, 0.866078, -0.254256));
            mesh.Normals.Add(new Vector3D(0.349948, 0.866078, -0.356994));
            mesh.Normals.Add(new Vector3D(0.245627, 0.866078, -0.435403));
            mesh.Normals.Add(new Vector3D(0.124567, 0.866078, -0.484140));
            mesh.Normals.Add(new Vector3D(-0.004982, 0.866078, -0.499884));
            mesh.Normals.Add(new Vector3D(-0.134192, 0.866078, -0.481561));
            mesh.Normals.Add(new Vector3D(-0.254256, 0.866078, -0.430421));
            mesh.Normals.Add(new Vector3D(-0.356994, 0.866078, -0.349948));
            mesh.Normals.Add(new Vector3D(-0.435403, 0.866078, -0.245627));
            mesh.Normals.Add(new Vector3D(-0.484140, 0.866078, -0.124567));
            mesh.Normals.Add(new Vector3D(-0.499884, 0.866078, 0.004982));
            mesh.Normals.Add(new Vector3D(-0.481561, 0.866078, 0.134192));
            mesh.Normals.Add(new Vector3D(-0.430421, 0.866078, 0.254256));
            mesh.Normals.Add(new Vector3D(-0.349948, 0.866078, 0.356994));
            mesh.Normals.Add(new Vector3D(-0.245627, 0.866078, 0.435403));
            mesh.Normals.Add(new Vector3D(-0.124567, 0.866078, 0.484140));
            mesh.Normals.Add(new Vector3D(0.004982, 0.866078, 0.499884));
            mesh.Normals.Add(new Vector3D(0.134192, 0.866078, 0.481561));
            mesh.Normals.Add(new Vector3D(0.254256, 0.866078, 0.430421));
            mesh.Normals.Add(new Vector3D(0.356994, 0.866078, 0.349948));
            mesh.Normals.Add(new Vector3D(0.435403, 0.866078, 0.245627));
            mesh.Normals.Add(new Vector3D(0.484140, 0.866078, 0.124567));
            mesh.Normals.Add(new Vector3D(0.284071, 0.958749, -0.010155));
            mesh.Normals.Add(new Vector3D(0.271764, 0.958749, -0.083332));
            mesh.Normals.Add(new Vector3D(0.240936, 0.958749, -0.150830));
            mesh.Normals.Add(new Vector3D(0.193688, 0.958749, -0.208049));
            mesh.Normals.Add(new Vector3D(0.133242, 0.958749, -0.251090));
            mesh.Normals.Add(new Vector3D(0.063714, 0.958749, -0.277020));
            mesh.Normals.Add(new Vector3D(-0.010155, 0.958749, -0.284071));
            mesh.Normals.Add(new Vector3D(-0.083332, 0.958749, -0.271764));
            mesh.Normals.Add(new Vector3D(-0.150830, 0.958749, -0.240936));
            mesh.Normals.Add(new Vector3D(-0.208049, 0.958749, -0.193688));
            mesh.Normals.Add(new Vector3D(-0.251090, 0.958749, -0.133242));
            mesh.Normals.Add(new Vector3D(-0.277020, 0.958749, -0.063714));
            mesh.Normals.Add(new Vector3D(-0.284071, 0.958749, 0.010155));
            mesh.Normals.Add(new Vector3D(-0.271764, 0.958749, 0.083332));
            mesh.Normals.Add(new Vector3D(-0.240936, 0.958749, 0.150830));
            mesh.Normals.Add(new Vector3D(-0.193688, 0.958749, 0.208049));
            mesh.Normals.Add(new Vector3D(-0.133242, 0.958749, 0.251090));
            mesh.Normals.Add(new Vector3D(-0.063714, 0.958749, 0.277020));
            mesh.Normals.Add(new Vector3D(0.010155, 0.958749, 0.284071));
            mesh.Normals.Add(new Vector3D(0.083332, 0.958749, 0.271764));
            mesh.Normals.Add(new Vector3D(0.150830, 0.958749, 0.240936));
            mesh.Normals.Add(new Vector3D(0.208049, 0.958749, 0.193688));
            mesh.Normals.Add(new Vector3D(0.251090, 0.958749, 0.133242));
            mesh.Normals.Add(new Vector3D(0.277020, 0.958749, 0.063714));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
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
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, 0.482963));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
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
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, 0.129410));
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
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            mesh.Normals.Add(new Vector3D(0.863950, 0.447214, -0.231495));
            mesh.Normals.Add(new Vector3D(0.774597, 0.447214, -0.447214));
            mesh.Normals.Add(new Vector3D(0.632456, 0.447214, -0.632456));
            mesh.Normals.Add(new Vector3D(0.447214, 0.447214, -0.774597));
            mesh.Normals.Add(new Vector3D(0.231495, 0.447214, -0.863950));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            mesh.Normals.Add(new Vector3D(-0.231495, 0.447214, -0.863950));
            mesh.Normals.Add(new Vector3D(-0.447214, 0.447214, -0.774597));
            mesh.Normals.Add(new Vector3D(-0.632456, 0.447214, -0.632456));
            mesh.Normals.Add(new Vector3D(-0.774597, 0.447214, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.863950, 0.447214, -0.231495));
            mesh.Normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.863950, 0.447214, 0.231495));
            mesh.Normals.Add(new Vector3D(-0.774597, 0.447214, 0.447214));
            mesh.Normals.Add(new Vector3D(-0.632456, 0.447214, 0.632456));
            mesh.Normals.Add(new Vector3D(-0.447214, 0.447214, 0.774597));
            mesh.Normals.Add(new Vector3D(-0.231495, 0.447214, 0.863950));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(0.231495, 0.447214, 0.863950));
            mesh.Normals.Add(new Vector3D(0.447214, 0.447214, 0.774597));
            mesh.Normals.Add(new Vector3D(0.632456, 0.447214, 0.632456));
            mesh.Normals.Add(new Vector3D(0.774597, 0.447214, 0.447214));
            mesh.Normals.Add(new Vector3D(0.863950, 0.447214, 0.231495));
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
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, 0.482963));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
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
            mesh.Positions.Add(new Point3D(-0.000000, -0.000000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, 0.129410));
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
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.707107, 0.707107, -0.000000));
            mesh.Normals.Add(new Vector3D(0.683013, 0.707107, -0.183013));
            mesh.Normals.Add(new Vector3D(0.612372, 0.707107, -0.353553));
            mesh.Normals.Add(new Vector3D(0.500000, 0.707107, -0.500000));
            mesh.Normals.Add(new Vector3D(0.353553, 0.707107, -0.612372));
            mesh.Normals.Add(new Vector3D(0.183013, 0.707107, -0.683013));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.183013, 0.707107, -0.683013));
            mesh.Normals.Add(new Vector3D(-0.353553, 0.707107, -0.612372));
            mesh.Normals.Add(new Vector3D(-0.500000, 0.707107, -0.500000));
            mesh.Normals.Add(new Vector3D(-0.612372, 0.707107, -0.353553));
            mesh.Normals.Add(new Vector3D(-0.683013, 0.707107, -0.183013));
            mesh.Normals.Add(new Vector3D(-0.707107, 0.707107, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.683013, 0.707107, 0.183013));
            mesh.Normals.Add(new Vector3D(-0.612372, 0.707107, 0.353553));
            mesh.Normals.Add(new Vector3D(-0.500000, 0.707107, 0.500000));
            mesh.Normals.Add(new Vector3D(-0.353553, 0.707107, 0.612372));
            mesh.Normals.Add(new Vector3D(-0.183013, 0.707107, 0.683013));
            mesh.Normals.Add(new Vector3D(0.000000, 0.707107, 0.707107));
            mesh.Normals.Add(new Vector3D(0.183013, 0.707107, 0.683013));
            mesh.Normals.Add(new Vector3D(0.353553, 0.707107, 0.612372));
            mesh.Normals.Add(new Vector3D(0.500000, 0.707107, 0.500000));
            mesh.Normals.Add(new Vector3D(0.612372, 0.707107, 0.353553));
            mesh.Normals.Add(new Vector3D(0.683013, 0.707107, 0.183013));
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
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, -0.482963));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.433013));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, -0.353553));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, -0.129410));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.482963, -0.500000, 0.129410));
            mesh.Positions.Add(new Point3D(0.433013, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.353553, -0.500000, 0.353553));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, 0.433013));
            mesh.Positions.Add(new Point3D(0.129410, -0.500000, 0.482963));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
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
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            faceIndex += 5;
            // Right
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLargeC(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Front
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            mesh.Normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            faceIndex += 6;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
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
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLongE(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.499994, -0.500006, -0.000006));
            mesh.Positions.Add(new Point3D(-0.500007, 0.500000, -0.499993));
            mesh.Positions.Add(new Point3D(0.000006, -0.500000, -0.000006));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000006, 0.447202, 0.894433));
            mesh.Normals.Add(new Vector3D(-0.000006, 0.447202, 0.894433));
            mesh.Normals.Add(new Vector3D(-0.000006, 0.447202, 0.894433));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Front
            mesh.Positions.Add(new Point3D(-0.499994, -0.500000, -0.500006));
            mesh.Positions.Add(new Point3D(-0.500007, 0.500000, -0.499993));
            mesh.Positions.Add(new Point3D(0.000006, -0.499994, -0.500006));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000013, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000013, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000013, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Right
            mesh.Positions.Add(new Point3D(0.000006, -0.499994, -0.500006));
            mesh.Positions.Add(new Point3D(-0.500007, 0.500000, -0.499993));
            mesh.Positions.Add(new Point3D(0.000006, -0.500000, -0.000006));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.894422, 0.447225, 0.000006));
            mesh.Normals.Add(new Vector3D(0.894422, 0.447225, 0.000006));
            mesh.Normals.Add(new Vector3D(0.894422, 0.447225, 0.000006));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(-0.499994, -0.500000, -0.500006));
            mesh.Positions.Add(new Point3D(-0.500007, 0.500000, -0.499993));
            mesh.Positions.Add(new Point3D(-0.499994, -0.500006, -0.000006));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000013, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000013, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000013, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Bottom
            mesh.Positions.Add(new Point3D(0.000006, -0.499994, -0.500006));
            mesh.Positions.Add(new Point3D(-0.499994, -0.500006, -0.000006));
            mesh.Positions.Add(new Point3D(-0.499994, -0.500000, -0.500006));
            mesh.Positions.Add(new Point3D(0.000006, -0.500000, -0.000006));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000013, -1.000000, -0.000013));
            mesh.Normals.Add(new Vector3D(0.000013, -1.000000, -0.000013));
            mesh.Normals.Add(new Vector3D(0.000013, -1.000000, -0.000013));
            mesh.Normals.Add(new Vector3D(0.000013, -1.000000, -0.000013));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_PyramidA(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            mesh.Normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, -0.000000));
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
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.000000));
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
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);

            //BuildingBlockThin
            return faceIndex;
        }
        private int AddGeometry_Wall(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354331));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
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
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
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
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354331));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.354331, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, 0.500000, -0.500000));
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
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, 0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 6, faceIndex + 7);
            AddTriangle(mesh.TriangleIndices, faceIndex + 7, faceIndex + 4, faceIndex + 5);
            faceIndex += 8;
            // Top
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.354331, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 3, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 4, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 5, faceIndex + 2, faceIndex + 1);
            faceIndex += 6;
            // Bottom
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
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
            mesh.Positions.Add(new Point3D(-0.354331, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(-0.354331, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, -0.500000));
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
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallBottomRight(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(-0.354331, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, -0.500000));
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
            mesh.Positions.Add(new Point3D(-0.354331, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
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
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
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
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, 0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
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
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallBottomLeft(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Left
            mesh.Positions.Add(new Point3D(0.354331, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.000000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
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
            mesh.Positions.Add(new Point3D(0.354331, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.354331, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.000000, -0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, 0.250000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, 0.250000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250000, 0.354331));
            mesh.Positions.Add(new Point3D(0.354331, 0.250000, 0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250000, -0.500000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
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
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 3, faceIndex + 1);
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250000, 0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.250000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
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
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.354331, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354331));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Back], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
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
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.354331));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.354331));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.354331));
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
        private int AddGeometry_CubeHalf(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Front
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
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
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 4);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            faceIndex += 5;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.500000, 0.000000, -0.500000));
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
            mesh.Positions.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottomA(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.250000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.250000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.250000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
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
            mesh.Positions.Add(new Point3D(-0.500000, -0.250000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.250000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.250000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottomB(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
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
            mesh.Positions.Add(new Point3D(-0.500000, -0.250000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.250000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.250000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.250000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.250000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.250000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
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
        private int AddGeometry_RampBottomC(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
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
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampWedgeBottom(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Front], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Right
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Right], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Left
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Top
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            mesh.Positions.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_Beam(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Back
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391336, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391336, -0.500000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.391336, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.391336, -0.500000, 0.354331));
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
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.354331));
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
            mesh.Positions.Add(new Point3D(0.391336, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391336, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.391336, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391336, -0.500000, 0.354331));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Left], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, -0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-1.000000, 0.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Top
            mesh.Positions.Add(new Point3D(0.391336, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.500000, 0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391336, 0.500000, 0.354331));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Top], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottom
            mesh.Positions.Add(new Point3D(0.391336, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.354331));
            mesh.Positions.Add(new Point3D(0.500000, -0.500000, 0.500000));
            mesh.Positions.Add(new Point3D(0.391336, -0.500000, 0.354331));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.TextureCoordinates.Add(GetUV(colours[(int)EpbBlock.FaceIndex.Bottom], 0.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(mesh.TriangleIndices, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CylinderThin(MeshGeometry3D mesh, EpbColourIndex[] colours, int faceIndex)
        {
            // Front
            mesh.Positions.Add(new Point3D(0.241481, -0.500000, -0.064705));
            mesh.Positions.Add(new Point3D(0.216506, -0.500000, -0.125000));
            mesh.Positions.Add(new Point3D(0.176777, -0.500000, -0.176777));
            mesh.Positions.Add(new Point3D(0.125000, -0.500000, -0.216506));
            mesh.Positions.Add(new Point3D(0.064705, -0.500000, -0.241481));
            mesh.Positions.Add(new Point3D(-0.064705, -0.500000, -0.241481));
            mesh.Positions.Add(new Point3D(-0.125000, -0.500000, -0.216506));
            mesh.Positions.Add(new Point3D(-0.176777, -0.500000, -0.176777));
            mesh.Positions.Add(new Point3D(-0.216506, -0.500000, -0.125000));
            mesh.Positions.Add(new Point3D(-0.241481, -0.500000, -0.064705));
            mesh.Positions.Add(new Point3D(-0.241481, -0.500000, 0.064705));
            mesh.Positions.Add(new Point3D(-0.216506, -0.500000, 0.125000));
            mesh.Positions.Add(new Point3D(-0.176777, -0.500000, 0.176777));
            mesh.Positions.Add(new Point3D(-0.125000, -0.500000, 0.216506));
            mesh.Positions.Add(new Point3D(-0.064705, -0.500000, 0.241481));
            mesh.Positions.Add(new Point3D(0.064705, -0.500000, 0.241481));
            mesh.Positions.Add(new Point3D(0.125000, -0.500000, 0.216506));
            mesh.Positions.Add(new Point3D(0.176777, -0.500000, 0.176777));
            mesh.Positions.Add(new Point3D(0.216506, -0.500000, 0.125000));
            mesh.Positions.Add(new Point3D(0.241481, -0.500000, 0.064705));
            mesh.Positions.Add(new Point3D(0.241481, 0.500000, -0.064705));
            mesh.Positions.Add(new Point3D(0.216506, 0.500000, -0.125000));
            mesh.Positions.Add(new Point3D(0.176777, 0.500000, -0.176777));
            mesh.Positions.Add(new Point3D(0.125000, 0.500000, -0.216506));
            mesh.Positions.Add(new Point3D(0.064705, 0.500000, -0.241481));
            mesh.Positions.Add(new Point3D(-0.064705, 0.500000, -0.241481));
            mesh.Positions.Add(new Point3D(-0.125000, 0.500000, -0.216506));
            mesh.Positions.Add(new Point3D(-0.176777, 0.500000, -0.176777));
            mesh.Positions.Add(new Point3D(-0.216506, 0.500000, -0.125000));
            mesh.Positions.Add(new Point3D(-0.241481, 0.500000, -0.064705));
            mesh.Positions.Add(new Point3D(-0.241481, 0.500000, 0.064705));
            mesh.Positions.Add(new Point3D(-0.216506, 0.500000, 0.125000));
            mesh.Positions.Add(new Point3D(-0.176777, 0.500000, 0.176777));
            mesh.Positions.Add(new Point3D(-0.125000, 0.500000, 0.216506));
            mesh.Positions.Add(new Point3D(-0.064705, 0.500000, 0.241481));
            mesh.Positions.Add(new Point3D(0.064705, 0.500000, 0.241481));
            mesh.Positions.Add(new Point3D(0.125000, 0.500000, 0.216506));
            mesh.Positions.Add(new Point3D(0.176777, 0.500000, 0.176777));
            mesh.Positions.Add(new Point3D(0.216506, 0.500000, 0.125000));
            mesh.Positions.Add(new Point3D(0.241481, 0.500000, 0.064705));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.250000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.250000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.000000));
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
            mesh.Normals.Add(new Vector3D(0.976344, 0.000000, -0.216222));
            mesh.Normals.Add(new Vector3D(0.887114, 0.000000, -0.461551));
            mesh.Normals.Add(new Vector3D(0.737428, 0.000000, -0.675426));
            mesh.Normals.Add(new Vector3D(0.537487, 0.000000, -0.843272));
            mesh.Normals.Add(new Vector3D(0.300918, 0.000000, -0.953650));
            mesh.Normals.Add(new Vector3D(-0.216222, 0.000000, -0.976344));
            mesh.Normals.Add(new Vector3D(-0.461551, 0.000000, -0.887114));
            mesh.Normals.Add(new Vector3D(-0.675426, 0.000000, -0.737428));
            mesh.Normals.Add(new Vector3D(-0.843272, 0.000000, -0.537487));
            mesh.Normals.Add(new Vector3D(-0.953650, 0.000000, -0.300918));
            mesh.Normals.Add(new Vector3D(-0.976344, -0.000000, 0.216222));
            mesh.Normals.Add(new Vector3D(-0.887114, -0.000000, 0.461551));
            mesh.Normals.Add(new Vector3D(-0.737428, -0.000000, 0.675426));
            mesh.Normals.Add(new Vector3D(-0.537487, -0.000000, 0.843272));
            mesh.Normals.Add(new Vector3D(-0.300918, -0.000000, 0.953650));
            mesh.Normals.Add(new Vector3D(0.216222, -0.000000, 0.976344));
            mesh.Normals.Add(new Vector3D(0.461551, -0.000000, 0.887114));
            mesh.Normals.Add(new Vector3D(0.675426, -0.000000, 0.737428));
            mesh.Normals.Add(new Vector3D(0.843272, -0.000000, 0.537487));
            mesh.Normals.Add(new Vector3D(0.953650, -0.000000, 0.300918));
            mesh.Normals.Add(new Vector3D(0.953650, 0.000000, -0.300918));
            mesh.Normals.Add(new Vector3D(0.843272, 0.000000, -0.537487));
            mesh.Normals.Add(new Vector3D(0.675426, 0.000000, -0.737428));
            mesh.Normals.Add(new Vector3D(0.461551, 0.000000, -0.887114));
            mesh.Normals.Add(new Vector3D(0.216222, 0.000000, -0.976344));
            mesh.Normals.Add(new Vector3D(-0.300918, 0.000000, -0.953650));
            mesh.Normals.Add(new Vector3D(-0.537487, 0.000000, -0.843272));
            mesh.Normals.Add(new Vector3D(-0.737428, 0.000000, -0.675426));
            mesh.Normals.Add(new Vector3D(-0.887114, 0.000000, -0.461551));
            mesh.Normals.Add(new Vector3D(-0.976344, 0.000000, -0.216222));
            mesh.Normals.Add(new Vector3D(-0.953650, -0.000000, 0.300918));
            mesh.Normals.Add(new Vector3D(-0.843272, -0.000000, 0.537487));
            mesh.Normals.Add(new Vector3D(-0.675426, -0.000000, 0.737428));
            mesh.Normals.Add(new Vector3D(-0.461551, -0.000000, 0.887114));
            mesh.Normals.Add(new Vector3D(-0.216222, -0.000000, 0.976344));
            mesh.Normals.Add(new Vector3D(0.300918, -0.000000, 0.953650));
            mesh.Normals.Add(new Vector3D(0.537487, -0.000000, 0.843272));
            mesh.Normals.Add(new Vector3D(0.737428, -0.000000, 0.675426));
            mesh.Normals.Add(new Vector3D(0.887114, -0.000000, 0.461551));
            mesh.Normals.Add(new Vector3D(0.976344, -0.000000, 0.216222));
            mesh.Normals.Add(new Vector3D(-0.043842, -0.000000, 0.999038));
            mesh.Normals.Add(new Vector3D(0.043842, -0.000000, 0.999038));
            mesh.Normals.Add(new Vector3D(-0.043842, 0.000000, -0.999038));
            mesh.Normals.Add(new Vector3D(0.043842, 0.000000, -0.999038));
            mesh.Normals.Add(new Vector3D(-0.999038, -0.000000, 0.043842));
            mesh.Normals.Add(new Vector3D(-0.999038, 0.000000, -0.043842));
            mesh.Normals.Add(new Vector3D(0.999038, 0.000000, -0.043842));
            mesh.Normals.Add(new Vector3D(0.999038, -0.000000, 0.043842));
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
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.241481, 0.500000, -0.064705));
            mesh.Positions.Add(new Point3D(0.216506, 0.500000, -0.125000));
            mesh.Positions.Add(new Point3D(0.176777, 0.500000, -0.176777));
            mesh.Positions.Add(new Point3D(0.125000, 0.500000, -0.216506));
            mesh.Positions.Add(new Point3D(0.064705, 0.500000, -0.241481));
            mesh.Positions.Add(new Point3D(-0.064705, 0.500000, -0.241481));
            mesh.Positions.Add(new Point3D(-0.125000, 0.500000, -0.216506));
            mesh.Positions.Add(new Point3D(-0.176777, 0.500000, -0.176777));
            mesh.Positions.Add(new Point3D(-0.216506, 0.500000, -0.125000));
            mesh.Positions.Add(new Point3D(-0.241481, 0.500000, -0.064705));
            mesh.Positions.Add(new Point3D(-0.241481, 0.500000, 0.064705));
            mesh.Positions.Add(new Point3D(-0.216506, 0.500000, 0.125000));
            mesh.Positions.Add(new Point3D(-0.176777, 0.500000, 0.176777));
            mesh.Positions.Add(new Point3D(-0.125000, 0.500000, 0.216506));
            mesh.Positions.Add(new Point3D(-0.064705, 0.500000, 0.241481));
            mesh.Positions.Add(new Point3D(0.064705, 0.500000, 0.241481));
            mesh.Positions.Add(new Point3D(0.125000, 0.500000, 0.216506));
            mesh.Positions.Add(new Point3D(0.176777, 0.500000, 0.176777));
            mesh.Positions.Add(new Point3D(0.216506, 0.500000, 0.125000));
            mesh.Positions.Add(new Point3D(0.241481, 0.500000, 0.064705));
            mesh.Positions.Add(new Point3D(-0.000000, 0.500000, -0.250000));
            mesh.Positions.Add(new Point3D(-0.250000, 0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.250000, 0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(0.000000, 0.500000, 0.250000));
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
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
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
            mesh.Positions.Add(new Point3D(0.241481, -0.500000, -0.064705));
            mesh.Positions.Add(new Point3D(0.216506, -0.500000, -0.125000));
            mesh.Positions.Add(new Point3D(0.176777, -0.500000, -0.176777));
            mesh.Positions.Add(new Point3D(0.125000, -0.500000, -0.216506));
            mesh.Positions.Add(new Point3D(0.064705, -0.500000, -0.241481));
            mesh.Positions.Add(new Point3D(-0.064705, -0.500000, -0.241481));
            mesh.Positions.Add(new Point3D(-0.125000, -0.500000, -0.216506));
            mesh.Positions.Add(new Point3D(-0.176777, -0.500000, -0.176777));
            mesh.Positions.Add(new Point3D(-0.216506, -0.500000, -0.125000));
            mesh.Positions.Add(new Point3D(-0.241481, -0.500000, -0.064705));
            mesh.Positions.Add(new Point3D(-0.241481, -0.500000, 0.064705));
            mesh.Positions.Add(new Point3D(-0.216506, -0.500000, 0.125000));
            mesh.Positions.Add(new Point3D(-0.176777, -0.500000, 0.176777));
            mesh.Positions.Add(new Point3D(-0.125000, -0.500000, 0.216506));
            mesh.Positions.Add(new Point3D(-0.064705, -0.500000, 0.241481));
            mesh.Positions.Add(new Point3D(0.064705, -0.500000, 0.241481));
            mesh.Positions.Add(new Point3D(0.125000, -0.500000, 0.216506));
            mesh.Positions.Add(new Point3D(0.176777, -0.500000, 0.176777));
            mesh.Positions.Add(new Point3D(0.216506, -0.500000, 0.125000));
            mesh.Positions.Add(new Point3D(0.241481, -0.500000, 0.064705));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.250000, -0.500000, 0.000000));
            mesh.Positions.Add(new Point3D(0.000000, -0.500000, 0.250000));
            mesh.Positions.Add(new Point3D(0.250000, -0.500000, -0.000000));
            mesh.Positions.Add(new Point3D(-0.000000, -0.500000, -0.250000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, 0.000000, -0.500000));
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
            mesh.Positions.Add(new Point3D(0.500000, -0.000000, 0.500000));
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
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            mesh.Normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(mesh.TriangleIndices, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        #endregion AddGeometry
    }
}
