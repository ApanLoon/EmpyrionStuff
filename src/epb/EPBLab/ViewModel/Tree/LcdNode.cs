
using System.Windows.Media;
using EPBLib;

namespace EPBLab.ViewModel.Tree
{
    public class LcdNode : BlockNode
    {
        public string Text
        {
            get
            {
                BlockTag tag = Block.GetTag("Text");
                if (tag != null && tag is BlockTagString s)
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
                BlockTag tag = Block.GetTag("ColB");
                if (tag != null && tag is BlockTagColour t)
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
                BlockTag tag = Block.GetTag("ColF");
                if (tag != null && tag is BlockTagColour t)
                {
                    return Color.FromArgb(t.Alpha, t.Red, t.Green, t.Blue);
                }
                return Color.FromRgb(0, 0, 0);
            }
        }

        public LcdNode(Block block, Blueprint blueprint):base (block, blueprint)
        {
        }
    }
}
