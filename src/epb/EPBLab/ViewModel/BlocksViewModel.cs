
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
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

                GroupNode typeNode = (GroupNode)categoryNode.Children.FirstOrDefault(x => x.Title == blockNode.Title);
                if (typeNode == null)
                {
                    typeNode = new GroupNode() { Title = blockNode.Title };
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
            Point3DCollection vertices = new Point3DCollection();
            Int32Collection triangles = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();

            EpbColourIndex colourIndex = block.Colours[0];
            EpbColour c = blueprint.Palette[colourIndex];
            Color colour = Color.FromArgb(255, c.R, c.G, c.B); //128

            int faceIndex = 0;

            Vector3D v1;
            Vector3D v2;
            Vector3D normal;

            switch (block.Variant)
            {
                case 0: //Cube
                    faceIndex = AddGeometry_Cube(vertices, triangles, normals, faceIndex);
                    break;
                case 1: //CutCorner
                    faceIndex = AddGeometry_CutCorner(vertices, triangles, normals, faceIndex);
                    break;
                case 2: //CornerLongA
                    faceIndex = AddGeometry_CornerLongA(vertices, triangles, normals, faceIndex);
                    break;
                case 3: //CornerLongB
                    faceIndex = AddGeometry_CornerLongB(vertices, triangles, normals, faceIndex);
                    break;
                case 4: //CornerLongC
                    faceIndex = AddGeometry_CornerLongC(vertices, triangles, normals, faceIndex);
                    break;
                case 5: //CornerLongD
                    faceIndex = AddGeometry_CornerLongD(vertices, triangles, normals, faceIndex);
                    break;
                case 6: //CornerLargeA
                    faceIndex = AddGeometry_CornerLargeA(vertices, triangles, normals, faceIndex);
                    break;
                case 7: //Corner
                    faceIndex = AddGeometry_Corner(vertices, triangles, normals, faceIndex);
                    break;
                case 8: //RampBottom
                    faceIndex = AddGeometry_RampBottom(vertices, triangles, normals, faceIndex);
                    break;
                case 9: //RampTop
                    faceIndex = AddGeometry_RampTop(vertices, triangles, normals, faceIndex);
                    break;
                case 10: //Slope
                    faceIndex = AddGeometry_Slope(vertices, triangles, normals, faceIndex);
                    break;
                /*
                case 11: //CurvedCorner
                    break;
                case 12: //RoundCutCorner
                    break;
                case 13: //RoundCorner
                    break;
                case 14: //RoundCornerLong
                    break;
                case 15: //RoundSlope
                    break;
                case 16: //Cylinder
                    break;
                case 17: //InvardCorner
                    break;
                case 18: //InwardRoundSlope
                    break;
                case 19: //InwardCurvedCorner
                    break;
                case 20: //RoundSlopeEdgeInward
                    break;
                case 21: //CylinderEndA
                    break;
                case 22: //CylinderEndB
                    break;
                case 23: //CylinderEndC
                    break;
                case 24: //RampWedgeTop
                    break;
                case 25: //Round4WayConnector
                    break;
                case 26: //RoundSlopeEdge
                    break;
                */
                case 27: //CornerLargeB
                    faceIndex = AddGeometry_CornerLargeB(vertices, triangles, normals, faceIndex);
                    break;
                case 28: //CornerLargeC
                    faceIndex = AddGeometry_CornerLargeC(vertices, triangles, normals, faceIndex);
                    break;
                case 29: //CornerLargeD
                    faceIndex = AddGeometry_CornerLargeD(vertices, triangles, normals, faceIndex);
                    break;
                case 30: //CornerLongE
                    faceIndex = AddGeometry_CornerLongE(vertices, triangles, normals, faceIndex);
                    break;
                case 31: //PyramidA
                    faceIndex = AddGeometry_PyramidA(vertices, triangles, normals, faceIndex);
                    break;
                default:
                    faceIndex = AddGeometry_Cube(vertices, triangles, normals, faceIndex);
                    break;
            }
            mesh.Positions = vertices;
            mesh.TriangleIndices = triangles;
            mesh.Normals = normals;

            model.Geometry = mesh;
            Transform3DGroup tg = new Transform3DGroup();
            tg.Children.Add(new RotateTransform3D(new QuaternionRotation3D(Rotation[block.Rotation])));
            tg.Children.Add(new TranslateTransform3D(pos.X, pos.Y, pos.Z));
            model.Transform = tg;

            SolidColorBrush brush = new SolidColorBrush(colour);
            var material = new DiffuseMaterial(brush);
            model.Material = material;

            return model;
        }

        private GeometryModel3D CreateBuildingBlockThin(Point3D pos, EpbBlock block, EpBlueprint blueprint)
        {
            GeometryModel3D model = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();
            Point3DCollection vertices = new Point3DCollection();
            Int32Collection triangles = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();

            EpbColourIndex colourIndex = block.Colours[0];
            EpbColour c = blueprint.Palette[colourIndex];
            Color colour = Color.FromArgb(255, c.R, c.G, c.B); //128

            int faceIndex = 0;

            Vector3D v1;
            Vector3D v2;
            Vector3D normal;

            switch (block.Variant)
            {
                case 0: //Wall
                    faceIndex = AddGeometry_Wall(vertices, triangles, normals, faceIndex);
                    break;
                case 1: //Wall L-shape
                    faceIndex = AddGeometry_WallLShape(vertices, triangles, normals, faceIndex);
                    break;
                /*
                case 2: //Thin Slope
                    break;
                case 3: //Thin Corner
                    break;
                */
                case 4: //Sloped Wall
                    faceIndex = AddGeometry_SlopedWall(vertices, triangles, normals, faceIndex);
                    break;
                case 5: //Sloped Wall Bottom (right)
                    faceIndex = AddGeometry_SlopedWallBottomRight(vertices, triangles, normals, faceIndex);
                    break;
                case 6: //Sloped Wall Top (right)
                    faceIndex = AddGeometry_SlopedWallTopRight(vertices, triangles, normals, faceIndex);
                    break;
                case 7: //Sloped Wall Bottom (left)
                    faceIndex = AddGeometry_SlopedWallBottomLeft(vertices, triangles, normals, faceIndex);
                    break;
                case 8: //Sloped Wall Top (left)
                    faceIndex = AddGeometry_SlopedWallTopLeft(vertices, triangles, normals, faceIndex);
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
                    faceIndex = AddGeometry_Wall3Corner(vertices, triangles, normals, faceIndex);
                    break;
                case 17: //Wall Half
                    faceIndex = AddGeometry_WallHalf(vertices, triangles, normals, faceIndex);
                    break;
                case 18: //Cube Half
                    faceIndex = AddGeometry_CubeHalf(vertices, triangles, normals, faceIndex);
                    break;
                case 19: //Ramp Top Double
                    faceIndex = AddGeometry_RampTopDouble(vertices, triangles, normals, faceIndex);
                    break;
                case 20: //Ramp Bottom A
                    faceIndex = AddGeometry_RampBottomA(vertices, triangles, normals, faceIndex);
                    break;
                case 21: //Ramp Bottom B
                    faceIndex = AddGeometry_RampBottomB(vertices, triangles, normals, faceIndex);
                    break;
                case 22: //Ramp Bottom C
                    faceIndex = AddGeometry_RampBottomC(vertices, triangles, normals, faceIndex);
                    break;
                case 23: //Ramp Wedge Bottom
                    faceIndex = AddGeometry_RampWedgeBottom(vertices, triangles, normals, faceIndex);
                    break;
                case 24: //Beam
                    faceIndex = AddGeometry_Beam(vertices, triangles, normals, faceIndex);
                    break;
                /*
                case 25: //Cylinder Thin
                    break;
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
                    faceIndex = AddGeometry_SlopeHalf(vertices, triangles, normals, faceIndex);
                    break;
                default:
                    faceIndex = AddGeometry_Cube(vertices, triangles, normals, faceIndex);
                    break;
            }
            mesh.Positions = vertices;
            mesh.TriangleIndices = triangles;
            mesh.Normals = normals;

            model.Geometry = mesh;
            Transform3DGroup tg = new Transform3DGroup();
            tg.Children.Add(new RotateTransform3D(new QuaternionRotation3D(Rotation[block.Rotation])));
            tg.Children.Add(new TranslateTransform3D(pos.X, pos.Y, pos.Z));
            model.Transform = tg;

            SolidColorBrush brush = new SolidColorBrush(colour);
            var material = new DiffuseMaterial(brush);
            model.Material = material;

            return model;
        }

        private GeometryModel3D CreateBox(Point3D pos, Color colour)
        {
            GeometryModel3D model = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();
            Point3DCollection vertices = new Point3DCollection();
            Int32Collection triangles = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();

            int faceIndex = 0;
            faceIndex = AddGeometry_Cube(vertices, triangles, normals, faceIndex);

            mesh.Positions = vertices;
            mesh.TriangleIndices = triangles;
            mesh.Normals = normals;

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
        #region AddGeometry

        //BuildingBlockFull
        private int AddGeometry_Cube(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CutCorner(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLongA(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            vertices.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 2, faceIndex + 4);
            faceIndex += 5;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Insideface
            vertices.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLongB(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 4, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 4);
            faceIndex += 5;
            // Insideface
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            return faceIndex;
        }
        private int AddGeometry_CornerLongC(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(0.000000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Insideface
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            return faceIndex;
        }
        private int AddGeometry_CornerLongD(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            normals.Add(new Vector3D(-0.666667, 0.333333, -0.666667));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Frontface
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLargeA(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            normals.Add(new Vector3D(0.707107, 0.707107, -0.000000));
            normals.Add(new Vector3D(0.707107, 0.707107, -0.000000));
            normals.Add(new Vector3D(0.707107, 0.707107, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, 0.707107, 0.707107));
            normals.Add(new Vector3D(0.000000, 0.707107, 0.707107));
            normals.Add(new Vector3D(0.000000, 0.707107, 0.707107));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_Corner(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            normals.Add(new Vector3D(-0.577350, 0.577350, -0.577350));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottom(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampTop(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 3);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 4, faceIndex + 3);
            return faceIndex;
        }
        private int AddGeometry_Slope(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLargeB(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            faceIndex += 5;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.000000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.525731, -0.850651));
            normals.Add(new Vector3D(-0.000000, 0.343279, -0.939234));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 4);
            faceIndex += 5;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.000000, -0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-0.447214, 0.894427, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-0.850651, 0.525731, 0.000000));
            normals.Add(new Vector3D(-0.939234, 0.343279, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            faceIndex += 5;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 2, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            faceIndex += 5;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLargeC(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Insideface
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            return faceIndex;
        }
        private int AddGeometry_CornerLargeD(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.000000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(0.000000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CornerLongE(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(-0.499994, -0.500000, -0.500006));
            vertices.Add(new Point3D(-0.500007, 0.500000, -0.499993));
            vertices.Add(new Point3D(0.000006, -0.499994, -0.500006));
            normals.Add(new Vector3D(-0.000000, 0.000013, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000013, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000013, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Backface
            vertices.Add(new Point3D(-0.499994, -0.500006, -0.000006));
            vertices.Add(new Point3D(-0.500007, 0.500000, -0.499993));
            vertices.Add(new Point3D(0.000006, -0.500000, -0.000006));
            normals.Add(new Vector3D(-0.000006, 0.447202, 0.894433));
            normals.Add(new Vector3D(-0.000006, 0.447202, 0.894433));
            normals.Add(new Vector3D(-0.000006, 0.447202, 0.894433));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Rightface
            vertices.Add(new Point3D(0.000006, -0.499994, -0.500006));
            vertices.Add(new Point3D(-0.500007, 0.500000, -0.499993));
            vertices.Add(new Point3D(0.000006, -0.500000, -0.000006));
            normals.Add(new Vector3D(0.894422, 0.447225, 0.000006));
            normals.Add(new Vector3D(0.894422, 0.447225, 0.000006));
            normals.Add(new Vector3D(0.894422, 0.447225, 0.000006));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(-0.499994, -0.500000, -0.500006));
            vertices.Add(new Point3D(-0.500007, 0.500000, -0.499993));
            vertices.Add(new Point3D(-0.499994, -0.500006, -0.000006));
            normals.Add(new Vector3D(-1.000000, -0.000013, -0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000013, -0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000013, -0.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(0.000006, -0.499994, -0.500006));
            vertices.Add(new Point3D(-0.499994, -0.500006, -0.000006));
            vertices.Add(new Point3D(-0.499994, -0.500000, -0.500006));
            vertices.Add(new Point3D(0.000006, -0.500000, -0.000006));
            normals.Add(new Vector3D(0.000013, -1.000000, -0.000013));
            normals.Add(new Vector3D(0.000013, -1.000000, -0.000013));
            normals.Add(new Vector3D(0.000013, -1.000000, -0.000013));
            normals.Add(new Vector3D(0.000013, -1.000000, -0.000013));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_PyramidA(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.000000));
            normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, -0.000000));
            normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, -0.000000));
            normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, -0.000000));
            normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);

            //BuildingBlockThin
            return faceIndex;
        }
        private int AddGeometry_Wall(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.250000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.250000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.250000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.250000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.250000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_WallLShape(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(0.250000, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.250000, 0.500000, 0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.250000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, 0.500000, -0.500000));
            normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 4, faceIndex + 1, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 0, faceIndex + 1);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 6;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 1);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.250000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(0.250000, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 5, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 2, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 0, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 4, faceIndex + 3);
            faceIndex += 6;
            // Backface
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, 0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 1);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.250000));
            vertices.Add(new Point3D(0.250000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, 0.500000, 0.250000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 5, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 4, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 2, faceIndex + 1);
            return faceIndex;
        }
        private int AddGeometry_SlopedWall(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallBottomRight(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.250000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(-0.250000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallTopRight(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.250000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallBottomLeft(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.250000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Frontface
            vertices.Add(new Point3D(0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.250000, -0.000000, 0.500000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Rightface
            vertices.Add(new Point3D(0.250000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            return faceIndex;
        }
        private int AddGeometry_SlopedWallTopLeft(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.250000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.250000, -0.000000, -0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.250000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.250000, -0.000000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            return faceIndex;
        }
        private int AddGeometry_Wall3Corner(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(0.250000, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, 0.250000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.250000, 0.250000));
            vertices.Add(new Point3D(0.250000, 0.250000, 0.250000));
            vertices.Add(new Point3D(-0.500000, 0.250000, -0.500000));
            normals.Add(new Vector3D(-0.707107, 0.000000, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-0.707107, -0.707107, 0.000000));
            normals.Add(new Vector3D(-0.000000, -0.707107, -0.707107));
            normals.Add(new Vector3D(-0.577350, -0.577350, -0.577350));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 2, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 4, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 3, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 4, faceIndex + 5);
            faceIndex += 7;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 1);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, 0.250000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.250000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 5, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 4, faceIndex + 0);
            faceIndex += 6;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.500000, 0.250000, 0.250000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.250000, -0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 2, faceIndex + 1);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 6;
            // Bottomface
            vertices.Add(new Point3D(0.250000, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 3, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 5);
            return faceIndex;
        }
        private int AddGeometry_WallHalf(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.250000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.250000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.250000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.250000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.250000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_CubeHalf(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampTopDouble(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.382683, -0.923880));
            normals.Add(new Vector3D(-0.000000, 0.382683, -0.923880));
            AddTriangle(triangles, faceIndex + 5, faceIndex + 0, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 6, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 6, faceIndex + 4);
            faceIndex += 7;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            vertices.Add(new Point3D(-0.500000, 0.000000, -0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 3);
            faceIndex += 5;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            faceIndex += 5;
            // Topface
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottomA(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.250000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.250000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.250000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.250000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.250000, 0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.250000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottomB(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.250000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.250000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.250000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.250000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.250000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.250000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            normals.Add(new Vector3D(-0.000000, 0.970143, -0.242536));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampBottomC(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RampWedgeBottom(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(-0.408248, 0.816497, -0.408248));
            normals.Add(new Vector3D(-0.408248, 0.816497, -0.408248));
            normals.Add(new Vector3D(-0.408248, 0.816497, -0.408248));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 2);
            faceIndex += 3;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_Beam(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Frontface
            vertices.Add(new Point3D(0.227273, -0.500000, 0.227273));
            vertices.Add(new Point3D(-0.272727, 0.500000, 0.227273));
            vertices.Add(new Point3D(0.227273, 0.500000, 0.227273));
            vertices.Add(new Point3D(-0.272727, -0.500000, 0.227273));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Backface
            vertices.Add(new Point3D(0.227273, -0.500000, -0.272727));
            vertices.Add(new Point3D(-0.272727, 0.500000, -0.272727));
            vertices.Add(new Point3D(0.227273, 0.500000, -0.272727));
            vertices.Add(new Point3D(-0.272727, -0.500000, -0.272727));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.272727, -0.500000, 0.227273));
            vertices.Add(new Point3D(-0.272727, 0.500000, -0.272727));
            vertices.Add(new Point3D(-0.272727, 0.500000, 0.227273));
            vertices.Add(new Point3D(-0.272727, -0.500000, -0.272727));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.227273, -0.500000, 0.227273));
            vertices.Add(new Point3D(0.227273, 0.500000, -0.272727));
            vertices.Add(new Point3D(0.227273, 0.500000, 0.227273));
            vertices.Add(new Point3D(0.227273, -0.500000, -0.272727));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.272727, 0.500000, 0.227273));
            vertices.Add(new Point3D(0.227273, 0.500000, -0.272727));
            vertices.Add(new Point3D(0.227273, 0.500000, 0.227273));
            vertices.Add(new Point3D(-0.272727, 0.500000, -0.272727));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Bottomface
            vertices.Add(new Point3D(-0.272727, -0.500000, 0.227273));
            vertices.Add(new Point3D(0.227273, -0.500000, -0.272727));
            vertices.Add(new Point3D(0.227273, -0.500000, 0.227273));
            vertices.Add(new Point3D(-0.272727, -0.500000, -0.272727));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_SlopeHalf(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            normals.Add(new Vector3D(-0.000000, 0.894427, -0.447214));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        #endregion AddGeometry
    }
}
