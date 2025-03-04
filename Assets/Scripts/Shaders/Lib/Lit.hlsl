#ifndef ARP_LIT_INCLUDE
#define ARP_LIT_INCLUDE

#include "Lib/Common.hlsl"
#include "Lib/Surface.hlsl"
#include "Lib/Light.hlsl"
#include "Lib/BRDF.hlsl"
#include "Lib/Lighting.hlsl"
#include "Lib/Shadow.hlsl"

struct VertexInput
{
    float4 positionOS : POSITION;
    float2 uv: TEXCOORD0;
    float3 normal:NORMAL;
};

struct VertexOutput
{
    float4 positionCS : SV_POSITION;
    float3 positionWS: VAR_POSITION;
    float3 viewDirection:TEXCOORD1;
    float2 uv:VAR_BASE_UV;
    float3 normal:TEXCOORD0;
};

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

float3 _WorldSpaceCameraPos;

CBUFFER_START(UnityPerMaterial)
    float4 _Color;
    float4 _MainTex_ST;
    float _Cutoff;
    float _Roughness;
    float _Metallic;
CBUFFER_END

VertexOutput VertexProgram (VertexInput input)
{
    VertexOutput output;

    output.positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
    output.normal = TransformObjectToWorldNormal(input.normal);
    return output;
}


half4 FragProgram (VertexOutput input) : SV_Target
{
    half4 col = _Color;
    half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    half4 finalCol = col * texCol;

    Surface surface;
    surface.albedo = finalCol.rgb;
    surface.alpha = finalCol.a;
    surface.normal = normalize(input.normal);
    surface.metallic = _Metallic;
    surface.perceptualroughness = _Roughness;
    surface.viewdirection = normalize(_WorldSpaceCameraPos - input.positionWS);

    BRDF brdf = GetBRDF(surface);
    
    half3 diffuse = GetIncomingLights(surface, brdf) ;
    
    clip(surface.alpha - _Cutoff);
    return float4(diffuse, surface.alpha);
}



#endif