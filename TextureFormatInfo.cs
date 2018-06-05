using UnityEngine;

namespace VT0
{
    public struct TextureFormatInfo
    {
        public int BlockPixelCount { get; }
        public int BlockDataSize { get; }

        private TextureFormatInfo(int blockPixelCount, int blockDataSize)
        {
            BlockPixelCount = blockPixelCount;
            BlockDataSize = blockDataSize;
        }

        public static TextureFormatInfo? FromFormatID(TextureFormat fmt)
        {
            switch (fmt)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.R8:
                    return new TextureFormatInfo(1, 1);
                case TextureFormat.ARGB4444:
                case TextureFormat.RGBA4444:
                case TextureFormat.RGB565:
                case TextureFormat.R16:
                case TextureFormat.RG16:
                case TextureFormat.RHalf:
                    return new TextureFormatInfo(1, 2);
                case TextureFormat.RGB24:
                    return new TextureFormatInfo(1, 3);
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGB9e5Float:
                    return new TextureFormatInfo(1, 4);
                case TextureFormat.DXT1:
                case TextureFormat.BC4:
                case TextureFormat.ETC_RGB4:
                //case TextureFormat.ATC_RGB4:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC_RGB4_3DS:
                    return new TextureFormatInfo(4, 8);
                case TextureFormat.DXT5:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                //case TextureFormat.ATC_RGBA8:
                case TextureFormat.EAC_RG:
                case TextureFormat.EAC_RG_SIGNED:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ETC_RGBA8_3DS:
                case TextureFormat.ASTC_RGB_4x4:
                case TextureFormat.ASTC_RGBA_4x4:
                    return new TextureFormatInfo(4, 16);
                case TextureFormat.RGBAHalf:
                case TextureFormat.RGFloat:
                    return new TextureFormatInfo(1, 8);
                case TextureFormat.RGBAFloat:
                    return new TextureFormatInfo(1, 16);
                case TextureFormat.YUY2:
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5Crunched:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4Crunched:
                case TextureFormat.ETC2_RGBA8Crunched:
                //case TextureFormat.PVRTC_2BPP_RGB:
                //case TextureFormat.PVRTC_2BPP_RGBA:
                //case TextureFormat.PVRTC_4BPP_RGB:
                //case TextureFormat.PVRTC_4BPP_RGBA:
                default:
                    return null;
                case TextureFormat.ETC2_RGBA1:
                    return new TextureFormatInfo(4, 10);
                case TextureFormat.ASTC_RGB_5x5:
                case TextureFormat.ASTC_RGBA_5x5:
                    return new TextureFormatInfo(5, 16);
                case TextureFormat.ASTC_RGB_6x6:
                case TextureFormat.ASTC_RGBA_6x6:
                    return new TextureFormatInfo(6, 16);
                case TextureFormat.ASTC_RGB_8x8:
                case TextureFormat.ASTC_RGBA_8x8:
                    return new TextureFormatInfo(8, 16);
                case TextureFormat.ASTC_RGB_10x10:
                case TextureFormat.ASTC_RGBA_10x10:
                    return new TextureFormatInfo(10, 16);
                case TextureFormat.ASTC_RGB_12x12:
                case TextureFormat.ASTC_RGBA_12x12:
                    return new TextureFormatInfo(12, 16);
            }
        }
    }
}