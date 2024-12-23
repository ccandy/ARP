#ifndef ARP_UNLIT_INCLUDE
#define ARP_UNLIT_INCLUDE

#include "Lib/Common.hlsl"

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
    float4 _Color;
    float4 _MainTex_ST;
CBUFFER_END


VertexOutput VertexProgram (VertexInput input)
{
    VertexOutput output;

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);
    output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
    return output;
}

half4 FragProgram (VertexOutput input) : SV_Target
{
    // sample the texture
    half4 col = _Color;
    half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    half4 finalCol = col * texCol;
    return finalCol;
}


#endif
