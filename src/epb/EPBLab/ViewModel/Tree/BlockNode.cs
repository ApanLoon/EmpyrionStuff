
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using EPBLab.Helpers;
using EPBLib;
using EPBLib.BlockData;

namespace EPBLab.ViewModel.Tree
{
    public class BlockNode : TreeNode
    {
        protected EpbBlock Block;
        protected EpBlueprint Blueprint;

        public string BlockType => Block.BlockType.ToString();
        public string Variant => Block.VariantName;

        public Point3D Position { get; set; }

        public EpbBlock.EpbBlockRotation Rotation => Block.Rotation;

        public Color[] Colours { get; set; }
        public byte[] Textures => Block.Textures;

        public BlockNode(EpbBlock block, EpBlueprint blueprint)
        {
            Block = block;
            Blueprint = blueprint;
            Title = block.BlockType.ToString();
            EpbBlockPos pos = block.Position;
            Position = new Point3D(pos.X - Blueprint.Width / 2.0, pos.Y - Blueprint.Height / 2.0, pos.Z - Blueprint.Depth / 2.0);
            Colours = Block.Colours.Select(index => Blueprint.Palette[(EpbColourIndex)index].ToColor()).ToArray();
        }
    }
}
