

#define VT0_def(TEX) uniform sampler2D VT0##TEX; uniform float4 VT0_pos##TEX;
#include "VT0_channels.cginc"

#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)
#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) tex.SampleGrad (sampler##tex,coord,dx,dy)
#else
#if defined(UNITY_COMPILER_HLSL2GLSL) || defined(SHADER_TARGET_SURFACE_ANALYSIS)
#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) tex2DArray(tex,coord,dx,dy)
#endif
#endif

#ifdef _VT0_ON

float4 VT0_sample_explicit(sampler2D small, sampler2D vt, float4 pos, float2 uv)
{
    return (pos.w <= 0 ? tex2D(small, uv) : tex2D(vt, (saturate(uv) * pos.zw) + pos.xy));
}

#define VT0_sample(TEX, UV) VT0_sample_explicit(TEX, VT0##TEX, VT0_pos##TEX, (UV))

#else
#define VT0_sample(TEX, UV) tex2D(TEX, (UV))
#endif
