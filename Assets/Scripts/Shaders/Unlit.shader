Shader "ARP/Unlit"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "LightMode" = "ARPUnlit"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #include "Lib/Unlit.hlsl"
            
            #pragma vertex VertexProgram
            #pragma fragment FragProgram
            
            
            ENDHLSL
        }
    }
}
