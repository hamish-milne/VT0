using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace VT0
{
    public interface ITextureOutput
    {
        int Size { get; }
        int GetMaxSize(Object obj);
        void Copy(Object obj, Vector2 position, int size);
        void Remove(Object obj);
    }

    public interface IArranger
    {
        void Update(IPriority priority, ITextureOutput output);
    }

    public class Arranger : IArranger
    {
        public float Hysteresis { get; set; } = 0.5f;

        public IPriority Priority { get; set; }
        public ITextureOutput Output { get; set; }

        private readonly VTTree _tree = new VTTree(16384/128);

        public void Update(IPriority priority, ITextureOutput output)
        {
            var set = new ValueTuple<Object, float>[priority.Count];
            for (int idx = 0; idx < priority.Count; idx++)
            {
                set[idx++] = new ValueTuple<Object, float>(Priority.GetObject(idx), Priority.GetPriority(idx));
            }
            Array.Sort(set, (a, b) => a.Item2.CompareTo(b.Item2));

            var operations = new List<ValueTuple<Object, int, int, int>>();
            
            for (int i = 0; i < set.Length; i++)
            {
                var r = set[i].Item1;
                var rawArea = set[i].Item2;
                var outSize = Output.Size;
                var normSize = Mathf.Sqrt(rawArea) * outSize;
                var smallness = _tree.GetSmallness(r.GetInstanceID());
                var currentSize = smallness.HasValue ? (int)(outSize / Mathf.Pow(2, smallness.Value)) : 0;

                var candidateNextSize = 0;
                if (normSize > 0.5f) // If size < 0.5, try to unload it completely
                {
                    // Only go in single steps:
                    candidateNextSize = normSize < currentSize ? currentSize*2 : currentSize/2;
                    // Use the below to jump to the closest size right away:
                    // candidateNextSize = Mathf.ClosestPowerOfTwo(Mathf.RoundToInt(normSize));
                }
                candidateNextSize = Mathf.Clamp(candidateNextSize, 0, Output.GetMaxSize(r));
                
                // 0 = current size exactly, 1 = next size exactly
                var interp = Mathf.InverseLerp(currentSize, candidateNextSize, normSize);
                if (interp > 0.5f) // Only switch if we're closer to the next size than the current size
                {
                    // 0 = definitely stick to the current, 1 = definitely switch
                    interp = 2*(interp - 0.5f);
                    if (interp > Hysteresis)
                    {
                        if (candidateNextSize == 0) {
                            _tree.Remove(r.GetInstanceID());
                            output.Remove(r);
                        } else {
                            var newSmallness = (smallness ?? 0) + (currentSize > candidateNextSize ? +1 : -1);
                            operations.Add(ValueTuple.Create(r, currentSize, candidateNextSize, newSmallness));
                        }
                    }
                }
            }

            // This implementation ensures only one texture is uploaded per frame:
            // Pick the smallest increase. If that will fit in, do it; otherwise, do the largest decrease
            if (operations.Count == 0)
            {
                // TODO: A 'defrag' operation
                return;
            }
            operations.Sort((a, b) => (a.Item3 - a.Item2).CompareTo(b.Item3 - b.Item2));
            var smallestIncrease = operations.FirstOrDefault(v => v.Item3 > v.Item2);
            if (smallestIncrease.Item1 != null)
            {
                var id = smallestIncrease.Item1.GetInstanceID();
                _tree.Remove(id);
                var v = _tree.Pack(smallestIncrease.Item4, id, true);
                if (v.HasValue)
                {
                    Output.Copy(smallestIncrease.Item1, v.Value, smallestIncrease.Item3);
                    return;
                }
            }
            var largestDecrease = operations.FirstOrDefault(v => v.Item3 < v.Item2 && v.Item3 > 0);
            if (largestDecrease.Item1 != null)
            {
                var id = largestDecrease.Item1.GetInstanceID();
                _tree.Remove(id);
                var v = _tree.Pack(smallestIncrease.Item4, id, true);
                if (!v.HasValue)
                {
                    Debug.LogAssertion("Unable to pack a smaller texture just after removing a larger one?");
                    return;
                }
                Output.Copy(largestDecrease.Item1, v.Value, largestDecrease.Item3);
            }


        }
    }
}