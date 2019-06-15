
namespace EPBLib
{
    public class BlockTagPos : BlockTag
    {
        public BlockTagPos()
        {
            BlockTagType = TagType.UInt32;
            Name = "Pos";
        }

        public BlockTagPos(BlockPos value) : this()
        {
            Value = value;
        }

        public BlockPos Value { get; set; }

        public override string ToString()
        {
            return $"{Name,-14}: {Value}";
        }
    }
}
