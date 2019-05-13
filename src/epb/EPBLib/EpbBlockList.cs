using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPBLib
{
    public class EpbBlockList : IEnumerable<EpbBlock>
    {
        protected Dictionary<EpbBlockPos, EpbBlock> Blocks;

        public EpbBlockList()
        {
            Blocks = new Dictionary<EpbBlockPos, EpbBlock>(new KeyComparer());
        }

        public EpbBlock this[EpbBlockPos pos]
        {
            get => Blocks.ContainsKey(pos) ? Blocks[pos] : null;
            set => Blocks[pos] = value;
        }

        public EpbBlock this[byte x, byte y, byte z]
        {
            get => this[new EpbBlockPos(x, y, z)];
            set => this[new EpbBlockPos(x, y, z)] = value;
        }

        public int Count => Blocks.Count;

        public class KeyComparer : IEqualityComparer<EpbBlockPos>
        {
            public bool Equals(EpbBlockPos a, EpbBlockPos b)
            {
                return a != null && b != null && a.X == b.X && a.Y == b.Y && a.Z == b.Z;
            }

            public int GetHashCode(EpbBlockPos obj)
            {
                return (obj.X << 16) + (obj.Y << 8) + (obj.Z);
            }
        }

        public IEnumerator<EpbBlock> GetEnumerator()
        {
            return Blocks.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
