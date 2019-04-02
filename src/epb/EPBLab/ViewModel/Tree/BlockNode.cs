
using System;
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
        public EpbBlock Block;
        protected EpBlueprint Blueprint;

        public string BlockType => Block.BlockType.ToString();
        public string VariantName => Block.VariantName;
        public string Variant => $"(0x{Block.Variant:x2}={Block.Variant})";

        public Point3D Position { get; set; }

        public EpbBlock.EpbBlockRotation Rotation => Block.Rotation;

        public Color[] Colours { get; set; }
        public byte[] Textures => Block.Textures;

        public BlockNode(EpbBlock block, EpBlueprint blueprint)
        {
            Block = block;
            Blueprint = blueprint;
            Title = block.BlockType.Name;
            if (block.VariantName != "")
            {
                Title += "/" + block.VariantName;
            }
            EpbBlockPos pos = block.Position;
            Position = new Point3D(Math.Floor(pos.X - Blueprint.Width  / 2.0),
                                   Math.Floor(pos.Y - Blueprint.Height / 2.0),
                                   //Math.Floor(pos.Z - Blueprint.Depth  / 2.0));
                                   Math.Floor(Blueprint.Depth / 2.0 - pos.Z)); //TODO: Is this really correct? It makes Pyramid look right.
            Colours = Block.Colours.Select(index => Blueprint.Palette[(EpbColourIndex)index].ToColor()).ToArray();
        }
    }
}
