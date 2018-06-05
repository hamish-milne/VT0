using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;

namespace VT0
{
    public interface ICopyTextureStrategy
    {
        bool Supported { get; }
        void Copy(
            byte[] src, int srcWidth, int srcHeight, int srcMipBias,
            Texture2D dst, int dstElement, int dstX, int dstY, int dstMipLimit);
    }

    public class CopyTextureStrategy : ICopyTextureStrategy
    {
        public bool Supported => (SystemInfo.copyTextureSupport & CopyTextureSupport.Basic) != 0;

        private static readonly Dictionary<ValueTuple<int, int, TextureFormat>, Texture2D> _staging
            = new Dictionary<ValueTuple<int, int, TextureFormat>, Texture2D>();

        public void Copy(
            byte[] src, int srcWidth, int srcHeight, int srcMipBias,
            Texture2D dst, int dstElement, int dstX, int dstY, int dstMipLimit)
        {
            var stagingKey = ValueTuple.Create(srcWidth, srcHeight, dst.format);
            Texture2D staging;
            _staging.TryGetValue(stagingKey, out staging);
            if (staging == null) {
                _staging[stagingKey] =
                    (staging = new Texture2D(srcWidth, srcHeight, dst.format, true));
            }
            // TODO: Only stage the mips we need?
            staging.LoadRawTextureData(src);

            for (int m = 0; m < dstMipLimit; m++)
            {
                Graphics.CopyTexture(
                    staging, 0, m + srcMipBias, 0, 0, srcWidth, srcHeight,
                    dst, 0, m, dstX, dstY
                    );
            }
        }
    }

    public class DataCopyTextureStrategy : ICopyTextureStrategy
    {
        public bool Supported => true;

        private ConditionalWeakTable<Texture2D, byte[]> _cache
            = new ConditionalWeakTable<Texture2D, byte[]>();
        
        private static ConditionalWeakTable<Texture2D, byte[]>.CreateValueCallback
            _getData = t => t.GetRawTextureData();
        
        private int GetMipOffset(int width, int height, int mip, int bPixels)
        {
            var offset = 0;
            for (int i = 0; i < mip; i++) {
                var ratio = (1 << i);
                offset += ((width / ratio) / bPixels) * ((height / ratio) / bPixels);
            }
            return offset;
        }

        public void Copy(
            byte[] src, int srcWidth, int srcHeight, int srcMipBias,
            Texture2D dst, int dstElement, int dstX, int dstY, int dstMipLimit)
        {
            var dstData = _cache.GetValue(dst, _getData);

            var fmtInfo = TextureFormatInfo.FromFormatID(dst.format);
            Debug.Assert(fmtInfo.HasValue);
            var bPixels = fmtInfo.Value.BlockPixelCount;
            var bBytes = fmtInfo.Value.BlockDataSize;

            for (int m = 0; m < dstMipLimit; m++)
            {
                var srcOffset = GetMipOffset(srcWidth, srcHeight, m + srcMipBias, bPixels);
                var dstOffset = GetMipOffset(dst.width, dst.height, m, bPixels);

                for (int y = 0; y < srcHeight; y += bPixels) {
                    Buffer.BlockCopy(
                        src,
                        (srcOffset + (y/bPixels) * (srcWidth/bPixels)) * bBytes,
                        dstData,
                        (dstOffset + ( ((dstY + y)/bPixels) * (dst.width/bPixels) + (dstX/bPixels) )) * bBytes,
                        (srcWidth/bPixels) * bBytes
                    );
                }
            }

            dst.LoadRawTextureData(dstData);
            dst.Apply(false, false);
        }
    }
}   