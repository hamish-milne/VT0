using UnityEngine;
using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

namespace VT0
{
    public class VTOutput : MonoBehaviour, ITextureOutput, IMaterialFilter
    {
        public class MaterialVTInfo
        {
            private readonly Material _material;
            private readonly Dictionary<VT0Channel, ValueTuple<Texture2D, int>> _channelProperties
                = new Dictionary<VT0Channel, ValueTuple<Texture2D, int>>();

            private int? _maxSize;

            public int GetMaxSize(VTOutput output)
            {
                if (_maxSize == null) {
                    _maxSize = _channelProperties.Values.Max(t => TextureData.GetData(t.Item1).Width);
                }
                return _maxSize.Value;
            }

            public bool IsVirtualized { get { return _channelProperties.Count > 0; } }

            public MaterialVTInfo(Material m)
            {
                _material = m;
                if (!m.IsKeywordEnabled("_VT0_ON"))
                {
                    return;
                }
                foreach (var channel in VT0Info.Current.Channels)
                {
                    string foundPosName = null;
                    string foundTextureName = null;
                    foreach (var tname in channel.TextureNames)
                    {
                        var posName = "VT0_pos" + tname;
                        if (m.HasProperty(posName)) {
                            foundPosName = posName;
                            foundTextureName = tname;
                            break;
                        }
                    }
                    if (foundTextureName == null) {
                        if (channel.TextureNames.Count > 0) {
                            Debug.LogWarningFormat("Material {0} missing texture {1}",
                                m, channel.TextureNames[0]);
                        }
                    } else {
                        var tex = m.GetTexture(foundTextureName) as Texture2D;
                        if (tex != null)
                        {
                            if (tex.format != channel.Format) {
                                Debug.LogErrorFormat(
                                    "Incorrect texture format for {0} {1} (got {2}, expected {3})",
                                    m, foundTextureName, tex.format, channel.Format);
                            } else {
                                _channelProperties[channel] = ValueTuple.Create(
                                    tex, Shader.PropertyToID(foundPosName));
                            }
                        }
                    }
                }
            }

            private static readonly Dictionary<ValueTuple<int, int, TextureFormat>, Texture2D> _staging
                = new Dictionary<ValueTuple<int, int, TextureFormat>, Texture2D>();

            public void Load(Texture2D[] targets, Vector2 position, int size, VTOutput output)
            {
                var channels = VT0Info.Current.Channels;
                var outSize = output.Size;
                int srcMipBias = outSize - size;
                var vtSize = VT0Info.Current.VTSize;
                for (int i = 0; i < channels.Count; i++)
                {
                    var props = _channelProperties[channels[i]];
                    var target = targets[i];
                    var data = TextureData.GetData(props.Item1);

                    var stagingKey = ValueTuple.Create(data.Width, data.Height, data.Format);
                    Texture2D staging;
                    _staging.TryGetValue(stagingKey, out staging);
                    if (staging == null) {
                        _staging[stagingKey] =
                            (staging = new Texture2D(data.Width, data.Height, data.Format, true));
                    }
                    // TODO: Only stage the mips we need
                    staging.LoadRawTextureData(data.Data);
                    var intPos = Vector2Int.FloorToInt(position * vtSize);

                    for (int m = 0; m < target.mipmapCount; m++)
                    {
                        Graphics.CopyTexture(
                            staging, 0, m + srcMipBias, 0, 0, data.Width, data.Height,
                            target, 0, m, intPos.x, intPos.y
                            );
                    }

                    _material.SetVector(props.Item2, new Vector4(position.x, position.y,
                        (float)size/outSize,
                        (float)size/outSize)); // TODO: Rectangular texture support
                }
            }

            public void Unload()
            {
                var channels = VT0Info.Current.Channels;
                for (int i = 0; i < channels.Count; i++)
                {
                    var props = _channelProperties[channels[i]];

                    _material.SetVector(props.Item2, -Vector4.one);
                }
            }
        }

        private Dictionary<Material, MaterialVTInfo> _infoCache =
            new Dictionary<Material, MaterialVTInfo>();

        public int Size {
            get {
                return (int)Mathf.Log(VT0Info.Current.VTSize / VT0Info.Current.ThumbSize, 2);
            }
        }

        private Texture2D[] _textures;

        private void Setup()
        {
            var channels = VT0Info.Current.Channels;
            var vtSize = VT0Info.Current.VTSize;
            Array.Resize(ref _textures, channels.Count);
            //var mipMapCount = (int)Mathf.Log(TextureSize / Info.ThumbSize, 2) - 1;
            for (int i = 0; i < _textures.Length; i++)
            {
                if (_textures[i] == null || _textures[i].width != vtSize)
                {
                    Destroy(_textures[i]);
                    _textures[i] = new Texture2D(vtSize, vtSize,
                        channels[i].Format, false, false);
                }
            }
        }

        public void Copy(Object obj, Vector2 position, int size)
        {
            Setup();
            MaterialVTInfo info;
            if (!_infoCache.TryGetValue((Material)obj, out info))
            {
                info = new MaterialVTInfo((Material)obj);
                _infoCache.Add((Material)obj, info);
            }
            info.Load(_textures, position, size, this);
        }

        public int GetMaxSize(Object obj)
        {
            MaterialVTInfo info;
            if (!_infoCache.TryGetValue((Material)obj, out info))
            {
                info = new MaterialVTInfo((Material)obj);
                _infoCache.Add((Material)obj, info);
            }
            return info.GetMaxSize(this);
        }

        public void Remove(Object obj)
        {
            MaterialVTInfo info;
            if (_infoCache.TryGetValue((Material)obj, out info))
            {
                info.Unload();
            }
        }

        public bool ValidMaterial(Material m)
        {
            MaterialVTInfo info;
            if (!_infoCache.TryGetValue(m, out info))
            {
                info = new MaterialVTInfo(m);
                _infoCache.Add(m, info);
            }
            return info.IsVirtualized;
        }
    }
}