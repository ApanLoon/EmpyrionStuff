using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPBLib
{
    public class BlockList : IEnumerable<Block>
    {
        protected Dictionary<BlockPos, Block> Blocks;

        public BlockList()
        {
            Blocks = new Dictionary<BlockPos, Block>(new KeyComparer());
        }

        public Block this[BlockPos pos]
        {
            get => Blocks.ContainsKey(pos) ? Blocks[pos] : null;
            set => Blocks[pos] = value;
        }

        public Block this[byte x, byte y, byte z]
        {
            get => this[new BlockPos(x, y, z)];
            set => this[new BlockPos(x, y, z)] = value;
        }

        public int Count => Blocks.Count;

        public void Remove(BlockPos pos)
        {
            if (Blocks.ContainsKey(pos))
            {
                Blocks.Remove(pos);
            }
        }
        public void Remove(byte x, byte y, byte z)
        {
            Remove(new BlockPos(x, y, z));
        }

        public void Remove(Block block)
        {
            Remove(block.Position);
        }

        public class KeyComparer : IEqualityComparer<BlockPos>
        {
            public bool Equals(BlockPos a, BlockPos b)
            {
                return a != null && b != null && a.X == b.X && a.Y == b.Y && a.Z == b.Z;
            }

            public int GetHashCode(BlockPos obj)
            {
                return (obj.X << 16) + (obj.Y << 8) + (obj.Z);
            }
        }

        public IEnumerator<Block> GetEnumerator()
        {
            return Blocks.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
