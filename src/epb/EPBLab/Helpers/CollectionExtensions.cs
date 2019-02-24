
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EPBLab.Helpers
{
    public static class CollectionExtensions
    {
        public static void AddSorted<T>(this Collection<T> collection, T item, IComparer<T> comparer = null)
        {
            if (comparer == null)
            {
                comparer = Comparer<T>.Default;
            }

            int i = 0;
            while (i < collection.Count && comparer.Compare(collection[i], item) < 0)
            {
                i++;
            }

            collection.Insert(i, item);
        }
    }
}
