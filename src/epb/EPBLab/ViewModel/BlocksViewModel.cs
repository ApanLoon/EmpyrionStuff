
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
                case 11: //CurvedCorner
                    faceIndex = AddGeometry_CurvedCorner(vertices, triangles, normals, faceIndex);
                    break;
                /*
                case 12: //RoundCutCorner
                    break;
                */
                case 13: //RoundCorner
                    faceIndex = AddGeometry_RoundCorner(vertices, triangles, normals, faceIndex);
                    break;
                /*
                case 14: //RoundCornerLong
                    break;
                */
                case 15: //RoundSlope
                    faceIndex = AddGeometry_RoundSlope(vertices, triangles, normals, faceIndex);
                    break;
                case 16: //Cylinder
                    faceIndex = AddGeometry_Cylinder(vertices, triangles, normals, faceIndex);
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
                    faceIndex = AddGeometry_CylinderEndA(vertices, triangles, normals, faceIndex);
                    break;
                /*
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
                case 25: //Cylinder Thin
                    faceIndex = AddGeometry_CylinderThin(vertices, triangles, normals, faceIndex);
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
        private int AddGeometry_CurvedCorner(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Topface
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.465926, -0.500000, 0.241181));
            vertices.Add(new Point3D(0.366025, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.207107, -0.500000, -0.207107));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.366025));
            vertices.Add(new Point3D(-0.241181, -0.500000, -0.465926));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(0.476107, 0.739354, -0.476107));
            normals.Add(new Vector3D(0.704063, 0.704063, -0.092692));
            normals.Add(new Vector3D(0.683013, 0.707107, -0.183013));
            normals.Add(new Vector3D(0.612372, 0.707107, -0.353553));
            normals.Add(new Vector3D(0.500000, 0.707107, -0.500000));
            normals.Add(new Vector3D(0.353553, 0.707107, -0.612372));
            normals.Add(new Vector3D(0.183013, 0.707107, -0.683013));
            normals.Add(new Vector3D(0.092692, 0.704063, -0.704063));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.241181, -0.500000, -0.465926));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.366025));
            vertices.Add(new Point3D(0.207107, -0.500000, -0.207107));
            vertices.Add(new Point3D(0.366025, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.465926, -0.500000, 0.241181));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        private int AddGeometry_RoundCorner(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Topface
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.465926, -0.500000, 0.241181));
            vertices.Add(new Point3D(0.366025, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.207107, -0.500000, -0.207107));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.366025));
            vertices.Add(new Point3D(-0.241181, -0.500000, -0.465926));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.465926, -0.241181, 0.500000));
            vertices.Add(new Point3D(0.433013, -0.241181, 0.250000));
            vertices.Add(new Point3D(0.336516, -0.241181, 0.017037));
            vertices.Add(new Point3D(0.183013, -0.241181, -0.183013));
            vertices.Add(new Point3D(-0.017037, -0.241181, -0.336516));
            vertices.Add(new Point3D(-0.250000, -0.241181, -0.433013));
            vertices.Add(new Point3D(-0.500000, -0.241181, -0.465926));
            vertices.Add(new Point3D(0.366025, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.336516, -0.000000, 0.275856));
            vertices.Add(new Point3D(0.250000, -0.000000, 0.066987));
            vertices.Add(new Point3D(0.112372, -0.000000, -0.112372));
            vertices.Add(new Point3D(-0.066987, -0.000000, -0.250000));
            vertices.Add(new Point3D(-0.275856, -0.000000, -0.336516));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.366025));
            vertices.Add(new Point3D(0.207107, 0.207107, 0.500000));
            vertices.Add(new Point3D(0.183013, 0.207107, 0.316987));
            vertices.Add(new Point3D(0.112372, 0.207107, 0.146447));
            vertices.Add(new Point3D(-0.000000, 0.207107, -0.000000));
            vertices.Add(new Point3D(-0.146447, 0.207107, -0.112372));
            vertices.Add(new Point3D(-0.316987, 0.207107, -0.183013));
            vertices.Add(new Point3D(-0.500000, 0.207107, -0.207107));
            vertices.Add(new Point3D(0.000000, 0.366025, 0.500000));
            vertices.Add(new Point3D(-0.017037, 0.366025, 0.370590));
            vertices.Add(new Point3D(-0.066987, 0.366025, 0.250000));
            vertices.Add(new Point3D(-0.146447, 0.366025, 0.146447));
            vertices.Add(new Point3D(-0.250000, 0.366025, 0.066987));
            vertices.Add(new Point3D(-0.370590, 0.366025, 0.017037));
            vertices.Add(new Point3D(-0.500000, 0.366025, 0.000000));
            vertices.Add(new Point3D(-0.241181, 0.465926, 0.500000));
            vertices.Add(new Point3D(-0.250000, 0.465926, 0.433013));
            vertices.Add(new Point3D(-0.275856, 0.465926, 0.370590));
            vertices.Add(new Point3D(-0.316987, 0.465926, 0.316987));
            vertices.Add(new Point3D(-0.370590, 0.465926, 0.275856));
            vertices.Add(new Point3D(-0.433013, 0.465926, 0.250000));
            vertices.Add(new Point3D(-0.500000, 0.465926, 0.241181));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.983106, 0.129428, -0.129428));
            normals.Add(new Vector3D(0.968007, 0.130403, -0.214376));
            normals.Add(new Vector3D(0.879539, 0.130403, -0.457610));
            normals.Add(new Vector3D(0.731131, 0.130403, -0.669659));
            normals.Add(new Vector3D(0.532898, 0.130403, -0.836071));
            normals.Add(new Vector3D(0.298349, 0.130403, -0.945507));
            normals.Add(new Vector3D(0.129428, 0.129428, -0.983106));
            normals.Add(new Vector3D(0.968364, 0.214520, -0.127487));
            normals.Add(new Vector3D(0.932613, 0.258889, -0.251417));
            normals.Add(new Vector3D(0.835763, 0.258889, -0.484228));
            normals.Add(new Vector3D(0.681958, 0.258889, -0.684039));
            normals.Add(new Vector3D(0.481678, 0.258889, -0.837235));
            normals.Add(new Vector3D(0.248573, 0.258889, -0.933374));
            normals.Add(new Vector3D(0.124570, 0.298635, -0.946203));
            normals.Add(new Vector3D(0.881074, 0.458534, -0.115996));
            normals.Add(new Vector3D(0.835714, 0.500107, -0.226882));
            normals.Add(new Vector3D(0.748516, 0.500107, -0.435450));
            normals.Add(new Vector3D(0.610308, 0.500107, -0.614342));
            normals.Add(new Vector3D(0.430509, 0.500107, -0.751368));
            normals.Add(new Vector3D(0.221372, 0.500107, -0.837190));
            normals.Add(new Vector3D(0.110333, 0.534300, -0.838063));
            normals.Add(new Vector3D(0.733890, 0.672361, -0.096618));
            normals.Add(new Vector3D(0.681859, 0.707204, -0.186897));
            normals.Add(new Vector3D(0.610252, 0.707204, -0.357007));
            normals.Add(new Vector3D(0.497058, 0.707204, -0.502787));
            normals.Add(new Vector3D(0.349991, 0.707204, -0.614303));
            normals.Add(new Vector3D(0.179072, 0.707204, -0.683956));
            normals.Add(new Vector3D(0.088560, 0.734618, -0.672677));
            normals.Add(new Vector3D(0.536052, 0.841230, -0.070573));
            normals.Add(new Vector3D(0.481561, 0.866078, -0.134192));
            normals.Add(new Vector3D(0.430421, 0.866078, -0.254256));
            normals.Add(new Vector3D(0.349948, 0.866078, -0.356994));
            normals.Add(new Vector3D(0.245627, 0.866078, -0.435403));
            normals.Add(new Vector3D(0.124567, 0.866078, -0.484140));
            normals.Add(new Vector3D(0.060639, 0.885533, -0.460602));
            normals.Add(new Vector3D(0.300620, 0.952922, -0.039577));
            normals.Add(new Vector3D(0.271764, 0.958749, -0.083332));
            normals.Add(new Vector3D(0.240936, 0.958749, -0.150830));
            normals.Add(new Vector3D(0.193688, 0.958749, -0.208049));
            normals.Add(new Vector3D(0.133242, 0.958749, -0.251090));
            normals.Add(new Vector3D(0.063714, 0.958749, -0.277020));
            normals.Add(new Vector3D(0.034045, 0.965385, -0.258598));
            normals.Add(new Vector3D(0.084175, 0.992889, -0.084175));
            AddTriangle(triangles, faceIndex + 42, faceIndex + 40, faceIndex + 41);
            AddTriangle(triangles, faceIndex + 42, faceIndex + 39, faceIndex + 40);
            AddTriangle(triangles, faceIndex + 42, faceIndex + 38, faceIndex + 39);
            AddTriangle(triangles, faceIndex + 42, faceIndex + 37, faceIndex + 38);
            AddTriangle(triangles, faceIndex + 42, faceIndex + 36, faceIndex + 37);
            AddTriangle(triangles, faceIndex + 42, faceIndex + 35, faceIndex + 36);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 7, faceIndex + 1);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 8, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 9, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 11, faceIndex + 10, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 5, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 12, faceIndex + 11, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 6, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 13, faceIndex + 12, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 8, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 15, faceIndex + 14, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 9, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 16, faceIndex + 15, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 10, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 17, faceIndex + 16, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 11, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 18, faceIndex + 17, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 11, faceIndex + 12, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 19, faceIndex + 18, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 12, faceIndex + 13, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 20, faceIndex + 19, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 14, faceIndex + 15, faceIndex + 21);
            AddTriangle(triangles, faceIndex + 22, faceIndex + 21, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 15, faceIndex + 16, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 23, faceIndex + 22, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 16, faceIndex + 17, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 24, faceIndex + 23, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 17, faceIndex + 18, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 25, faceIndex + 24, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 18, faceIndex + 19, faceIndex + 25);
            AddTriangle(triangles, faceIndex + 26, faceIndex + 25, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 19, faceIndex + 20, faceIndex + 26);
            AddTriangle(triangles, faceIndex + 27, faceIndex + 26, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 21, faceIndex + 22, faceIndex + 28);
            AddTriangle(triangles, faceIndex + 29, faceIndex + 28, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 22, faceIndex + 23, faceIndex + 29);
            AddTriangle(triangles, faceIndex + 30, faceIndex + 29, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 23, faceIndex + 24, faceIndex + 30);
            AddTriangle(triangles, faceIndex + 31, faceIndex + 30, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 24, faceIndex + 25, faceIndex + 31);
            AddTriangle(triangles, faceIndex + 32, faceIndex + 31, faceIndex + 25);
            AddTriangle(triangles, faceIndex + 25, faceIndex + 26, faceIndex + 32);
            AddTriangle(triangles, faceIndex + 33, faceIndex + 32, faceIndex + 26);
            AddTriangle(triangles, faceIndex + 26, faceIndex + 27, faceIndex + 33);
            AddTriangle(triangles, faceIndex + 34, faceIndex + 33, faceIndex + 27);
            AddTriangle(triangles, faceIndex + 28, faceIndex + 29, faceIndex + 35);
            AddTriangle(triangles, faceIndex + 36, faceIndex + 35, faceIndex + 29);
            AddTriangle(triangles, faceIndex + 29, faceIndex + 30, faceIndex + 36);
            AddTriangle(triangles, faceIndex + 37, faceIndex + 36, faceIndex + 30);
            AddTriangle(triangles, faceIndex + 30, faceIndex + 31, faceIndex + 37);
            AddTriangle(triangles, faceIndex + 38, faceIndex + 37, faceIndex + 31);
            AddTriangle(triangles, faceIndex + 31, faceIndex + 32, faceIndex + 38);
            AddTriangle(triangles, faceIndex + 39, faceIndex + 38, faceIndex + 32);
            AddTriangle(triangles, faceIndex + 32, faceIndex + 33, faceIndex + 39);
            AddTriangle(triangles, faceIndex + 40, faceIndex + 39, faceIndex + 33);
            AddTriangle(triangles, faceIndex + 33, faceIndex + 34, faceIndex + 40);
            AddTriangle(triangles, faceIndex + 41, faceIndex + 40, faceIndex + 34);
            faceIndex += 43;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.241181, -0.500000, -0.465926));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.366025));
            vertices.Add(new Point3D(0.207107, -0.500000, -0.207107));
            vertices.Add(new Point3D(0.366025, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.465926, -0.500000, 0.241181));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Frontface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.465926, -0.241181, 0.500000));
            vertices.Add(new Point3D(0.366025, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.207107, 0.207107, 0.500000));
            vertices.Add(new Point3D(0.000000, 0.366025, 0.500000));
            vertices.Add(new Point3D(-0.241181, 0.465926, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            faceIndex += 8;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.241181, -0.465926));
            vertices.Add(new Point3D(-0.500000, -0.000000, -0.366025));
            vertices.Add(new Point3D(-0.500000, 0.207107, -0.207107));
            vertices.Add(new Point3D(-0.500000, 0.366025, 0.000000));
            vertices.Add(new Point3D(-0.500000, 0.465926, 0.241181));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 2, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 3, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 4, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 5, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 6, faceIndex + 0);
            return faceIndex;
        }
        private int AddGeometry_RoundSlope(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Sideface
            vertices.Add(new Point3D(0.500000, 0.465926, 0.241181));
            vertices.Add(new Point3D(0.500000, 0.366025, -0.000000));
            vertices.Add(new Point3D(0.500000, 0.207107, -0.207107));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.366025));
            vertices.Add(new Point3D(0.500000, -0.241181, -0.465926));
            vertices.Add(new Point3D(-0.500000, 0.465926, 0.241181));
            vertices.Add(new Point3D(-0.500000, 0.366025, 0.000000));
            vertices.Add(new Point3D(-0.500000, 0.207107, -0.207107));
            vertices.Add(new Point3D(-0.500000, 0.000000, -0.366025));
            vertices.Add(new Point3D(-0.500000, -0.241181, -0.465926));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(-0.000000, 0.976344, -0.216222));
            normals.Add(new Vector3D(-0.000000, 0.887114, -0.461551));
            normals.Add(new Vector3D(-0.000000, 0.737428, -0.675426));
            normals.Add(new Vector3D(-0.000000, 0.537487, -0.843272));
            normals.Add(new Vector3D(-0.000000, 0.300918, -0.953650));
            normals.Add(new Vector3D(-0.000000, 0.953650, -0.300918));
            normals.Add(new Vector3D(-0.000000, 0.843272, -0.537487));
            normals.Add(new Vector3D(-0.000000, 0.675426, -0.737428));
            normals.Add(new Vector3D(-0.000000, 0.461551, -0.887114));
            normals.Add(new Vector3D(-0.000000, 0.216222, -0.976344));
            normals.Add(new Vector3D(-0.000000, 0.130526, -0.991445));
            normals.Add(new Vector3D(-0.000000, 0.130526, -0.991445));
            normals.Add(new Vector3D(-0.000000, 0.991445, -0.130526));
            normals.Add(new Vector3D(-0.000000, 0.991445, -0.130526));
            AddTriangle(triangles, faceIndex + 13, faceIndex + 0, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 12, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 5, faceIndex + 1);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 6, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 7, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 8, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 11, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 9, faceIndex + 11);
            faceIndex += 14;
            // Rightface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.465926, 0.241181));
            vertices.Add(new Point3D(-0.500000, 0.366025, 0.000000));
            vertices.Add(new Point3D(-0.500000, 0.207107, -0.207107));
            vertices.Add(new Point3D(-0.500000, 0.000000, -0.366025));
            vertices.Add(new Point3D(-0.500000, -0.241181, -0.465926));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 7, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 5, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 6, faceIndex + 0);
            faceIndex += 8;
            // Leftface
            vertices.Add(new Point3D(0.500000, 0.465926, 0.241181));
            vertices.Add(new Point3D(0.500000, 0.366025, -0.000000));
            vertices.Add(new Point3D(0.500000, 0.207107, -0.207107));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.366025));
            vertices.Add(new Point3D(0.500000, -0.241181, -0.465926));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, -0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 7, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 2, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 3, faceIndex + 5);
            faceIndex += 8;
            // Frontface
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, 0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 4, faceIndex + 3);
            faceIndex += 6;
            // Bottomface
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 5);
            return faceIndex;
        }
        private int AddGeometry_Cylinder(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Sideface
            vertices.Add(new Point3D(0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(0.482963, 0.500000, -0.129410));
            vertices.Add(new Point3D(0.433013, 0.500000, -0.250000));
            vertices.Add(new Point3D(0.353553, 0.500000, -0.353553));
            vertices.Add(new Point3D(0.250000, 0.500000, -0.433013));
            vertices.Add(new Point3D(0.129410, 0.500000, -0.482963));
            vertices.Add(new Point3D(-0.129410, 0.500000, -0.482963));
            vertices.Add(new Point3D(-0.250000, 0.500000, -0.433013));
            vertices.Add(new Point3D(-0.353553, 0.500000, -0.353553));
            vertices.Add(new Point3D(-0.433013, 0.500000, -0.250000));
            vertices.Add(new Point3D(-0.482963, 0.500000, -0.129410));
            vertices.Add(new Point3D(-0.482963, 0.500000, 0.129410));
            vertices.Add(new Point3D(-0.433013, 0.500000, 0.250000));
            vertices.Add(new Point3D(-0.353553, 0.500000, 0.353553));
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.433013));
            vertices.Add(new Point3D(-0.129410, 0.500000, 0.482963));
            vertices.Add(new Point3D(0.129410, 0.500000, 0.482963));
            vertices.Add(new Point3D(0.250000, 0.500000, 0.433013));
            vertices.Add(new Point3D(0.353553, 0.500000, 0.353553));
            vertices.Add(new Point3D(0.433013, 0.500000, 0.250000));
            vertices.Add(new Point3D(0.482963, 0.500000, 0.129410));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.500000));
            vertices.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            normals.Add(new Vector3D(0.976344, 0.000000, -0.216222));
            normals.Add(new Vector3D(0.887114, 0.000000, -0.461551));
            normals.Add(new Vector3D(0.737428, 0.000000, -0.675426));
            normals.Add(new Vector3D(0.537487, 0.000000, -0.843272));
            normals.Add(new Vector3D(0.300918, 0.000000, -0.953650));
            normals.Add(new Vector3D(-0.216222, 0.000000, -0.976344));
            normals.Add(new Vector3D(-0.461551, 0.000000, -0.887114));
            normals.Add(new Vector3D(-0.675426, 0.000000, -0.737428));
            normals.Add(new Vector3D(-0.843272, 0.000000, -0.537487));
            normals.Add(new Vector3D(-0.953650, 0.000000, -0.300918));
            normals.Add(new Vector3D(-0.976344, -0.000000, 0.216222));
            normals.Add(new Vector3D(-0.887114, -0.000000, 0.461551));
            normals.Add(new Vector3D(-0.737428, -0.000000, 0.675426));
            normals.Add(new Vector3D(-0.537487, -0.000000, 0.843272));
            normals.Add(new Vector3D(-0.300918, -0.000000, 0.953650));
            normals.Add(new Vector3D(0.216222, -0.000000, 0.976344));
            normals.Add(new Vector3D(0.461551, -0.000000, 0.887114));
            normals.Add(new Vector3D(0.675426, -0.000000, 0.737428));
            normals.Add(new Vector3D(0.843272, -0.000000, 0.537487));
            normals.Add(new Vector3D(0.953650, -0.000000, 0.300918));
            normals.Add(new Vector3D(0.953650, 0.000000, -0.300918));
            normals.Add(new Vector3D(0.843272, 0.000000, -0.537487));
            normals.Add(new Vector3D(0.675426, 0.000000, -0.737428));
            normals.Add(new Vector3D(0.461551, 0.000000, -0.887114));
            normals.Add(new Vector3D(0.216222, 0.000000, -0.976344));
            normals.Add(new Vector3D(-0.300918, 0.000000, -0.953650));
            normals.Add(new Vector3D(-0.537487, 0.000000, -0.843272));
            normals.Add(new Vector3D(-0.737428, 0.000000, -0.675426));
            normals.Add(new Vector3D(-0.887114, 0.000000, -0.461551));
            normals.Add(new Vector3D(-0.976344, 0.000000, -0.216222));
            normals.Add(new Vector3D(-0.953650, -0.000000, 0.300918));
            normals.Add(new Vector3D(-0.843272, -0.000000, 0.537487));
            normals.Add(new Vector3D(-0.675426, -0.000000, 0.737428));
            normals.Add(new Vector3D(-0.461551, -0.000000, 0.887114));
            normals.Add(new Vector3D(-0.216222, -0.000000, 0.976344));
            normals.Add(new Vector3D(0.300918, -0.000000, 0.953650));
            normals.Add(new Vector3D(0.537487, -0.000000, 0.843272));
            normals.Add(new Vector3D(0.737428, -0.000000, 0.675426));
            normals.Add(new Vector3D(0.887114, -0.000000, 0.461551));
            normals.Add(new Vector3D(0.976344, -0.000000, 0.216222));
            normals.Add(new Vector3D(-0.043842, -0.000000, 0.999038));
            normals.Add(new Vector3D(0.043842, -0.000000, 0.999038));
            normals.Add(new Vector3D(-0.043842, 0.000000, -0.999038));
            normals.Add(new Vector3D(0.043842, 0.000000, -0.999038));
            normals.Add(new Vector3D(-0.999038, -0.000000, 0.043842));
            normals.Add(new Vector3D(-0.999038, 0.000000, -0.043842));
            normals.Add(new Vector3D(0.999038, 0.000000, -0.043842));
            normals.Add(new Vector3D(0.999038, -0.000000, 0.043842));
            AddTriangle(triangles, faceIndex + 47, faceIndex + 0, faceIndex + 46);
            AddTriangle(triangles, faceIndex + 20, faceIndex + 46, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 21, faceIndex + 20, faceIndex + 1);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 21);
            AddTriangle(triangles, faceIndex + 22, faceIndex + 21, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 23, faceIndex + 22, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 24, faceIndex + 23, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 43, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 42, faceIndex + 24, faceIndex + 43);
            AddTriangle(triangles, faceIndex + 43, faceIndex + 5, faceIndex + 42);
            AddTriangle(triangles, faceIndex + 25, faceIndex + 42, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 6, faceIndex + 25);
            AddTriangle(triangles, faceIndex + 26, faceIndex + 25, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 7, faceIndex + 26);
            AddTriangle(triangles, faceIndex + 27, faceIndex + 26, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 8, faceIndex + 27);
            AddTriangle(triangles, faceIndex + 28, faceIndex + 27, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 9, faceIndex + 28);
            AddTriangle(triangles, faceIndex + 29, faceIndex + 28, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 45, faceIndex + 29);
            AddTriangle(triangles, faceIndex + 44, faceIndex + 29, faceIndex + 45);
            AddTriangle(triangles, faceIndex + 45, faceIndex + 10, faceIndex + 44);
            AddTriangle(triangles, faceIndex + 30, faceIndex + 44, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 11, faceIndex + 30);
            AddTriangle(triangles, faceIndex + 31, faceIndex + 30, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 11, faceIndex + 12, faceIndex + 31);
            AddTriangle(triangles, faceIndex + 32, faceIndex + 31, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 12, faceIndex + 13, faceIndex + 32);
            AddTriangle(triangles, faceIndex + 33, faceIndex + 32, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 13, faceIndex + 14, faceIndex + 33);
            AddTriangle(triangles, faceIndex + 34, faceIndex + 33, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 14, faceIndex + 40, faceIndex + 34);
            AddTriangle(triangles, faceIndex + 41, faceIndex + 34, faceIndex + 40);
            AddTriangle(triangles, faceIndex + 40, faceIndex + 15, faceIndex + 41);
            AddTriangle(triangles, faceIndex + 35, faceIndex + 41, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 15, faceIndex + 16, faceIndex + 35);
            AddTriangle(triangles, faceIndex + 36, faceIndex + 35, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 16, faceIndex + 17, faceIndex + 36);
            AddTriangle(triangles, faceIndex + 37, faceIndex + 36, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 17, faceIndex + 18, faceIndex + 37);
            AddTriangle(triangles, faceIndex + 38, faceIndex + 37, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 18, faceIndex + 19, faceIndex + 38);
            AddTriangle(triangles, faceIndex + 39, faceIndex + 38, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 19, faceIndex + 47, faceIndex + 39);
            AddTriangle(triangles, faceIndex + 46, faceIndex + 39, faceIndex + 47);
            faceIndex += 48;
            // Topface
            vertices.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            vertices.Add(new Point3D(0.482963, 0.500000, -0.129410));
            vertices.Add(new Point3D(0.433013, 0.500000, -0.250000));
            vertices.Add(new Point3D(0.353553, 0.500000, -0.353553));
            vertices.Add(new Point3D(0.250000, 0.500000, -0.433013));
            vertices.Add(new Point3D(0.129410, 0.500000, -0.482963));
            vertices.Add(new Point3D(-0.129410, 0.500000, -0.482963));
            vertices.Add(new Point3D(-0.250000, 0.500000, -0.433013));
            vertices.Add(new Point3D(-0.353553, 0.500000, -0.353553));
            vertices.Add(new Point3D(-0.433013, 0.500000, -0.250000));
            vertices.Add(new Point3D(-0.482963, 0.500000, -0.129410));
            vertices.Add(new Point3D(-0.482963, 0.500000, 0.129410));
            vertices.Add(new Point3D(-0.433013, 0.500000, 0.250000));
            vertices.Add(new Point3D(-0.353553, 0.500000, 0.353553));
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.433013));
            vertices.Add(new Point3D(-0.129410, 0.500000, 0.482963));
            vertices.Add(new Point3D(0.129410, 0.500000, 0.482963));
            vertices.Add(new Point3D(0.250000, 0.500000, 0.433013));
            vertices.Add(new Point3D(0.353553, 0.500000, 0.353553));
            vertices.Add(new Point3D(0.433013, 0.500000, 0.250000));
            vertices.Add(new Point3D(0.482963, 0.500000, 0.129410));
            vertices.Add(new Point3D(-0.000000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, 0.000000));
            vertices.Add(new Point3D(0.500000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 23, faceIndex + 0, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 23, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 15, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 21, faceIndex + 6, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 7, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 8, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 9, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 10, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 22, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 22, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 24, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 5, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 21, faceIndex + 0);
            faceIndex += 25;
            // Bottomface
            vertices.Add(new Point3D(0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 19, faceIndex + 20, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 21, faceIndex + 9, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 22, faceIndex + 14, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 24, faceIndex + 4, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 24, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 5, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 6, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 7, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 8, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 21, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 11, faceIndex + 10, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 12, faceIndex + 11, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 13, faceIndex + 12, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 14, faceIndex + 13, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 15, faceIndex + 22, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 16, faceIndex + 15, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 17, faceIndex + 16, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 18, faceIndex + 17, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 19, faceIndex + 18, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 23, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 2, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 3, faceIndex + 20);
            return faceIndex;
        }
        private int AddGeometry_CylinderEndA(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Topface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(0.482963, -0.370590, -0.000000));
            vertices.Add(new Point3D(0.466506, -0.370590, -0.125000));
            vertices.Add(new Point3D(0.418258, -0.370590, -0.241481));
            vertices.Add(new Point3D(0.341506, -0.370590, -0.341506));
            vertices.Add(new Point3D(0.241481, -0.370590, -0.418258));
            vertices.Add(new Point3D(0.125000, -0.370590, -0.466506));
            vertices.Add(new Point3D(-0.000000, -0.370590, -0.482963));
            vertices.Add(new Point3D(-0.125000, -0.370590, -0.466506));
            vertices.Add(new Point3D(-0.241481, -0.370590, -0.418258));
            vertices.Add(new Point3D(-0.341506, -0.370590, -0.341506));
            vertices.Add(new Point3D(-0.418258, -0.370590, -0.241481));
            vertices.Add(new Point3D(-0.466506, -0.370590, -0.125000));
            vertices.Add(new Point3D(-0.482963, -0.370590, 0.000000));
            vertices.Add(new Point3D(-0.466506, -0.370590, 0.125000));
            vertices.Add(new Point3D(-0.418258, -0.370590, 0.241481));
            vertices.Add(new Point3D(-0.341506, -0.370590, 0.341506));
            vertices.Add(new Point3D(-0.241481, -0.370590, 0.418258));
            vertices.Add(new Point3D(-0.125000, -0.370590, 0.466506));
            vertices.Add(new Point3D(0.000000, -0.370590, 0.482963));
            vertices.Add(new Point3D(0.125000, -0.370590, 0.466506));
            vertices.Add(new Point3D(0.241481, -0.370590, 0.418258));
            vertices.Add(new Point3D(0.341506, -0.370590, 0.341506));
            vertices.Add(new Point3D(0.418258, -0.370590, 0.241481));
            vertices.Add(new Point3D(0.466506, -0.370590, 0.125000));
            vertices.Add(new Point3D(0.433013, -0.250000, -0.000000));
            vertices.Add(new Point3D(0.418258, -0.250000, -0.112072));
            vertices.Add(new Point3D(0.375000, -0.250000, -0.216506));
            vertices.Add(new Point3D(0.306186, -0.250000, -0.306186));
            vertices.Add(new Point3D(0.216506, -0.250000, -0.375000));
            vertices.Add(new Point3D(0.112072, -0.250000, -0.418258));
            vertices.Add(new Point3D(-0.000000, -0.250000, -0.433013));
            vertices.Add(new Point3D(-0.112072, -0.250000, -0.418258));
            vertices.Add(new Point3D(-0.216506, -0.250000, -0.375000));
            vertices.Add(new Point3D(-0.306186, -0.250000, -0.306186));
            vertices.Add(new Point3D(-0.375000, -0.250000, -0.216506));
            vertices.Add(new Point3D(-0.418258, -0.250000, -0.112072));
            vertices.Add(new Point3D(-0.433013, -0.250000, 0.000000));
            vertices.Add(new Point3D(-0.418258, -0.250000, 0.112072));
            vertices.Add(new Point3D(-0.375000, -0.250000, 0.216506));
            vertices.Add(new Point3D(-0.306186, -0.250000, 0.306186));
            vertices.Add(new Point3D(-0.216506, -0.250000, 0.375000));
            vertices.Add(new Point3D(-0.112072, -0.250000, 0.418258));
            vertices.Add(new Point3D(0.000000, -0.250000, 0.433013));
            vertices.Add(new Point3D(0.112072, -0.250000, 0.418258));
            vertices.Add(new Point3D(0.216506, -0.250000, 0.375000));
            vertices.Add(new Point3D(0.306186, -0.250000, 0.306186));
            vertices.Add(new Point3D(0.375000, -0.250000, 0.216506));
            vertices.Add(new Point3D(0.418258, -0.250000, 0.112072));
            vertices.Add(new Point3D(0.353553, -0.146447, -0.000000));
            vertices.Add(new Point3D(0.341506, -0.146447, -0.091506));
            vertices.Add(new Point3D(0.306186, -0.146447, -0.176777));
            vertices.Add(new Point3D(0.250000, -0.146447, -0.250000));
            vertices.Add(new Point3D(0.176777, -0.146447, -0.306186));
            vertices.Add(new Point3D(0.091506, -0.146447, -0.341506));
            vertices.Add(new Point3D(-0.000000, -0.146447, -0.353553));
            vertices.Add(new Point3D(-0.091506, -0.146447, -0.341506));
            vertices.Add(new Point3D(-0.176777, -0.146447, -0.306186));
            vertices.Add(new Point3D(-0.250000, -0.146447, -0.250000));
            vertices.Add(new Point3D(-0.306186, -0.146447, -0.176777));
            vertices.Add(new Point3D(-0.341506, -0.146447, -0.091506));
            vertices.Add(new Point3D(-0.353553, -0.146447, 0.000000));
            vertices.Add(new Point3D(-0.341506, -0.146447, 0.091506));
            vertices.Add(new Point3D(-0.306186, -0.146447, 0.176777));
            vertices.Add(new Point3D(-0.250000, -0.146447, 0.250000));
            vertices.Add(new Point3D(-0.176777, -0.146447, 0.306186));
            vertices.Add(new Point3D(-0.091506, -0.146447, 0.341506));
            vertices.Add(new Point3D(0.000000, -0.146447, 0.353553));
            vertices.Add(new Point3D(0.091506, -0.146447, 0.341506));
            vertices.Add(new Point3D(0.176777, -0.146447, 0.306186));
            vertices.Add(new Point3D(0.250000, -0.146447, 0.250000));
            vertices.Add(new Point3D(0.306186, -0.146447, 0.176777));
            vertices.Add(new Point3D(0.341506, -0.146447, 0.091506));
            vertices.Add(new Point3D(0.250000, -0.066987, -0.000000));
            vertices.Add(new Point3D(0.241481, -0.066987, -0.064705));
            vertices.Add(new Point3D(0.216506, -0.066987, -0.125000));
            vertices.Add(new Point3D(0.176777, -0.066987, -0.176777));
            vertices.Add(new Point3D(0.125000, -0.066987, -0.216506));
            vertices.Add(new Point3D(0.064705, -0.066987, -0.241481));
            vertices.Add(new Point3D(-0.000000, -0.066987, -0.250000));
            vertices.Add(new Point3D(-0.064705, -0.066987, -0.241481));
            vertices.Add(new Point3D(-0.125000, -0.066987, -0.216506));
            vertices.Add(new Point3D(-0.176777, -0.066987, -0.176777));
            vertices.Add(new Point3D(-0.216506, -0.066987, -0.125000));
            vertices.Add(new Point3D(-0.241481, -0.066987, -0.064705));
            vertices.Add(new Point3D(-0.250000, -0.066987, 0.000000));
            vertices.Add(new Point3D(-0.241481, -0.066987, 0.064705));
            vertices.Add(new Point3D(-0.216506, -0.066987, 0.125000));
            vertices.Add(new Point3D(-0.176777, -0.066987, 0.176777));
            vertices.Add(new Point3D(-0.125000, -0.066987, 0.216506));
            vertices.Add(new Point3D(-0.064705, -0.066987, 0.241481));
            vertices.Add(new Point3D(0.000000, -0.066987, 0.250000));
            vertices.Add(new Point3D(0.064705, -0.066987, 0.241481));
            vertices.Add(new Point3D(0.125000, -0.066987, 0.216506));
            vertices.Add(new Point3D(0.176777, -0.066987, 0.176777));
            vertices.Add(new Point3D(0.216506, -0.066987, 0.125000));
            vertices.Add(new Point3D(0.241481, -0.066987, 0.064705));
            vertices.Add(new Point3D(0.129410, -0.017037, -0.000000));
            vertices.Add(new Point3D(0.125000, -0.017037, -0.033494));
            vertices.Add(new Point3D(0.112072, -0.017037, -0.064705));
            vertices.Add(new Point3D(0.091506, -0.017037, -0.091506));
            vertices.Add(new Point3D(0.064705, -0.017037, -0.112072));
            vertices.Add(new Point3D(0.033494, -0.017037, -0.125000));
            vertices.Add(new Point3D(-0.000000, -0.017037, -0.129410));
            vertices.Add(new Point3D(-0.033494, -0.017037, -0.125000));
            vertices.Add(new Point3D(-0.064705, -0.017037, -0.112072));
            vertices.Add(new Point3D(-0.091506, -0.017037, -0.091506));
            vertices.Add(new Point3D(-0.112072, -0.017037, -0.064705));
            vertices.Add(new Point3D(-0.125000, -0.017037, -0.033494));
            vertices.Add(new Point3D(-0.129410, -0.017037, 0.000000));
            vertices.Add(new Point3D(-0.125000, -0.017037, 0.033494));
            vertices.Add(new Point3D(-0.112072, -0.017037, 0.064705));
            vertices.Add(new Point3D(-0.091506, -0.017037, 0.091506));
            vertices.Add(new Point3D(-0.064705, -0.017037, 0.112072));
            vertices.Add(new Point3D(-0.033494, -0.017037, 0.125000));
            vertices.Add(new Point3D(0.000000, -0.017037, 0.129410));
            vertices.Add(new Point3D(0.033494, -0.017037, 0.125000));
            vertices.Add(new Point3D(0.064705, -0.017037, 0.112072));
            vertices.Add(new Point3D(0.091506, -0.017037, 0.091506));
            vertices.Add(new Point3D(0.112072, -0.017037, 0.064705));
            vertices.Add(new Point3D(0.125000, -0.017037, 0.033494));
            vertices.Add(new Point3D(-0.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(0.990508, 0.130403, 0.043468));
            normals.Add(new Vector3D(0.968007, 0.130403, -0.214376));
            normals.Add(new Vector3D(0.879539, 0.130403, -0.457610));
            normals.Add(new Vector3D(0.731131, 0.130403, -0.669659));
            normals.Add(new Vector3D(0.532898, 0.130403, -0.836071));
            normals.Add(new Vector3D(0.298349, 0.130403, -0.945507));
            normals.Add(new Vector3D(0.043468, 0.130403, -0.990508));
            normals.Add(new Vector3D(-0.214376, 0.130403, -0.968007));
            normals.Add(new Vector3D(-0.457610, 0.130403, -0.879539));
            normals.Add(new Vector3D(-0.669659, 0.130403, -0.731131));
            normals.Add(new Vector3D(-0.836071, 0.130403, -0.532898));
            normals.Add(new Vector3D(-0.945507, 0.130403, -0.298349));
            normals.Add(new Vector3D(-0.990508, 0.130403, -0.043468));
            normals.Add(new Vector3D(-0.968007, 0.130403, 0.214376));
            normals.Add(new Vector3D(-0.879539, 0.130403, 0.457610));
            normals.Add(new Vector3D(-0.731131, 0.130403, 0.669659));
            normals.Add(new Vector3D(-0.532898, 0.130403, 0.836071));
            normals.Add(new Vector3D(-0.298349, 0.130403, 0.945507));
            normals.Add(new Vector3D(-0.043468, 0.130403, 0.990508));
            normals.Add(new Vector3D(0.214376, 0.130403, 0.968007));
            normals.Add(new Vector3D(0.457610, 0.130403, 0.879539));
            normals.Add(new Vector3D(0.669659, 0.130403, 0.731131));
            normals.Add(new Vector3D(0.836071, 0.130403, 0.532898));
            normals.Add(new Vector3D(0.945507, 0.130403, 0.298349));
            normals.Add(new Vector3D(0.965906, 0.258889, -0.001472));
            normals.Add(new Vector3D(0.932613, 0.258889, -0.251417));
            normals.Add(new Vector3D(0.835763, 0.258889, -0.484228));
            normals.Add(new Vector3D(0.681958, 0.258889, -0.684039));
            normals.Add(new Vector3D(0.481678, 0.258889, -0.837235));
            normals.Add(new Vector3D(0.248573, 0.258889, -0.933374));
            normals.Add(new Vector3D(-0.001472, 0.258889, -0.965906));
            normals.Add(new Vector3D(-0.251417, 0.258889, -0.932613));
            normals.Add(new Vector3D(-0.484228, 0.258889, -0.835763));
            normals.Add(new Vector3D(-0.684039, 0.258889, -0.681958));
            normals.Add(new Vector3D(-0.837235, 0.258889, -0.481678));
            normals.Add(new Vector3D(-0.933374, 0.258889, -0.248573));
            normals.Add(new Vector3D(-0.965906, 0.258889, 0.001472));
            normals.Add(new Vector3D(-0.932613, 0.258889, 0.251417));
            normals.Add(new Vector3D(-0.835763, 0.258889, 0.484228));
            normals.Add(new Vector3D(-0.681958, 0.258889, 0.684039));
            normals.Add(new Vector3D(-0.481678, 0.258889, 0.837235));
            normals.Add(new Vector3D(-0.248573, 0.258889, 0.933374));
            normals.Add(new Vector3D(0.001472, 0.258889, 0.965906));
            normals.Add(new Vector3D(0.251417, 0.258889, 0.932613));
            normals.Add(new Vector3D(0.484228, 0.258889, 0.835763));
            normals.Add(new Vector3D(0.684039, 0.258889, 0.681958));
            normals.Add(new Vector3D(0.837235, 0.258889, 0.481678));
            normals.Add(new Vector3D(0.933374, 0.258889, 0.248573));
            normals.Add(new Vector3D(0.865959, 0.500107, -0.002852));
            normals.Add(new Vector3D(0.835714, 0.500107, -0.226882));
            normals.Add(new Vector3D(0.748516, 0.500107, -0.435450));
            normals.Add(new Vector3D(0.610308, 0.500107, -0.614342));
            normals.Add(new Vector3D(0.430509, 0.500107, -0.751368));
            normals.Add(new Vector3D(0.221372, 0.500107, -0.837190));
            normals.Add(new Vector3D(-0.002852, 0.500107, -0.865959));
            normals.Add(new Vector3D(-0.226882, 0.500107, -0.835714));
            normals.Add(new Vector3D(-0.435450, 0.500107, -0.748516));
            normals.Add(new Vector3D(-0.614342, 0.500107, -0.610308));
            normals.Add(new Vector3D(-0.751368, 0.500107, -0.430509));
            normals.Add(new Vector3D(-0.837190, 0.500107, -0.221372));
            normals.Add(new Vector3D(-0.865959, 0.500107, 0.002852));
            normals.Add(new Vector3D(-0.835714, 0.500107, 0.226882));
            normals.Add(new Vector3D(-0.748516, 0.500107, 0.435450));
            normals.Add(new Vector3D(-0.610308, 0.500107, 0.614342));
            normals.Add(new Vector3D(-0.430509, 0.500107, 0.751368));
            normals.Add(new Vector3D(-0.221372, 0.500107, 0.837190));
            normals.Add(new Vector3D(0.002852, 0.500107, 0.865959));
            normals.Add(new Vector3D(0.226882, 0.500107, 0.835714));
            normals.Add(new Vector3D(0.435450, 0.500107, 0.748516));
            normals.Add(new Vector3D(0.614342, 0.500107, 0.610308));
            normals.Add(new Vector3D(0.751368, 0.500107, 0.430509));
            normals.Add(new Vector3D(0.837190, 0.500107, 0.221372));
            normals.Add(new Vector3D(0.706997, 0.707204, -0.004051));
            normals.Add(new Vector3D(0.681859, 0.707204, -0.186897));
            normals.Add(new Vector3D(0.610252, 0.707204, -0.357007));
            normals.Add(new Vector3D(0.497058, 0.707204, -0.502787));
            normals.Add(new Vector3D(0.349991, 0.707204, -0.614303));
            normals.Add(new Vector3D(0.179072, 0.707204, -0.683956));
            normals.Add(new Vector3D(-0.004051, 0.707204, -0.706997));
            normals.Add(new Vector3D(-0.186897, 0.707204, -0.681859));
            normals.Add(new Vector3D(-0.357007, 0.707204, -0.610252));
            normals.Add(new Vector3D(-0.502787, 0.707204, -0.497058));
            normals.Add(new Vector3D(-0.614303, 0.707204, -0.349991));
            normals.Add(new Vector3D(-0.683956, 0.707204, -0.179072));
            normals.Add(new Vector3D(-0.706997, 0.707204, 0.004051));
            normals.Add(new Vector3D(-0.681859, 0.707204, 0.186897));
            normals.Add(new Vector3D(-0.610252, 0.707204, 0.357007));
            normals.Add(new Vector3D(-0.497058, 0.707204, 0.502787));
            normals.Add(new Vector3D(-0.349991, 0.707204, 0.614303));
            normals.Add(new Vector3D(-0.179072, 0.707204, 0.683956));
            normals.Add(new Vector3D(0.004051, 0.707204, 0.706997));
            normals.Add(new Vector3D(0.186897, 0.707204, 0.681859));
            normals.Add(new Vector3D(0.357007, 0.707204, 0.610252));
            normals.Add(new Vector3D(0.502787, 0.707204, 0.497058));
            normals.Add(new Vector3D(0.614303, 0.707204, 0.349991));
            normals.Add(new Vector3D(0.683956, 0.707204, 0.179072));
            normals.Add(new Vector3D(0.499884, 0.866078, -0.004982));
            normals.Add(new Vector3D(0.481561, 0.866078, -0.134192));
            normals.Add(new Vector3D(0.430421, 0.866078, -0.254256));
            normals.Add(new Vector3D(0.349948, 0.866078, -0.356994));
            normals.Add(new Vector3D(0.245627, 0.866078, -0.435403));
            normals.Add(new Vector3D(0.124567, 0.866078, -0.484140));
            normals.Add(new Vector3D(-0.004982, 0.866078, -0.499884));
            normals.Add(new Vector3D(-0.134192, 0.866078, -0.481561));
            normals.Add(new Vector3D(-0.254256, 0.866078, -0.430421));
            normals.Add(new Vector3D(-0.356994, 0.866078, -0.349948));
            normals.Add(new Vector3D(-0.435403, 0.866078, -0.245627));
            normals.Add(new Vector3D(-0.484140, 0.866078, -0.124567));
            normals.Add(new Vector3D(-0.499884, 0.866078, 0.004982));
            normals.Add(new Vector3D(-0.481561, 0.866078, 0.134192));
            normals.Add(new Vector3D(-0.430421, 0.866078, 0.254256));
            normals.Add(new Vector3D(-0.349948, 0.866078, 0.356994));
            normals.Add(new Vector3D(-0.245627, 0.866078, 0.435403));
            normals.Add(new Vector3D(-0.124567, 0.866078, 0.484140));
            normals.Add(new Vector3D(0.004982, 0.866078, 0.499884));
            normals.Add(new Vector3D(0.134192, 0.866078, 0.481561));
            normals.Add(new Vector3D(0.254256, 0.866078, 0.430421));
            normals.Add(new Vector3D(0.356994, 0.866078, 0.349948));
            normals.Add(new Vector3D(0.435403, 0.866078, 0.245627));
            normals.Add(new Vector3D(0.484140, 0.866078, 0.124567));
            normals.Add(new Vector3D(0.284071, 0.958749, -0.010155));
            normals.Add(new Vector3D(0.271764, 0.958749, -0.083332));
            normals.Add(new Vector3D(0.240936, 0.958749, -0.150830));
            normals.Add(new Vector3D(0.193688, 0.958749, -0.208049));
            normals.Add(new Vector3D(0.133242, 0.958749, -0.251090));
            normals.Add(new Vector3D(0.063714, 0.958749, -0.277020));
            normals.Add(new Vector3D(-0.010155, 0.958749, -0.284071));
            normals.Add(new Vector3D(-0.083332, 0.958749, -0.271764));
            normals.Add(new Vector3D(-0.150830, 0.958749, -0.240936));
            normals.Add(new Vector3D(-0.208049, 0.958749, -0.193688));
            normals.Add(new Vector3D(-0.251090, 0.958749, -0.133242));
            normals.Add(new Vector3D(-0.277020, 0.958749, -0.063714));
            normals.Add(new Vector3D(-0.284071, 0.958749, 0.010155));
            normals.Add(new Vector3D(-0.271764, 0.958749, 0.083332));
            normals.Add(new Vector3D(-0.240936, 0.958749, 0.150830));
            normals.Add(new Vector3D(-0.193688, 0.958749, 0.208049));
            normals.Add(new Vector3D(-0.133242, 0.958749, 0.251090));
            normals.Add(new Vector3D(-0.063714, 0.958749, 0.277020));
            normals.Add(new Vector3D(0.010155, 0.958749, 0.284071));
            normals.Add(new Vector3D(0.083332, 0.958749, 0.271764));
            normals.Add(new Vector3D(0.150830, 0.958749, 0.240936));
            normals.Add(new Vector3D(0.208049, 0.958749, 0.193688));
            normals.Add(new Vector3D(0.251090, 0.958749, 0.133242));
            normals.Add(new Vector3D(0.277020, 0.958749, 0.063714));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 144, faceIndex + 142, faceIndex + 143);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 141, faceIndex + 142);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 140, faceIndex + 141);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 139, faceIndex + 140);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 138, faceIndex + 139);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 137, faceIndex + 138);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 136, faceIndex + 137);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 135, faceIndex + 136);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 134, faceIndex + 135);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 133, faceIndex + 134);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 132, faceIndex + 133);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 131, faceIndex + 132);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 130, faceIndex + 131);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 129, faceIndex + 130);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 128, faceIndex + 129);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 127, faceIndex + 128);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 126, faceIndex + 127);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 125, faceIndex + 126);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 124, faceIndex + 125);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 123, faceIndex + 124);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 122, faceIndex + 123);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 121, faceIndex + 122);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 120, faceIndex + 121);
            AddTriangle(triangles, faceIndex + 144, faceIndex + 143, faceIndex + 120);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 25, faceIndex + 24, faceIndex + 1);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 25);
            AddTriangle(triangles, faceIndex + 26, faceIndex + 25, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 26);
            AddTriangle(triangles, faceIndex + 27, faceIndex + 26, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 27);
            AddTriangle(triangles, faceIndex + 28, faceIndex + 27, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 5, faceIndex + 28);
            AddTriangle(triangles, faceIndex + 29, faceIndex + 28, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 6, faceIndex + 29);
            AddTriangle(triangles, faceIndex + 30, faceIndex + 29, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 7, faceIndex + 30);
            AddTriangle(triangles, faceIndex + 31, faceIndex + 30, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 8, faceIndex + 31);
            AddTriangle(triangles, faceIndex + 32, faceIndex + 31, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 9, faceIndex + 32);
            AddTriangle(triangles, faceIndex + 33, faceIndex + 32, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 10, faceIndex + 33);
            AddTriangle(triangles, faceIndex + 34, faceIndex + 33, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 11, faceIndex + 34);
            AddTriangle(triangles, faceIndex + 35, faceIndex + 34, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 11, faceIndex + 12, faceIndex + 35);
            AddTriangle(triangles, faceIndex + 36, faceIndex + 35, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 12, faceIndex + 13, faceIndex + 36);
            AddTriangle(triangles, faceIndex + 37, faceIndex + 36, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 13, faceIndex + 14, faceIndex + 37);
            AddTriangle(triangles, faceIndex + 38, faceIndex + 37, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 14, faceIndex + 15, faceIndex + 38);
            AddTriangle(triangles, faceIndex + 39, faceIndex + 38, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 15, faceIndex + 16, faceIndex + 39);
            AddTriangle(triangles, faceIndex + 40, faceIndex + 39, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 16, faceIndex + 17, faceIndex + 40);
            AddTriangle(triangles, faceIndex + 41, faceIndex + 40, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 17, faceIndex + 18, faceIndex + 41);
            AddTriangle(triangles, faceIndex + 42, faceIndex + 41, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 18, faceIndex + 19, faceIndex + 42);
            AddTriangle(triangles, faceIndex + 43, faceIndex + 42, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 19, faceIndex + 20, faceIndex + 43);
            AddTriangle(triangles, faceIndex + 44, faceIndex + 43, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 20, faceIndex + 21, faceIndex + 44);
            AddTriangle(triangles, faceIndex + 45, faceIndex + 44, faceIndex + 21);
            AddTriangle(triangles, faceIndex + 21, faceIndex + 22, faceIndex + 45);
            AddTriangle(triangles, faceIndex + 46, faceIndex + 45, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 22, faceIndex + 23, faceIndex + 46);
            AddTriangle(triangles, faceIndex + 47, faceIndex + 46, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 23, faceIndex + 0, faceIndex + 47);
            AddTriangle(triangles, faceIndex + 24, faceIndex + 47, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 24, faceIndex + 25, faceIndex + 48);
            AddTriangle(triangles, faceIndex + 49, faceIndex + 48, faceIndex + 25);
            AddTriangle(triangles, faceIndex + 25, faceIndex + 26, faceIndex + 49);
            AddTriangle(triangles, faceIndex + 50, faceIndex + 49, faceIndex + 26);
            AddTriangle(triangles, faceIndex + 26, faceIndex + 27, faceIndex + 50);
            AddTriangle(triangles, faceIndex + 51, faceIndex + 50, faceIndex + 27);
            AddTriangle(triangles, faceIndex + 27, faceIndex + 28, faceIndex + 51);
            AddTriangle(triangles, faceIndex + 52, faceIndex + 51, faceIndex + 28);
            AddTriangle(triangles, faceIndex + 28, faceIndex + 29, faceIndex + 52);
            AddTriangle(triangles, faceIndex + 53, faceIndex + 52, faceIndex + 29);
            AddTriangle(triangles, faceIndex + 29, faceIndex + 30, faceIndex + 53);
            AddTriangle(triangles, faceIndex + 54, faceIndex + 53, faceIndex + 30);
            AddTriangle(triangles, faceIndex + 30, faceIndex + 31, faceIndex + 54);
            AddTriangle(triangles, faceIndex + 55, faceIndex + 54, faceIndex + 31);
            AddTriangle(triangles, faceIndex + 31, faceIndex + 32, faceIndex + 55);
            AddTriangle(triangles, faceIndex + 56, faceIndex + 55, faceIndex + 32);
            AddTriangle(triangles, faceIndex + 32, faceIndex + 33, faceIndex + 56);
            AddTriangle(triangles, faceIndex + 57, faceIndex + 56, faceIndex + 33);
            AddTriangle(triangles, faceIndex + 33, faceIndex + 34, faceIndex + 57);
            AddTriangle(triangles, faceIndex + 58, faceIndex + 57, faceIndex + 34);
            AddTriangle(triangles, faceIndex + 34, faceIndex + 35, faceIndex + 58);
            AddTriangle(triangles, faceIndex + 59, faceIndex + 58, faceIndex + 35);
            AddTriangle(triangles, faceIndex + 35, faceIndex + 36, faceIndex + 59);
            AddTriangle(triangles, faceIndex + 60, faceIndex + 59, faceIndex + 36);
            AddTriangle(triangles, faceIndex + 36, faceIndex + 37, faceIndex + 60);
            AddTriangle(triangles, faceIndex + 61, faceIndex + 60, faceIndex + 37);
            AddTriangle(triangles, faceIndex + 37, faceIndex + 38, faceIndex + 61);
            AddTriangle(triangles, faceIndex + 62, faceIndex + 61, faceIndex + 38);
            AddTriangle(triangles, faceIndex + 38, faceIndex + 39, faceIndex + 62);
            AddTriangle(triangles, faceIndex + 63, faceIndex + 62, faceIndex + 39);
            AddTriangle(triangles, faceIndex + 39, faceIndex + 40, faceIndex + 63);
            AddTriangle(triangles, faceIndex + 64, faceIndex + 63, faceIndex + 40);
            AddTriangle(triangles, faceIndex + 40, faceIndex + 41, faceIndex + 64);
            AddTriangle(triangles, faceIndex + 65, faceIndex + 64, faceIndex + 41);
            AddTriangle(triangles, faceIndex + 41, faceIndex + 42, faceIndex + 65);
            AddTriangle(triangles, faceIndex + 66, faceIndex + 65, faceIndex + 42);
            AddTriangle(triangles, faceIndex + 42, faceIndex + 43, faceIndex + 66);
            AddTriangle(triangles, faceIndex + 67, faceIndex + 66, faceIndex + 43);
            AddTriangle(triangles, faceIndex + 43, faceIndex + 44, faceIndex + 67);
            AddTriangle(triangles, faceIndex + 68, faceIndex + 67, faceIndex + 44);
            AddTriangle(triangles, faceIndex + 44, faceIndex + 45, faceIndex + 68);
            AddTriangle(triangles, faceIndex + 69, faceIndex + 68, faceIndex + 45);
            AddTriangle(triangles, faceIndex + 45, faceIndex + 46, faceIndex + 69);
            AddTriangle(triangles, faceIndex + 70, faceIndex + 69, faceIndex + 46);
            AddTriangle(triangles, faceIndex + 46, faceIndex + 47, faceIndex + 70);
            AddTriangle(triangles, faceIndex + 71, faceIndex + 70, faceIndex + 47);
            AddTriangle(triangles, faceIndex + 47, faceIndex + 24, faceIndex + 71);
            AddTriangle(triangles, faceIndex + 48, faceIndex + 71, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 48, faceIndex + 49, faceIndex + 72);
            AddTriangle(triangles, faceIndex + 73, faceIndex + 72, faceIndex + 49);
            AddTriangle(triangles, faceIndex + 49, faceIndex + 50, faceIndex + 73);
            AddTriangle(triangles, faceIndex + 74, faceIndex + 73, faceIndex + 50);
            AddTriangle(triangles, faceIndex + 50, faceIndex + 51, faceIndex + 74);
            AddTriangle(triangles, faceIndex + 75, faceIndex + 74, faceIndex + 51);
            AddTriangle(triangles, faceIndex + 51, faceIndex + 52, faceIndex + 75);
            AddTriangle(triangles, faceIndex + 76, faceIndex + 75, faceIndex + 52);
            AddTriangle(triangles, faceIndex + 52, faceIndex + 53, faceIndex + 76);
            AddTriangle(triangles, faceIndex + 77, faceIndex + 76, faceIndex + 53);
            AddTriangle(triangles, faceIndex + 53, faceIndex + 54, faceIndex + 77);
            AddTriangle(triangles, faceIndex + 78, faceIndex + 77, faceIndex + 54);
            AddTriangle(triangles, faceIndex + 54, faceIndex + 55, faceIndex + 78);
            AddTriangle(triangles, faceIndex + 79, faceIndex + 78, faceIndex + 55);
            AddTriangle(triangles, faceIndex + 55, faceIndex + 56, faceIndex + 79);
            AddTriangle(triangles, faceIndex + 80, faceIndex + 79, faceIndex + 56);
            AddTriangle(triangles, faceIndex + 56, faceIndex + 57, faceIndex + 80);
            AddTriangle(triangles, faceIndex + 81, faceIndex + 80, faceIndex + 57);
            AddTriangle(triangles, faceIndex + 57, faceIndex + 58, faceIndex + 81);
            AddTriangle(triangles, faceIndex + 82, faceIndex + 81, faceIndex + 58);
            AddTriangle(triangles, faceIndex + 58, faceIndex + 59, faceIndex + 82);
            AddTriangle(triangles, faceIndex + 83, faceIndex + 82, faceIndex + 59);
            AddTriangle(triangles, faceIndex + 59, faceIndex + 60, faceIndex + 83);
            AddTriangle(triangles, faceIndex + 84, faceIndex + 83, faceIndex + 60);
            AddTriangle(triangles, faceIndex + 60, faceIndex + 61, faceIndex + 84);
            AddTriangle(triangles, faceIndex + 85, faceIndex + 84, faceIndex + 61);
            AddTriangle(triangles, faceIndex + 61, faceIndex + 62, faceIndex + 85);
            AddTriangle(triangles, faceIndex + 86, faceIndex + 85, faceIndex + 62);
            AddTriangle(triangles, faceIndex + 62, faceIndex + 63, faceIndex + 86);
            AddTriangle(triangles, faceIndex + 87, faceIndex + 86, faceIndex + 63);
            AddTriangle(triangles, faceIndex + 63, faceIndex + 64, faceIndex + 87);
            AddTriangle(triangles, faceIndex + 88, faceIndex + 87, faceIndex + 64);
            AddTriangle(triangles, faceIndex + 64, faceIndex + 65, faceIndex + 88);
            AddTriangle(triangles, faceIndex + 89, faceIndex + 88, faceIndex + 65);
            AddTriangle(triangles, faceIndex + 65, faceIndex + 66, faceIndex + 89);
            AddTriangle(triangles, faceIndex + 90, faceIndex + 89, faceIndex + 66);
            AddTriangle(triangles, faceIndex + 66, faceIndex + 67, faceIndex + 90);
            AddTriangle(triangles, faceIndex + 91, faceIndex + 90, faceIndex + 67);
            AddTriangle(triangles, faceIndex + 67, faceIndex + 68, faceIndex + 91);
            AddTriangle(triangles, faceIndex + 92, faceIndex + 91, faceIndex + 68);
            AddTriangle(triangles, faceIndex + 68, faceIndex + 69, faceIndex + 92);
            AddTriangle(triangles, faceIndex + 93, faceIndex + 92, faceIndex + 69);
            AddTriangle(triangles, faceIndex + 69, faceIndex + 70, faceIndex + 93);
            AddTriangle(triangles, faceIndex + 94, faceIndex + 93, faceIndex + 70);
            AddTriangle(triangles, faceIndex + 70, faceIndex + 71, faceIndex + 94);
            AddTriangle(triangles, faceIndex + 95, faceIndex + 94, faceIndex + 71);
            AddTriangle(triangles, faceIndex + 71, faceIndex + 48, faceIndex + 95);
            AddTriangle(triangles, faceIndex + 72, faceIndex + 95, faceIndex + 48);
            AddTriangle(triangles, faceIndex + 72, faceIndex + 73, faceIndex + 96);
            AddTriangle(triangles, faceIndex + 97, faceIndex + 96, faceIndex + 73);
            AddTriangle(triangles, faceIndex + 73, faceIndex + 74, faceIndex + 97);
            AddTriangle(triangles, faceIndex + 98, faceIndex + 97, faceIndex + 74);
            AddTriangle(triangles, faceIndex + 74, faceIndex + 75, faceIndex + 98);
            AddTriangle(triangles, faceIndex + 99, faceIndex + 98, faceIndex + 75);
            AddTriangle(triangles, faceIndex + 75, faceIndex + 76, faceIndex + 99);
            AddTriangle(triangles, faceIndex + 100, faceIndex + 99, faceIndex + 76);
            AddTriangle(triangles, faceIndex + 76, faceIndex + 77, faceIndex + 100);
            AddTriangle(triangles, faceIndex + 101, faceIndex + 100, faceIndex + 77);
            AddTriangle(triangles, faceIndex + 77, faceIndex + 78, faceIndex + 101);
            AddTriangle(triangles, faceIndex + 102, faceIndex + 101, faceIndex + 78);
            AddTriangle(triangles, faceIndex + 78, faceIndex + 79, faceIndex + 102);
            AddTriangle(triangles, faceIndex + 103, faceIndex + 102, faceIndex + 79);
            AddTriangle(triangles, faceIndex + 79, faceIndex + 80, faceIndex + 103);
            AddTriangle(triangles, faceIndex + 104, faceIndex + 103, faceIndex + 80);
            AddTriangle(triangles, faceIndex + 80, faceIndex + 81, faceIndex + 104);
            AddTriangle(triangles, faceIndex + 105, faceIndex + 104, faceIndex + 81);
            AddTriangle(triangles, faceIndex + 81, faceIndex + 82, faceIndex + 105);
            AddTriangle(triangles, faceIndex + 106, faceIndex + 105, faceIndex + 82);
            AddTriangle(triangles, faceIndex + 82, faceIndex + 83, faceIndex + 106);
            AddTriangle(triangles, faceIndex + 107, faceIndex + 106, faceIndex + 83);
            AddTriangle(triangles, faceIndex + 83, faceIndex + 84, faceIndex + 107);
            AddTriangle(triangles, faceIndex + 108, faceIndex + 107, faceIndex + 84);
            AddTriangle(triangles, faceIndex + 84, faceIndex + 85, faceIndex + 108);
            AddTriangle(triangles, faceIndex + 109, faceIndex + 108, faceIndex + 85);
            AddTriangle(triangles, faceIndex + 85, faceIndex + 86, faceIndex + 109);
            AddTriangle(triangles, faceIndex + 110, faceIndex + 109, faceIndex + 86);
            AddTriangle(triangles, faceIndex + 86, faceIndex + 87, faceIndex + 110);
            AddTriangle(triangles, faceIndex + 111, faceIndex + 110, faceIndex + 87);
            AddTriangle(triangles, faceIndex + 87, faceIndex + 88, faceIndex + 111);
            AddTriangle(triangles, faceIndex + 112, faceIndex + 111, faceIndex + 88);
            AddTriangle(triangles, faceIndex + 88, faceIndex + 89, faceIndex + 112);
            AddTriangle(triangles, faceIndex + 113, faceIndex + 112, faceIndex + 89);
            AddTriangle(triangles, faceIndex + 89, faceIndex + 90, faceIndex + 113);
            AddTriangle(triangles, faceIndex + 114, faceIndex + 113, faceIndex + 90);
            AddTriangle(triangles, faceIndex + 90, faceIndex + 91, faceIndex + 114);
            AddTriangle(triangles, faceIndex + 115, faceIndex + 114, faceIndex + 91);
            AddTriangle(triangles, faceIndex + 91, faceIndex + 92, faceIndex + 115);
            AddTriangle(triangles, faceIndex + 116, faceIndex + 115, faceIndex + 92);
            AddTriangle(triangles, faceIndex + 92, faceIndex + 93, faceIndex + 116);
            AddTriangle(triangles, faceIndex + 117, faceIndex + 116, faceIndex + 93);
            AddTriangle(triangles, faceIndex + 93, faceIndex + 94, faceIndex + 117);
            AddTriangle(triangles, faceIndex + 118, faceIndex + 117, faceIndex + 94);
            AddTriangle(triangles, faceIndex + 94, faceIndex + 95, faceIndex + 118);
            AddTriangle(triangles, faceIndex + 119, faceIndex + 118, faceIndex + 95);
            AddTriangle(triangles, faceIndex + 95, faceIndex + 72, faceIndex + 119);
            AddTriangle(triangles, faceIndex + 96, faceIndex + 119, faceIndex + 72);
            AddTriangle(triangles, faceIndex + 96, faceIndex + 97, faceIndex + 120);
            AddTriangle(triangles, faceIndex + 121, faceIndex + 120, faceIndex + 97);
            AddTriangle(triangles, faceIndex + 97, faceIndex + 98, faceIndex + 121);
            AddTriangle(triangles, faceIndex + 122, faceIndex + 121, faceIndex + 98);
            AddTriangle(triangles, faceIndex + 98, faceIndex + 99, faceIndex + 122);
            AddTriangle(triangles, faceIndex + 123, faceIndex + 122, faceIndex + 99);
            AddTriangle(triangles, faceIndex + 99, faceIndex + 100, faceIndex + 123);
            AddTriangle(triangles, faceIndex + 124, faceIndex + 123, faceIndex + 100);
            AddTriangle(triangles, faceIndex + 100, faceIndex + 101, faceIndex + 124);
            AddTriangle(triangles, faceIndex + 125, faceIndex + 124, faceIndex + 101);
            AddTriangle(triangles, faceIndex + 101, faceIndex + 102, faceIndex + 125);
            AddTriangle(triangles, faceIndex + 126, faceIndex + 125, faceIndex + 102);
            AddTriangle(triangles, faceIndex + 102, faceIndex + 103, faceIndex + 126);
            AddTriangle(triangles, faceIndex + 127, faceIndex + 126, faceIndex + 103);
            AddTriangle(triangles, faceIndex + 103, faceIndex + 104, faceIndex + 127);
            AddTriangle(triangles, faceIndex + 128, faceIndex + 127, faceIndex + 104);
            AddTriangle(triangles, faceIndex + 104, faceIndex + 105, faceIndex + 128);
            AddTriangle(triangles, faceIndex + 129, faceIndex + 128, faceIndex + 105);
            AddTriangle(triangles, faceIndex + 105, faceIndex + 106, faceIndex + 129);
            AddTriangle(triangles, faceIndex + 130, faceIndex + 129, faceIndex + 106);
            AddTriangle(triangles, faceIndex + 106, faceIndex + 107, faceIndex + 130);
            AddTriangle(triangles, faceIndex + 131, faceIndex + 130, faceIndex + 107);
            AddTriangle(triangles, faceIndex + 107, faceIndex + 108, faceIndex + 131);
            AddTriangle(triangles, faceIndex + 132, faceIndex + 131, faceIndex + 108);
            AddTriangle(triangles, faceIndex + 108, faceIndex + 109, faceIndex + 132);
            AddTriangle(triangles, faceIndex + 133, faceIndex + 132, faceIndex + 109);
            AddTriangle(triangles, faceIndex + 109, faceIndex + 110, faceIndex + 133);
            AddTriangle(triangles, faceIndex + 134, faceIndex + 133, faceIndex + 110);
            AddTriangle(triangles, faceIndex + 110, faceIndex + 111, faceIndex + 134);
            AddTriangle(triangles, faceIndex + 135, faceIndex + 134, faceIndex + 111);
            AddTriangle(triangles, faceIndex + 111, faceIndex + 112, faceIndex + 135);
            AddTriangle(triangles, faceIndex + 136, faceIndex + 135, faceIndex + 112);
            AddTriangle(triangles, faceIndex + 112, faceIndex + 113, faceIndex + 136);
            AddTriangle(triangles, faceIndex + 137, faceIndex + 136, faceIndex + 113);
            AddTriangle(triangles, faceIndex + 113, faceIndex + 114, faceIndex + 137);
            AddTriangle(triangles, faceIndex + 138, faceIndex + 137, faceIndex + 114);
            AddTriangle(triangles, faceIndex + 114, faceIndex + 115, faceIndex + 138);
            AddTriangle(triangles, faceIndex + 139, faceIndex + 138, faceIndex + 115);
            AddTriangle(triangles, faceIndex + 115, faceIndex + 116, faceIndex + 139);
            AddTriangle(triangles, faceIndex + 140, faceIndex + 139, faceIndex + 116);
            AddTriangle(triangles, faceIndex + 116, faceIndex + 117, faceIndex + 140);
            AddTriangle(triangles, faceIndex + 141, faceIndex + 140, faceIndex + 117);
            AddTriangle(triangles, faceIndex + 117, faceIndex + 118, faceIndex + 141);
            AddTriangle(triangles, faceIndex + 142, faceIndex + 141, faceIndex + 118);
            AddTriangle(triangles, faceIndex + 118, faceIndex + 119, faceIndex + 142);
            AddTriangle(triangles, faceIndex + 143, faceIndex + 142, faceIndex + 119);
            AddTriangle(triangles, faceIndex + 119, faceIndex + 96, faceIndex + 143);
            AddTriangle(triangles, faceIndex + 120, faceIndex + 143, faceIndex + 96);
            faceIndex += 145;
            // Bottomface
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(0.129410, -0.500000, 0.482963));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 24, faceIndex + 1);
            return faceIndex;
        }
        private int AddGeometry_CylinderEndB(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Topface
            vertices.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.482963, -0.500000, 0.129410));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.894427, 0.447214, -0.000000));
            normals.Add(new Vector3D(0.863950, 0.447214, -0.231495));
            normals.Add(new Vector3D(0.774597, 0.447214, -0.447214));
            normals.Add(new Vector3D(0.632456, 0.447214, -0.632456));
            normals.Add(new Vector3D(0.447214, 0.447214, -0.774597));
            normals.Add(new Vector3D(0.231495, 0.447214, -0.863950));
            normals.Add(new Vector3D(-0.000000, 0.447214, -0.894427));
            normals.Add(new Vector3D(-0.231495, 0.447214, -0.863950));
            normals.Add(new Vector3D(-0.447214, 0.447214, -0.774597));
            normals.Add(new Vector3D(-0.632456, 0.447214, -0.632456));
            normals.Add(new Vector3D(-0.774597, 0.447214, -0.447214));
            normals.Add(new Vector3D(-0.863950, 0.447214, -0.231495));
            normals.Add(new Vector3D(-0.894427, 0.447214, 0.000000));
            normals.Add(new Vector3D(-0.863950, 0.447214, 0.231495));
            normals.Add(new Vector3D(-0.774597, 0.447214, 0.447214));
            normals.Add(new Vector3D(-0.632456, 0.447214, 0.632456));
            normals.Add(new Vector3D(-0.447214, 0.447214, 0.774597));
            normals.Add(new Vector3D(-0.231495, 0.447214, 0.863950));
            normals.Add(new Vector3D(0.000000, 0.447214, 0.894427));
            normals.Add(new Vector3D(0.231495, 0.447214, 0.863950));
            normals.Add(new Vector3D(0.447214, 0.447214, 0.774597));
            normals.Add(new Vector3D(0.632456, 0.447214, 0.632456));
            normals.Add(new Vector3D(0.774597, 0.447214, 0.447214));
            normals.Add(new Vector3D(0.863950, 0.447214, 0.231495));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 24, faceIndex + 1);
            faceIndex += 25;
            // Bottomface
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(0.129410, -0.500000, 0.482963));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 24, faceIndex + 1);
            return faceIndex;
        }
        private int AddGeometry_CylinderEndC(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Topface
            vertices.Add(new Point3D(-0.000000, -0.000000, 0.000000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.482963, -0.500000, 0.129410));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.707107, 0.707107, -0.000000));
            normals.Add(new Vector3D(0.683013, 0.707107, -0.183013));
            normals.Add(new Vector3D(0.612372, 0.707107, -0.353553));
            normals.Add(new Vector3D(0.500000, 0.707107, -0.500000));
            normals.Add(new Vector3D(0.353553, 0.707107, -0.612372));
            normals.Add(new Vector3D(0.183013, 0.707107, -0.683013));
            normals.Add(new Vector3D(-0.000000, 0.707107, -0.707107));
            normals.Add(new Vector3D(-0.183013, 0.707107, -0.683013));
            normals.Add(new Vector3D(-0.353553, 0.707107, -0.612372));
            normals.Add(new Vector3D(-0.500000, 0.707107, -0.500000));
            normals.Add(new Vector3D(-0.612372, 0.707107, -0.353553));
            normals.Add(new Vector3D(-0.683013, 0.707107, -0.183013));
            normals.Add(new Vector3D(-0.707107, 0.707107, 0.000000));
            normals.Add(new Vector3D(-0.683013, 0.707107, 0.183013));
            normals.Add(new Vector3D(-0.612372, 0.707107, 0.353553));
            normals.Add(new Vector3D(-0.500000, 0.707107, 0.500000));
            normals.Add(new Vector3D(-0.353553, 0.707107, 0.612372));
            normals.Add(new Vector3D(-0.183013, 0.707107, 0.683013));
            normals.Add(new Vector3D(0.000000, 0.707107, 0.707107));
            normals.Add(new Vector3D(0.183013, 0.707107, 0.683013));
            normals.Add(new Vector3D(0.353553, 0.707107, 0.612372));
            normals.Add(new Vector3D(0.500000, 0.707107, 0.500000));
            normals.Add(new Vector3D(0.612372, 0.707107, 0.353553));
            normals.Add(new Vector3D(0.683013, 0.707107, 0.183013));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 24, faceIndex + 1);
            faceIndex += 25;
            // Bottomface
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.129410, -0.500000, 0.482963));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(-0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(-0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(-0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.000000));
            vertices.Add(new Point3D(-0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(-0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(-0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(-0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.129410, -0.500000, -0.482963));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.433013));
            vertices.Add(new Point3D(0.353553, -0.500000, -0.353553));
            vertices.Add(new Point3D(0.433013, -0.500000, -0.250000));
            vertices.Add(new Point3D(0.482963, -0.500000, -0.129410));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.000000));
            vertices.Add(new Point3D(0.482963, -0.500000, 0.129410));
            vertices.Add(new Point3D(0.433013, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.353553, -0.500000, 0.353553));
            vertices.Add(new Point3D(0.250000, -0.500000, 0.433013));
            vertices.Add(new Point3D(0.129410, -0.500000, 0.482963));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 4, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 5, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 6, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 7, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 8, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 9, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 10, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 15, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 20, faceIndex + 21);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 21, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 22, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 23, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 24, faceIndex + 1);
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
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            normals.Add(new Vector3D(-0.000000, 0.000000, -1.000000));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, -0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            normals.Add(new Vector3D(-1.000000, 0.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, 0.500000, -0.500000));
            normals.Add(new Vector3D(0.408248, 0.816497, 0.408248));
            normals.Add(new Vector3D(0.408248, 0.816497, 0.408248));
            normals.Add(new Vector3D(0.408248, 0.816497, 0.408248));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 2, faceIndex + 1);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, -0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 1);
            faceIndex += 3;
            // Insideface
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
            normals.Add(new Vector3D(0.707107, -0.000000, 0.707107));
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
        private int AddGeometry_CylinderThin(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Sideface
            vertices.Add(new Point3D(0.241481, -0.500000, -0.064705));
            vertices.Add(new Point3D(0.216506, -0.500000, -0.125000));
            vertices.Add(new Point3D(0.176777, -0.500000, -0.176777));
            vertices.Add(new Point3D(0.125000, -0.500000, -0.216506));
            vertices.Add(new Point3D(0.064705, -0.500000, -0.241481));
            vertices.Add(new Point3D(-0.064705, -0.500000, -0.241481));
            vertices.Add(new Point3D(-0.125000, -0.500000, -0.216506));
            vertices.Add(new Point3D(-0.176777, -0.500000, -0.176777));
            vertices.Add(new Point3D(-0.216506, -0.500000, -0.125000));
            vertices.Add(new Point3D(-0.241481, -0.500000, -0.064705));
            vertices.Add(new Point3D(-0.241481, -0.500000, 0.064705));
            vertices.Add(new Point3D(-0.216506, -0.500000, 0.125000));
            vertices.Add(new Point3D(-0.176777, -0.500000, 0.176777));
            vertices.Add(new Point3D(-0.125000, -0.500000, 0.216506));
            vertices.Add(new Point3D(-0.064705, -0.500000, 0.241481));
            vertices.Add(new Point3D(0.064705, -0.500000, 0.241481));
            vertices.Add(new Point3D(0.125000, -0.500000, 0.216506));
            vertices.Add(new Point3D(0.176777, -0.500000, 0.176777));
            vertices.Add(new Point3D(0.216506, -0.500000, 0.125000));
            vertices.Add(new Point3D(0.241481, -0.500000, 0.064705));
            vertices.Add(new Point3D(0.241481, 0.500000, -0.064705));
            vertices.Add(new Point3D(0.216506, 0.500000, -0.125000));
            vertices.Add(new Point3D(0.176777, 0.500000, -0.176777));
            vertices.Add(new Point3D(0.125000, 0.500000, -0.216506));
            vertices.Add(new Point3D(0.064705, 0.500000, -0.241481));
            vertices.Add(new Point3D(-0.064705, 0.500000, -0.241481));
            vertices.Add(new Point3D(-0.125000, 0.500000, -0.216506));
            vertices.Add(new Point3D(-0.176777, 0.500000, -0.176777));
            vertices.Add(new Point3D(-0.216506, 0.500000, -0.125000));
            vertices.Add(new Point3D(-0.241481, 0.500000, -0.064705));
            vertices.Add(new Point3D(-0.241481, 0.500000, 0.064705));
            vertices.Add(new Point3D(-0.216506, 0.500000, 0.125000));
            vertices.Add(new Point3D(-0.176777, 0.500000, 0.176777));
            vertices.Add(new Point3D(-0.125000, 0.500000, 0.216506));
            vertices.Add(new Point3D(-0.064705, 0.500000, 0.241481));
            vertices.Add(new Point3D(0.064705, 0.500000, 0.241481));
            vertices.Add(new Point3D(0.125000, 0.500000, 0.216506));
            vertices.Add(new Point3D(0.176777, 0.500000, 0.176777));
            vertices.Add(new Point3D(0.216506, 0.500000, 0.125000));
            vertices.Add(new Point3D(0.241481, 0.500000, 0.064705));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.250000));
            vertices.Add(new Point3D(-0.000000, 0.500000, -0.250000));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.250000));
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.000000));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.000000));
            vertices.Add(new Point3D(0.250000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.000000));
            normals.Add(new Vector3D(0.976344, 0.000000, -0.216222));
            normals.Add(new Vector3D(0.887114, 0.000000, -0.461551));
            normals.Add(new Vector3D(0.737428, 0.000000, -0.675426));
            normals.Add(new Vector3D(0.537487, 0.000000, -0.843272));
            normals.Add(new Vector3D(0.300918, 0.000000, -0.953650));
            normals.Add(new Vector3D(-0.216222, 0.000000, -0.976344));
            normals.Add(new Vector3D(-0.461551, 0.000000, -0.887114));
            normals.Add(new Vector3D(-0.675426, 0.000000, -0.737428));
            normals.Add(new Vector3D(-0.843272, 0.000000, -0.537487));
            normals.Add(new Vector3D(-0.953650, 0.000000, -0.300918));
            normals.Add(new Vector3D(-0.976344, -0.000000, 0.216222));
            normals.Add(new Vector3D(-0.887114, -0.000000, 0.461551));
            normals.Add(new Vector3D(-0.737428, -0.000000, 0.675426));
            normals.Add(new Vector3D(-0.537487, -0.000000, 0.843272));
            normals.Add(new Vector3D(-0.300918, -0.000000, 0.953650));
            normals.Add(new Vector3D(0.216222, -0.000000, 0.976344));
            normals.Add(new Vector3D(0.461551, -0.000000, 0.887114));
            normals.Add(new Vector3D(0.675426, -0.000000, 0.737428));
            normals.Add(new Vector3D(0.843272, -0.000000, 0.537487));
            normals.Add(new Vector3D(0.953650, -0.000000, 0.300918));
            normals.Add(new Vector3D(0.953650, 0.000000, -0.300918));
            normals.Add(new Vector3D(0.843272, 0.000000, -0.537487));
            normals.Add(new Vector3D(0.675426, 0.000000, -0.737428));
            normals.Add(new Vector3D(0.461551, 0.000000, -0.887114));
            normals.Add(new Vector3D(0.216222, 0.000000, -0.976344));
            normals.Add(new Vector3D(-0.300918, 0.000000, -0.953650));
            normals.Add(new Vector3D(-0.537487, 0.000000, -0.843272));
            normals.Add(new Vector3D(-0.737428, 0.000000, -0.675426));
            normals.Add(new Vector3D(-0.887114, 0.000000, -0.461551));
            normals.Add(new Vector3D(-0.976344, 0.000000, -0.216222));
            normals.Add(new Vector3D(-0.953650, -0.000000, 0.300918));
            normals.Add(new Vector3D(-0.843272, -0.000000, 0.537487));
            normals.Add(new Vector3D(-0.675426, -0.000000, 0.737428));
            normals.Add(new Vector3D(-0.461551, -0.000000, 0.887114));
            normals.Add(new Vector3D(-0.216222, -0.000000, 0.976344));
            normals.Add(new Vector3D(0.300918, -0.000000, 0.953650));
            normals.Add(new Vector3D(0.537487, -0.000000, 0.843272));
            normals.Add(new Vector3D(0.737428, -0.000000, 0.675426));
            normals.Add(new Vector3D(0.887114, -0.000000, 0.461551));
            normals.Add(new Vector3D(0.976344, -0.000000, 0.216222));
            normals.Add(new Vector3D(-0.043842, -0.000000, 0.999038));
            normals.Add(new Vector3D(0.043842, -0.000000, 0.999038));
            normals.Add(new Vector3D(-0.043842, 0.000000, -0.999038));
            normals.Add(new Vector3D(0.043842, 0.000000, -0.999038));
            normals.Add(new Vector3D(-0.999038, -0.000000, 0.043842));
            normals.Add(new Vector3D(-0.999038, 0.000000, -0.043842));
            normals.Add(new Vector3D(0.999038, 0.000000, -0.043842));
            normals.Add(new Vector3D(0.999038, -0.000000, 0.043842));
            AddTriangle(triangles, faceIndex + 47, faceIndex + 0, faceIndex + 46);
            AddTriangle(triangles, faceIndex + 20, faceIndex + 46, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 21, faceIndex + 20, faceIndex + 1);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 21);
            AddTriangle(triangles, faceIndex + 22, faceIndex + 21, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 22);
            AddTriangle(triangles, faceIndex + 23, faceIndex + 22, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 24, faceIndex + 23, faceIndex + 4);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 43, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 42, faceIndex + 24, faceIndex + 43);
            AddTriangle(triangles, faceIndex + 43, faceIndex + 5, faceIndex + 42);
            AddTriangle(triangles, faceIndex + 25, faceIndex + 42, faceIndex + 5);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 6, faceIndex + 25);
            AddTriangle(triangles, faceIndex + 26, faceIndex + 25, faceIndex + 6);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 7, faceIndex + 26);
            AddTriangle(triangles, faceIndex + 27, faceIndex + 26, faceIndex + 7);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 8, faceIndex + 27);
            AddTriangle(triangles, faceIndex + 28, faceIndex + 27, faceIndex + 8);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 9, faceIndex + 28);
            AddTriangle(triangles, faceIndex + 29, faceIndex + 28, faceIndex + 9);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 45, faceIndex + 29);
            AddTriangle(triangles, faceIndex + 44, faceIndex + 29, faceIndex + 45);
            AddTriangle(triangles, faceIndex + 45, faceIndex + 10, faceIndex + 44);
            AddTriangle(triangles, faceIndex + 30, faceIndex + 44, faceIndex + 10);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 11, faceIndex + 30);
            AddTriangle(triangles, faceIndex + 31, faceIndex + 30, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 11, faceIndex + 12, faceIndex + 31);
            AddTriangle(triangles, faceIndex + 32, faceIndex + 31, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 12, faceIndex + 13, faceIndex + 32);
            AddTriangle(triangles, faceIndex + 33, faceIndex + 32, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 13, faceIndex + 14, faceIndex + 33);
            AddTriangle(triangles, faceIndex + 34, faceIndex + 33, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 14, faceIndex + 40, faceIndex + 34);
            AddTriangle(triangles, faceIndex + 41, faceIndex + 34, faceIndex + 40);
            AddTriangle(triangles, faceIndex + 40, faceIndex + 15, faceIndex + 41);
            AddTriangle(triangles, faceIndex + 35, faceIndex + 41, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 15, faceIndex + 16, faceIndex + 35);
            AddTriangle(triangles, faceIndex + 36, faceIndex + 35, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 16, faceIndex + 17, faceIndex + 36);
            AddTriangle(triangles, faceIndex + 37, faceIndex + 36, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 17, faceIndex + 18, faceIndex + 37);
            AddTriangle(triangles, faceIndex + 38, faceIndex + 37, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 18, faceIndex + 19, faceIndex + 38);
            AddTriangle(triangles, faceIndex + 39, faceIndex + 38, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 19, faceIndex + 47, faceIndex + 39);
            AddTriangle(triangles, faceIndex + 46, faceIndex + 39, faceIndex + 47);
            faceIndex += 48;
            // Topface
            vertices.Add(new Point3D(-0.000000, 0.500000, 0.000000));
            vertices.Add(new Point3D(0.241481, 0.500000, -0.064705));
            vertices.Add(new Point3D(0.216506, 0.500000, -0.125000));
            vertices.Add(new Point3D(0.176777, 0.500000, -0.176777));
            vertices.Add(new Point3D(0.125000, 0.500000, -0.216506));
            vertices.Add(new Point3D(0.064705, 0.500000, -0.241481));
            vertices.Add(new Point3D(-0.064705, 0.500000, -0.241481));
            vertices.Add(new Point3D(-0.125000, 0.500000, -0.216506));
            vertices.Add(new Point3D(-0.176777, 0.500000, -0.176777));
            vertices.Add(new Point3D(-0.216506, 0.500000, -0.125000));
            vertices.Add(new Point3D(-0.241481, 0.500000, -0.064705));
            vertices.Add(new Point3D(-0.241481, 0.500000, 0.064705));
            vertices.Add(new Point3D(-0.216506, 0.500000, 0.125000));
            vertices.Add(new Point3D(-0.176777, 0.500000, 0.176777));
            vertices.Add(new Point3D(-0.125000, 0.500000, 0.216506));
            vertices.Add(new Point3D(-0.064705, 0.500000, 0.241481));
            vertices.Add(new Point3D(0.064705, 0.500000, 0.241481));
            vertices.Add(new Point3D(0.125000, 0.500000, 0.216506));
            vertices.Add(new Point3D(0.176777, 0.500000, 0.176777));
            vertices.Add(new Point3D(0.216506, 0.500000, 0.125000));
            vertices.Add(new Point3D(0.241481, 0.500000, 0.064705));
            vertices.Add(new Point3D(-0.000000, 0.500000, -0.250000));
            vertices.Add(new Point3D(-0.250000, 0.500000, 0.000000));
            vertices.Add(new Point3D(0.250000, 0.500000, -0.000000));
            vertices.Add(new Point3D(0.000000, 0.500000, 0.250000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(0.000000, 1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 23, faceIndex + 0, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 23, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 15, faceIndex + 24);
            AddTriangle(triangles, faceIndex + 21, faceIndex + 6, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 7, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 8, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 9, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 10, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 22, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 22, faceIndex + 11);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 11, faceIndex + 12);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 12, faceIndex + 13);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 13, faceIndex + 14);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 14, faceIndex + 15);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 24, faceIndex + 16);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 16, faceIndex + 17);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 17, faceIndex + 18);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 18, faceIndex + 19);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 19, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 2, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 4, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 5, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 21, faceIndex + 0);
            faceIndex += 25;
            // Bottomface
            vertices.Add(new Point3D(0.241481, -0.500000, -0.064705));
            vertices.Add(new Point3D(0.216506, -0.500000, -0.125000));
            vertices.Add(new Point3D(0.176777, -0.500000, -0.176777));
            vertices.Add(new Point3D(0.125000, -0.500000, -0.216506));
            vertices.Add(new Point3D(0.064705, -0.500000, -0.241481));
            vertices.Add(new Point3D(-0.064705, -0.500000, -0.241481));
            vertices.Add(new Point3D(-0.125000, -0.500000, -0.216506));
            vertices.Add(new Point3D(-0.176777, -0.500000, -0.176777));
            vertices.Add(new Point3D(-0.216506, -0.500000, -0.125000));
            vertices.Add(new Point3D(-0.241481, -0.500000, -0.064705));
            vertices.Add(new Point3D(-0.241481, -0.500000, 0.064705));
            vertices.Add(new Point3D(-0.216506, -0.500000, 0.125000));
            vertices.Add(new Point3D(-0.176777, -0.500000, 0.176777));
            vertices.Add(new Point3D(-0.125000, -0.500000, 0.216506));
            vertices.Add(new Point3D(-0.064705, -0.500000, 0.241481));
            vertices.Add(new Point3D(0.064705, -0.500000, 0.241481));
            vertices.Add(new Point3D(0.125000, -0.500000, 0.216506));
            vertices.Add(new Point3D(0.176777, -0.500000, 0.176777));
            vertices.Add(new Point3D(0.216506, -0.500000, 0.125000));
            vertices.Add(new Point3D(0.241481, -0.500000, 0.064705));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.000000));
            vertices.Add(new Point3D(-0.250000, -0.500000, 0.000000));
            vertices.Add(new Point3D(0.000000, -0.500000, 0.250000));
            vertices.Add(new Point3D(0.250000, -0.500000, -0.000000));
            vertices.Add(new Point3D(-0.000000, -0.500000, -0.250000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(-0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 19, faceIndex + 20, faceIndex + 23);
            AddTriangle(triangles, faceIndex + 21, faceIndex + 9, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 22, faceIndex + 14, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 24, faceIndex + 4, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 5, faceIndex + 24, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 6, faceIndex + 5, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 7, faceIndex + 6, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 8, faceIndex + 7, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 9, faceIndex + 8, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 10, faceIndex + 21, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 11, faceIndex + 10, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 12, faceIndex + 11, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 13, faceIndex + 12, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 14, faceIndex + 13, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 15, faceIndex + 22, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 16, faceIndex + 15, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 17, faceIndex + 16, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 18, faceIndex + 17, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 19, faceIndex + 18, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 23, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 1, faceIndex + 0, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 2, faceIndex + 20);
            AddTriangle(triangles, faceIndex + 4, faceIndex + 3, faceIndex + 20);
            return faceIndex;
        }
        private int AddGeometry_SlopeHalf(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Insideface
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(-0.707107, -0.000000, -0.707107));
            normals.Add(new Vector3D(-0.707107, -0.000000, -0.707107));
            normals.Add(new Vector3D(-0.707107, -0.000000, -0.707107));
            normals.Add(new Vector3D(-0.707107, -0.000000, -0.707107));
            AddTriangle(triangles, faceIndex + 3, faceIndex + 0, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 3);
            faceIndex += 4;
            // Leftface
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            normals.Add(new Vector3D(1.000000, 0.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            faceIndex += 4;
            // Topface
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(0.500000, 0.000000, -0.500000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            normals.Add(new Vector3D(-0.000000, 1.000000, 0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Bottomface
            vertices.Add(new Point3D(0.500000, -0.500000, -0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            normals.Add(new Vector3D(0.000000, -1.000000, -0.000000));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            // Frontface
            vertices.Add(new Point3D(0.500000, -0.000000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(0.500000, -0.500000, 0.500000));
            vertices.Add(new Point3D(-0.500000, -0.000000, 0.500000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            normals.Add(new Vector3D(0.000000, -0.000000, 1.000000));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 0, faceIndex + 3);
            AddTriangle(triangles, faceIndex + 3, faceIndex + 1, faceIndex + 2);
            return faceIndex;
        }
        #endregion AddGeometry
    }
}
