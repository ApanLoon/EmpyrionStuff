
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
        protected EpBlueprint Blueprint;

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

                ITreeNode blockNode;
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
            }
        }

        private void BuildModel(EpBlueprint blueprint)
        {
            Model3DGroup group = new Model3DGroup();
            for (int k = 0; k < blueprint.Depth; k++)
            {
                for (int j = 0; j < blueprint.Height; j++)
                {
                    for (int i = 0; i < blueprint.Width; i++)
                    {
                        EpbBlock block = blueprint.Blocks[i, j, k];
                        if (block != null)
                        {
                            EpbColourIndex colourIndex = block.Colours[0];
                            EpbColour colour = blueprint.Palette[colourIndex];
                            Color c = Color.FromArgb(128, colour.R, colour.G, colour.B);
                            group.Children.Add(CreateBox(new Point3D(i - blueprint.Width / 2.0, j - blueprint.Height / 2.0, k - blueprint.Depth / 2.0), c));
                        }
                    }
                }
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

        private GeometryModel3D CreateBox(Point3D pos, Color colour)
        {
            GeometryModel3D model = new GeometryModel3D();
            MeshGeometry3D mesh = new MeshGeometry3D();
            Point3DCollection vertices = new Point3DCollection();
            Int32Collection triangles = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();


            int faceIndex = 0;
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
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
            normals.Add(new Vector3D(1, 0, 0));
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


            mesh.Positions = vertices;
            mesh.TriangleIndices = triangles;
            mesh.Normals = normals;

            model.Geometry = mesh;
            model.Transform = new TranslateTransform3D(pos.X, pos.Y, pos.Z);

            
            SolidColorBrush brush = new SolidColorBrush(colour);
            DiffuseMaterial material = new DiffuseMaterial(brush);
            model.Material = material;

            //SolidColorBrush back = new SolidColorBrush(Color.FromRgb(0, 0, 255));
            //DiffuseMaterial backMaterial = new DiffuseMaterial(back);
            //model.BackMaterial = backMaterial;
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
