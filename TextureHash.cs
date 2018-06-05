using UnityEngine;
using System.Runtime.CompilerServices;

namespace VT0
{
    public static class TextureHash
    {
        private static ConditionalWeakTable<Texture2D, object> _cache
            = new ConditionalWeakTable<Texture2D, object>();
        

        private static ConditionalWeakTable<Texture2D, object>.CreateValueCallback
            _getHash = t => xxHashSharp.xxHash.CalculateHash(t.GetRawTextureData());

        public static uint GetImageHash(this Texture2D tex)
        {
            return (uint)_cache.GetValue(tex, _getHash);
        }
    }
}
