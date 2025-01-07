#ifndef ARP_SHADOWCASTER_INCLUDE
#define ARP_SHADOWCASTER_INCLUDE

#include "Assets/Scripts/Shaders/Lib/Common.hlsl"
#include "Assets/Scripts/Shaders/Lib//Surface.hlsl"


struct VertexInput
{
    float4 positionOS : POSITION;
    float2 uv: TEXCOORD0;
};

struct VertexOutput
{
    float4 positionCS : SV_POSITION;
    float2 uv:VAR_BASE_UV;
};

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

CBUFFER_START(UnityPerMaterial)
    float _Cutoff;
    float4 _MainTex_ST;
CBUFFER_END

VertexOutput ShadowCasterVertexProgram (VertexInput input)
{
    VertexOutput output;

    float3 posWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(posWS);
    output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
    return output;
}

half4 ShadowCasterFragProgram (VertexOutput input) : SV_Target
{
    return 0;
}

#endif
