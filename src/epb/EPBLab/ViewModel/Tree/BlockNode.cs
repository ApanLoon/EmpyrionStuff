
using System.Windows.Media.Media3D;
using EPBLib;

namespace EPBLab.ViewModel.Tree
{
    public class BlockNode : TreeNode
    {
        protected EpbBlock Block;
        protected EpBlueprint Blueprint;

        public Point3D Position { get; set; }

        public BlockNode(EpbBlock block, EpBlueprint blueprint)
        {
            Block = block;
            Blueprint = blueprint;
            Title = block.BlockType.ToString();
            EpbBlockPos pos = block.Position;
            Position = new Point3D(pos.X - Blueprint.Width / 2.0, pos.Y - Blueprint.Height / 2.0, pos.Z - Blueprint.Depth / 2.0);
        }
    }
}
