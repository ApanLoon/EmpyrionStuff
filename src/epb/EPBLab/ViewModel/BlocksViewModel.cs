
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
                    case 381:
                    case 383:
                    case 397:
                    case 400:
                    case 403:
                    case 406:
                    case 409:
                    case 412:
                        model = CreateBuildingBlockFull(pos, block, blueprint);
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
                /*
                case 1: //CutCorner
                    break;
                case 2: //CornerLongA
                    break;
                case 3: //CornerLongB
                    break;
                case 4: //CornerLongC
                    break;
                */
                case 5: //CornerLongD
                    faceIndex = AddGeometry_CornerLongD(vertices, triangles, normals, faceIndex);
                    break;

                case 6: //CornerLargeA
                    faceIndex = AddGeometry_CornerLargeA(vertices, triangles, normals, faceIndex);
                    break;
                case 7: //Corner
                    faceIndex = AddGeometry_Corner(vertices, triangles, normals, faceIndex);
                    break;
                /*
                case 8: //RampBottom
                    break;
                case 9: //RampTop
                    break;
                */
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
                case 27: //CornerLargeB
                    break;
                case 28: //CornerLargeC
                    break;
                case 29: //CornerLargeD
                    break;
                case 30: //CornerLongE
                    break;
                case 31: //PyramidA
                    break;
                */
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

        private int AddGeometry_Cube(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            // Front face:
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, 0.5));
            vertices.Add(new Point3D(-0.5, 0.5, 0.5));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            faceIndex += 4;

            // Back face:
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            vertices.Add(new Point3D(0.5, 0.5, -0.5));
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;

            // Right face:
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(-0.5, 0.5, 0.5));
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            normals.Add(new Vector3D(-1, 0, 0));
            normals.Add(new Vector3D(-1, 0, 0));
            normals.Add(new Vector3D(-1, 0, 0));
            normals.Add(new Vector3D(-1, 0, 0));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            faceIndex += 4;

            // Left face:
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, -0.5));
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;

            // Top face:
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            vertices.Add(new Point3D(-0.5, 0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, -0.5));
            normals.Add(new Vector3D(0, 1, 0));
            normals.Add(new Vector3D(0, 1, 0));
            normals.Add(new Vector3D(0, 1, 0));
            normals.Add(new Vector3D(0, 1, 0));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            faceIndex += 4;

            // Bottom face:
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;
            return faceIndex;
        }
        private int AddGeometry_CornerLongD(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            Vector3D v1;
            Vector3D v2;
            Vector3D normal;

            // Inside face:
            vertices.Add(new Point3D(0.5, -0.5, 0.0));
            vertices.Add(new Point3D(0.0, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, 0.5));
            v1 = new Vector3D(vertices[faceIndex + 0].X, vertices[faceIndex + 0].Y, vertices[faceIndex + 0].Z);
            v2 = new Vector3D(vertices[faceIndex + 1].X, vertices[faceIndex + 1].Y, vertices[faceIndex + 1].Z);
            normal = Vector3D.CrossProduct(v1, v2);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Left face:
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.0));
            vertices.Add(new Point3D(0.5, 0.5, 0.5));
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Front face:
            vertices.Add(new Point3D(0.0, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, 0.5));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Bottom face:
            vertices.Add(new Point3D(0.0, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.0));
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            return faceIndex;
        }
        private int AddGeometry_CornerLargeA(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            Vector3D v1;
            Vector3D v2;
            Vector3D normal;

            // Left face:
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            v1 = new Vector3D(vertices[faceIndex + 0].X, vertices[faceIndex + 0].Y, vertices[faceIndex + 0].Z);
            v2 = new Vector3D(vertices[faceIndex + 1].X, vertices[faceIndex + 1].Y, vertices[faceIndex + 1].Z);
            normal = Vector3D.CrossProduct(v1, v2);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Right face:
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            normals.Add(new Vector3D(-1, 0, 0));
            normals.Add(new Vector3D(-1, 0, 0));
            normals.Add(new Vector3D(-1, 0, 0));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Front face:
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            v1 = new Vector3D(vertices[faceIndex + 0].X, vertices[faceIndex + 0].Y, vertices[faceIndex + 0].Z);
            v2 = new Vector3D(vertices[faceIndex + 1].X, vertices[faceIndex + 1].Y, vertices[faceIndex + 1].Z);
            normal = Vector3D.CrossProduct(v1, v2);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Back face:
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Bottom face:
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;
            return faceIndex;
        }
        private int AddGeometry_Corner(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            Vector3D v1;
            Vector3D v2;
            Vector3D normal;

            // Inside face:
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            v1 = new Vector3D(vertices[faceIndex + 0].X, vertices[faceIndex + 0].Y, vertices[faceIndex + 0].Z);
            v2 = new Vector3D(vertices[faceIndex + 1].X, vertices[faceIndex + 1].Y, vertices[faceIndex + 1].Z);
            normal = Vector3D.CrossProduct(v1, v2);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Right face:
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            normals.Add(new Vector3D(-1, 0, 0));
            normals.Add(new Vector3D(-1, 0, 0));
            normals.Add(new Vector3D(-1, 0, 0));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Back face:
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, 0.5, -0.5));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            normals.Add(new Vector3D(0, 0, -1));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Bottom face:
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;
            return faceIndex;
        }
        private int AddGeometry_Slope(Point3DCollection vertices, Int32Collection triangles, Vector3DCollection normals, int faceIndex)
        {
            Vector3D v1;
            Vector3D v2;
            Vector3D normal;

            // Inside face:
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, 0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, 0.5));
            v1 = new Vector3D(vertices[faceIndex + 0].X, vertices[faceIndex + 0].Y, vertices[faceIndex + 0].Z);
            v2 = new Vector3D(vertices[faceIndex + 1].X, vertices[faceIndex + 1].Y, vertices[faceIndex + 1].Z);
            normal = Vector3D.CrossProduct(v1, v2);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            AddTriangle(triangles, faceIndex + 2, faceIndex + 3, faceIndex + 0);
            faceIndex += 4;

            // Front face:
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(-0.5, 0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, 0.5));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            normals.Add(new Vector3D(0, 0, 1));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;

            // Right face:
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(-0.5, 0.5, 0.5));
            normals.Add(new Vector3D(-1, 0, 0));
            normals.Add(new Vector3D(-1, 0, 0));
            normals.Add(new Vector3D(-1, 0, 0));
            AddTriangle(triangles, faceIndex + 0, faceIndex + 1, faceIndex + 2);
            faceIndex += 3;

            // Left face:
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, 0.5, 0.5));
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            faceIndex += 3;

            // Bottom face:
            vertices.Add(new Point3D(-0.5, -0.5, -0.5));
            vertices.Add(new Point3D(-0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, 0.5));
            vertices.Add(new Point3D(0.5, -0.5, -0.5));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            normals.Add(new Vector3D(0, -1, 0));
            AddTriangle(triangles, faceIndex + 2, faceIndex + 1, faceIndex + 0);
            AddTriangle(triangles, faceIndex + 0, faceIndex + 3, faceIndex + 2);
            faceIndex += 4;
            return faceIndex;
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
    }
}
