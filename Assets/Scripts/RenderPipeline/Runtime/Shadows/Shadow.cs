using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadow
{
    private const string BUFFERNAME = "Shadow Buffer";
    private const int MAX_DIRECTION_SHADOW_COUNT = 4;
    
    private int ShadowedDirectionalLightCount;
    
    private CommandBuffer shadowBuffer = new CommandBuffer
    {
        name = BUFFERNAME
    };

    struct LocalDirectionalShadowSetting
    {
        public int VisiualLightIndex;
        public float ShadowStrength;
    }


    private LocalDirectionalShadowSetting[] _localDirectionalShadowSetting =
        new LocalDirectionalShadowSetting[MAX_DIRECTION_SHADOW_COUNT];
    

    public void Setup(ScriptableRenderContext context, ref CullingResults cullingResults, ShadowGlobalSettings shadowGlobalSettings)
    {
        ShadowedDirectionalLightCount = 0;
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        for (int n = 0; n < visibleLights.Length; n++)
        {
            VisibleLight visibleLight = visibleLights[n];
            Light light = visibleLight.light;

            if (light.shadowStrength > 0 && light.shadows != LightShadows.None 
                                          && ShadowedDirectionalLightCount < MAX_DIRECTION_SHADOW_COUNT
                                          && cullingResults.GetShadowCasterBounds(n, out Bounds b))
            {
                _localDirectionalShadowSetting[ShadowedDirectionalLightCount++] = new LocalDirectionalShadowSetting
                {
                    VisiualLightIndex = n,
                    ShadowStrength = light.shadowStrength
                };
            } 
        }
    }

    public void Render()
    {
        if (ShadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadow();
        }
    }

    private void RenderDirectionalShadow()
    {
        
    }
    
    
    
    

}
