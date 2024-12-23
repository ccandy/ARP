#ifndef ARP_UNLIT_INCLUDE
#define ARP_UNLIT_INCLUDE

#include "Lib/Common.hlsl"

struct VertexInput
{
    float4 positionOS : POSITION;
};

struct VertexOutput
{
    float4 positionCS : SV_POSITION;
};

CBUFFER_START(UnityPerMaterial)
    float4 _Color;
CBUFFER_END


VertexOutput VertexProgram (VertexInput input)
{
    VertexOutput output;

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);
    return output;
}

half4 FragProgram (VertexOutput i) : SV_Target
{
    // sample the texture
    half4 col = _Color;
    return col;
}


#endif
