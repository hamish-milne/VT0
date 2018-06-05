using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace VT0
{
    public interface IPriority
    {
        int Count { get; }
        float GetPriority(int idx);
        UnityEngine.Object GetObject(int idx);
    }

    public interface IMaterialFilter
    {
        bool ValidMaterial(Material m);
    }

    public class Priority : MonoBehaviour, IPriority
    {
        private readonly List<Material> _materials = new List<Material>();
        private readonly Dictionary<Material, float> _sizes = new Dictionary<Material, float>();
        private float _totalSize;

        private static Vector3[] _boundsPoints = {
            new Vector3(+1, +1, +1),
            new Vector3(+1, +1, -1),
            new Vector3(+1, -1, +1),
            new Vector3(+1, -1, -1),
            new Vector3(-1, +1, +1),
            new Vector3(-1, +1, -1),
            new Vector3(-1, -1, +1),
            new Vector3(-1, -1, -1),
        };

        protected void Update()
        {
            var filter = GetComponent<IMaterialFilter>();
            _materials.Clear();
            _sizes.Clear();
            var camera = Camera.main; // TODO: Multiple cameras
            var renderers = FindObjectsOfType<Renderer>(); // TODO: Caching option

            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (!r.isVisible) {
                    continue;
                }
                var bounds = r.bounds;
                var c = bounds.center;
                var e = bounds.extents;
                float maxX = 0f, maxY = 0f, minX = 0f, minY = 0f;
                foreach (var bp in _boundsPoints) {
                    var vp = camera.WorldToViewportPoint(c + Vector3.Scale(bp, e));
                    if (vp.z > 0) {
                        maxX = Mathf.Max(maxX, vp.x);
                        maxY = Mathf.Max(maxY, vp.y);
                        minX = Mathf.Max(minX, vp.x);
                        minY = Mathf.Max(minY, vp.y);
                    }
                }
                maxX = Mathf.Clamp01(maxX);
                maxY = Mathf.Clamp01(maxY);
                minX = Mathf.Clamp01(minX);
                minY = Mathf.Clamp01(minY);

                var rSize = (maxX - minX)*(maxY - minY);

                foreach (var m in GetMaterials(r))
                {
                    if (!filter.ValidMaterial(m)) {
                        continue;
                    }
                    if (!_materials.Contains(m)) { // TODO: Use a hashset?
                        _materials.Add(m);
                    }
                    float mSize;
                    _sizes.TryGetValue(m, out mSize);
                    if (rSize > mSize) { // TODO: Other operations: Fraction, Sum etc.
                        _sizes[m] = rSize;
                    }
                }
            }

            _totalSize = _materials.Sum(m => _sizes[m]);
        }

        public int Count
        {
            get { return _materials.Count; }
        }

        public float GetPriority(int idx)
        {
            return _sizes[_materials[idx]] / _totalSize;
        }

        public UnityEngine.Object GetObject(int idx)
        {
            return _materials[idx];
        }
        
        private static Dictionary<Renderer, WeakReference<Material[]>> _materialsCache
            = new Dictionary<Renderer, WeakReference<Material[]>>();

        private static Material[] GetMaterials(Renderer r)
        {
            WeakReference<Material[]> weakReference;
            Material[] materials;
            if (!_materialsCache.TryGetValue(r, out weakReference)
                || !weakReference.TryGetTarget(out materials))
            {
                materials = r.sharedMaterials;
                _materialsCache[r] = new WeakReference<Material[]>(materials);
            }
            return materials;
        }
    }
}