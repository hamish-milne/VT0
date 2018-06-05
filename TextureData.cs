using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

namespace VT0
{
    public class TextureData
    {
        public int Width { get; }
        public int Height { get; }
        public TextureFormat Format { get; }
        public byte[] Data { get; }

        public TextureData(Texture2D tex)
        {
            Width = tex.width;
            Height = tex.height;
            Format = tex.format;
            Data = tex.GetRawTextureData();
        }

        private static readonly Dictionary<Texture2D, TextureData> _cache
            = new Dictionary<Texture2D, TextureData>();

        public const string ResourcesDir = "VT0_resources";

        public static TextureData GetData(Texture2D placeholder)
        {
            TextureData data;
            if (!_cache.TryGetValue(placeholder, out data))
            {
                Texture2D obj = null;
                if (Application.isEditor) {
                    LoadFromEditor(placeholder, ref obj);
                } else {
                    var path = $"{ResourcesDir}/{placeholder.GetImageHash():x8}";
                    obj = Resources.Load<Texture2D>(path);
                }
                data = new TextureData(obj);
            }
            return data;
        }
        
        [Conditional("UNITY_EDITOR")]
        private static void LoadFromEditor(Texture2D placeholder, ref Texture2D original)
        {
            original = VT0Drawer.GetOriginal(placeholder);
        }
    }
}
