using UnityEngine;
using System.Linq;

namespace VT0
{
    public class VTTree
    {
        public const int FilledWithMultipleValues = -1;

        private int? filledSelf;

        public int? FilledExcept(int? value = null)
        {
            if (filledSelf.HasValue) return filledSelf == value ? null : filledSelf;
            if (Descendents != null) {
                foreach (var d in Descendents) {
                    if (d.FilledExcept(value).HasValue)
                        return FilledWithMultipleValues;
                }
            }
            return null;
        }

        public VTTree[] Descendents { get; }
        public Vector2? Pack(int smallness, int? value, bool actuallyAddIt)
        {
            if (smallness < 0) {
                throw new System.ArgumentException(nameof(smallness));
            }
            else if (smallness == 0)
            {
                // If this cell is 'empty', add the ID here
                if (!FilledExcept(value).HasValue) {
                    if (actuallyAddIt) {
                        filledSelf = value;
                    }
                    return new Vector2(0, 0);
                }
            } else if (Descendents != null) {
                // Otherwise, descend.
                smallness--;
                for (int i = 0; i < Descendents.Length; i++)
                {
                    var v = Descendents[i].Pack(smallness, value, actuallyAddIt);
                    if (v.HasValue) {
                        return (v.Value + new Vector2(i%2, i/2))/2;
                    }
                }
            }
            return null;
        }

        public void Clear()
        {
            if (Descendents != null) {
                foreach (var d in Descendents) {
                    d.Clear();
                }
            }
            filledSelf = null;
        }

        public bool Remove(int id)
        {
            if (filledSelf == id)
            {
                filledSelf = null;
                return true;
            } else if(Descendents != null) {
                foreach (var d in Descendents) {
                    if (d.Remove(id)) {
                        return true;
                    }
                }
            }
            return true;
        }

        public int? GetSmallness(int id)
        {
            if (filledSelf == id) return 0;
            foreach (var d in Descendents)
            {
                var s = d.GetSmallness(id);
                if (s.HasValue) return s + 1;
            }
            return null;
        }

        public VTTree(int size)
        {
            const int Rank = 4;
            if (size > 0)
            {
                size--;
                Descendents = Enumerable.Range(0, Rank)
                    .Select(i => new VTTree(size))
                    .ToArray();
            }
        }
    }
}