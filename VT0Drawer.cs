#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace VT0
{
    [CustomPropertyDrawer(typeof(TextureSizeAttribute))]
    public class TextureSizeDrawer : PropertyDrawer
    {
        private static readonly int[] sizes = Enumerable.Range(4, 12)
            .Select(i => (int)Mathf.Pow(2, i)).ToArray();
        private static GUIContent[] sizeNames;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * (property.intValue > SystemInfo.maxTextureSize ? 3 : 1);
        }

        public override void OnGUI(Rect p, SerializedProperty property, GUIContent label)
        {
            var line = EditorGUIUtility.singleLineHeight;
            if (sizeNames == null) {
                sizeNames = sizes.Select(i => new GUIContent(i.ToString())).ToArray();
            }
            EditorGUI.IntPopup(new Rect(p.x, p.y, p.width, line),
                property, sizeNames, sizes);
            if (property.intValue > SystemInfo.maxTextureSize)
            {
                EditorGUI.HelpBox(new Rect(p.x, p.y + line, p.width, line*2),
                    string.Format("Max texture size for this platform is {0}", SystemInfo.maxTextureSize),
                    MessageType.Error);
            }
        }
    }

    public class VT0Drawer : MaterialPropertyDrawer
    {
        public static Rect WithHeight(ref Rect r, float height)
        {
            var ret = new Rect(r.x, r.y, r.width, height);
            r.y += height;
            return ret;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            var channel = VT0Info.Current.Channels.FirstOrDefault(c => c.TextureNames.Contains(prop.name));
            string why;
            var hasError = channel == null || !channel.IsTextureValid(prop.textureValue, out why);
            return EditorGUIUtility.singleLineHeight + (hasError ? 24f : 0f);
        }

        private bool IsVT0Enabled(Material m)
        {
            return m.IsKeywordEnabled("_VT0ENABLE_ON");
        }

        private static readonly TwoWayDictionary<Texture2D, Texture2D> _placeholderCache
            = new TwoWayDictionary<Texture2D, Texture2D>();

        public const string PlaceholderLocation = "Assets/VT0_thumbnails";

        private static Texture2D GetPlaceholder(Texture2D t, int size)
        {
            if (t == null) return null;
            Texture2D placeholder;
            if (!_placeholderCache.TryGetValue1(t, out placeholder))
            {
                var path = PlaceholderLocation + "/" +
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(t)) + ".png";
                placeholder = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (placeholder == null || placeholder.width != t.width)
                {
                    var tmp = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
                    tmp.SetPixels32(placeholder.GetPixels32((int)Mathf.Log(t.width / size, 2)));
                    File.WriteAllBytes(path, tmp.EncodeToPNG());
                    AssetDatabase.Refresh();
                    placeholder = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                _placeholderCache.Add(t, placeholder);
            }
            return placeholder;
        }

        public static Texture2D GetOriginal(Texture2D t)
        {
            if (t == null) return null;
            Texture2D original;
            if (!_placeholderCache.TryGetValue2(t, out original))
            {
                var placeholderPath = AssetDatabase.GetAssetPath(t);
                if (Path.GetDirectoryName(placeholderPath).Replace('\\', '/') != PlaceholderLocation)
                {
                    original = t;
                } else {
                    var path = AssetDatabase.GUIDToAssetPath(
                        Path.GetFileNameWithoutExtension(placeholderPath));
                    if (string.IsNullOrEmpty(path)) {
                        Debug.LogWarning("Original texture has gone!");
                        AssetDatabase.DeleteAsset(placeholderPath);
                        return null;
                    } else {
                        original = AssetDatabase.LoadAssetAtPath<Texture2D>(path); 
                    }
                }
                _placeholderCache.Add(original, t);
            }
            return original;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            var enable = IsVT0Enabled((Material)prop.targets[0]);
            var orig = prop.textureValue as Texture2D;
            prop.textureValue = GetOriginal(orig);
            editor.TexturePropertyMiniThumbnail(
                WithHeight(ref position, EditorGUIUtility.singleLineHeight),
                prop, label, "");
            if (enable) {
                prop.textureValue = GetPlaceholder(orig, 64);
            }
            var channel = VT0Info.Current.Channels.FirstOrDefault(c => c.TextureNames.Contains(prop.name));
            string why = null;
            if (channel == null || !channel.IsTextureValid(prop.textureValue, out why)) {
                EditorGUI.HelpBox(WithHeight(ref position, 24f),
                    why ?? ("Unknown channel for property " + prop.name), MessageType.Error);
            }
        }
    }
}
#endif
