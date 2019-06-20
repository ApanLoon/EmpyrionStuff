using EPBLab.Helpers;
using EPBLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace EPBLab.ViewModel.Tree
{
    public class BlockNode : TreeNode
    {
        public Block Block;
        protected Blueprint Blueprint;

        public string BlockType => Block.BlockType.ToString();
        public string VariantName => Block.VariantName;
        public string Variant => $"(0x{Block.Variant:x2}={Block.Variant})";

        public Point3D Position { get; set; }

        public Block.BlockRotation Rotation => Block.Rotation;

        public ColourInfo[] Colours { get; set; }
        public byte[] Textures => Block.Textures;

        public List<BlockTag> Tags => Block.Tags.Values.ToList();

        public BlockNode(Block block, Blueprint blueprint)
        {
            Block = block;
            Blueprint = blueprint;
            Title = block.BlockType.Name;
            if (block.VariantName != "")
            {
                Title += "/" + block.VariantName;
            }

            BlockPos pos = block.Position;
            Position = new Point3D(pos.X, pos.Y, pos.Z);

            Colours = Block.Colours.Select((colourIndex, faceIndex) => new ColourInfo()
            {
                Face = ((Block.FaceIndex)faceIndex).ToString(),
                Index = (byte)colourIndex,
                Colour = Blueprint.Palette[colourIndex].ToColor()
            } ).ToArray();
        }
    }

    public class ColourInfo
    {
        public string Face { get; set; }
        public byte Index { get; set; }
        public Color Colour { get; set; }
    }
}
