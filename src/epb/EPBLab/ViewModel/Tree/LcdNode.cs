
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

        public LcdNode(EpbBlock block, EpBlueprint blueprint):base (block, blueprint)
        {
        }
    }
}
