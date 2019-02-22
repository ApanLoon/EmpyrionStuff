
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using EPBLib;

namespace EPBLab.ViewModel.Tree
{
    public class BlockLcdViewModel : ITreeNode
    {
        protected EpbBlock Block;
        protected EpBlueprint Blueprint;

        public string Title { get; set; }
        public ObservableCollection<ITreeNode> Children { get; set; }

        public Point3D Position { get; set; }

        public string Text
        {
            get
            {
                EpbBlockTag tag = Block.GetTag("Text");
                if (tag != null && tag is EpbBlockTagString s)
                {
                    return s.Value;
                }
                return "";
            }
        }
        public Color ColB
        {
            get
            {
                EpbBlockTag tag = Block.GetTag("ColB");
                if (tag != null && tag is EpbBlockTagColour t)
                {
                    return Color.FromArgb(t.Alpha, t.Red, t.Green, t.Blue);
                }
                return Color.FromRgb(255,255,255);
            }
        }
        public Color ColF
        {
            get
            {
                EpbBlockTag tag = Block.GetTag("ColF");
                if (tag != null && tag is EpbBlockTagColour t)
                {
                    return Color.FromArgb(t.Alpha, t.Red, t.Green, t.Blue);
                }
                return Color.FromRgb(0, 0, 0);
            }
        }

        public BlockLcdViewModel(EpbBlock block, EpBlueprint blueprint)
        {
            Block = block;
            Blueprint = blueprint;
            Title = block.BlockType.ToString();
            EpbBlockPos pos = block.Position;
            Position = new Point3D(pos.X - Blueprint.Width / 2.0, pos.Y - Blueprint.Height / 2.0, pos.Z - Blueprint.Depth / 2.0);
        }
    }
}
