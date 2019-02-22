
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using EPBLib;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel.Tree
{
    public class BlockDefaultViewModel : ITreeNode
    {
        protected EpbBlock Block;
        protected EpBlueprint Blueprint;

        public string Title { get; set; }
        public ObservableCollection<ITreeNode> Children { get; set; }

        public Point3D Position { get; set; }

        public BlockDefaultViewModel(EpbBlock block, EpBlueprint blueprint)
        {
            Block = block;
            Blueprint = blueprint;
            Title = block.BlockType.ToString();
            Children = new ObservableCollection<ITreeNode>();
            EpbBlockPos pos = block.Position;
            Position = new Point3D(pos.X - Blueprint.Width / 2.0, pos.Y - Blueprint.Height / 2.0, pos.Z - Blueprint.Depth / 2.0);
        }
    }
}
