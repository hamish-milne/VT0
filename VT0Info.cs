using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VT0
{
    [Serializable] public struct Empty { }

    public class TextureSizeAttribute : PropertyAttribute { }

    [System.Serializable]
    public class VT0Channel
    {
        public List<string> TextureNames = new List<string>();

        public TextureFormat Format;

        public bool IsTextureValid(Texture tex, out string why)
        {
            if (tex == null) {
                why = "No texture selected";
                return false;
            }
            var tex2d = tex as Texture2D;
            if (tex2d == null) {
                why = "Only 2D textures are supported";
                return false;
            }
            if (tex2d.format != Format) {
                why = string.Format(
                    "Selected texture is in the {0} format, but this channel requires {1}",
                    tex2d.format, Format);
                return false;
            }
            if (tex2d.mipmapCount < Mathf.Log(tex.width / VT0Info.Current.ThumbSize)) {
                why = "Texture doesn't have enough mipmaps. " +
                    "It could be too small, not a power of 2, or have mipmaps disabled in import.";
                return false;
            }
            why = null;
            return true;
        }
    }

    public class VT0Info : ScriptableObject
    {
        public List<VT0Channel> Channels = new List<VT0Channel>
        {
            new VT0Channel { TextureNames = {"_MainTex"} },
            new VT0Channel { TextureNames = {"_BumpMap"} },
            new VT0Channel { TextureNames = {"_MetallicMap"} }
        };

        [TextureSize] public int VTSize = 16384;
        [TextureSize] public int ThumbSize = 64;

        public float EstimateVRAM(int materialCount)
        {
            return Channels.Sum(c => {
                var fmt = TextureFormatInfo.FromFormatID(c.Format);
                if (fmt == null) return 0;
                return (4.0f/3.0f) *
                    (((float)VTSize*VTSize + materialCount*ThumbSize*ThumbSize)
                    * fmt.Value.BlockDataSize)
                    /(fmt.Value.BlockPixelCount*fmt.Value.BlockPixelCount);
            });
        }

        public class EstimateVRAMAttribute : PropertyAttribute { }
        [EstimateVRAM] private Empty _editorMarker0;

        private static VT0Info _current;
        public static VT0Info Current {
            get {
                if (_current == null) {
                    _current = Resources.Load<VT0Info>("VT0Info");
                }
                Create(ref _current);
                return _current;
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void Create(ref VT0Info obj)
        {
            if (obj == null) {
                System.IO.Directory.CreateDirectory("Assets/Resources");
                UnityEditor.AssetDatabase.CreateAsset(
                    obj = CreateInstance<VT0Info>(), "Assets/Resources/VT0Info.asset");
            }
        }
    }
}
