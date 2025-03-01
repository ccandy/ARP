Shader "ARP/Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutoff", Range(0.0,1.0)) = 0.5
        _Roughness("Roughness", float) = 0.5
        _Metallic("Metallic", float) = 0.5
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
    }
    SubShader
    {
        
        LOD 100

        Pass
        {
            Tags 
            { 
                "RenderType"="Opaque" 
                "LightMode" = "ARPUnlit"
            }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #include "Lib/Lit.hlsl"
            
            #pragma vertex VertexProgram
            #pragma fragment FragProgram
            
            
            ENDHLSL
        }

        Pass
        {
            Tags 
            { 
                "RenderType"="Opaque" 
                "LightMode" = "ShadowCaster"
            }
            
            ColorMask 0
            HLSLPROGRAM
            #include "Lib/ShadowCaster.hlsl"
             #pragma vertex ShadowCasterVertexProgram
            #pragma fragment ShadowCasterFragProgram
            ENDHLSL
        }

    }
}
