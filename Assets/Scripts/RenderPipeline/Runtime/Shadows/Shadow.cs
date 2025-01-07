using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadow
{
    private static int dirShadowAtlasId = Shader.PropertyToID("_DIRECTIONAL_SHADOWMAP");
    
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
        public float shadowLightNearPlane;
    }


    private LocalDirectionalShadowSetting[] _localDirectionalShadowSetting =
        new LocalDirectionalShadowSetting[MAX_DIRECTION_SHADOW_COUNT];

    private ShadowGlobalSettings _shadowGlobalSettings;

    private ScriptableRenderContext _context;
    
    public void Setup(ScriptableRenderContext context, ref CullingResults cullingResults, ShadowGlobalSettings shadowGlobalSettings)
    {
        _shadowGlobalSettings = shadowGlobalSettings;
        _context = context;
        
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
                    ShadowStrength = light.shadowStrength,
                    shadowLightNearPlane = light.shadowNearPlane
                };
            } 
        }
    }

    public void Render(ref CullingResults cullingResults)
    {
        if (ShadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows(ref cullingResults);
        }
    }

    private void RenderDirectionalShadow(ref CullingResults cullingResults, int tileSize, int split,int cascadeCount,Vector3 rationRatio, int index)
    {
        LocalDirectionalShadowSetting localDirectionalShadowSetting = _localDirectionalShadowSetting[index];
        int visualIndex = localDirectionalShadowSetting.VisiualLightIndex;
        float nearPlane = localDirectionalShadowSetting.shadowLightNearPlane;

        for (int n = 0; n < cascadeCount; n++)
        {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(visualIndex, n, cascadeCount, rationRatio,
                tileSize, nearPlane,
                out Matrix4x4 viewMat, out Matrix4x4 projMat, out ShadowSplitData shadowSplitData);
            var shadowDrawSetting = new ShadowDrawingSettings(cullingResults, visualIndex,
                BatchCullingProjectionType.Orthographic
            );
            shadowDrawSetting.splitData = shadowSplitData;
            SetTileViewport(index * cascadeCount  + n , split, tileSize);
            shadowBuffer.SetViewProjectionMatrices(viewMat,projMat);
            RPUtil.ExecuteBuffer(ref _context, shadowBuffer);
            _context.DrawShadows(ref shadowDrawSetting);
        }
    }
    
    private void RenderDirectionalShadows(ref CullingResults cullingResults)
    {
        int atlasSize = (int)_shadowGlobalSettings.DirectionalShadowSetting.AtlasSize;
        int cascadeCount = _shadowGlobalSettings.DirectionalShadowSetting.CascadeCount;
        Vector3 cascadeRatio = new Vector3(_shadowGlobalSettings.DirectionalShadowSetting.CascadeRatio1,
            _shadowGlobalSettings.DirectionalShadowSetting.CascadeRatio2,_shadowGlobalSettings.DirectionalShadowSetting.CascadeRatio3);

        int tiles = ShadowedDirectionalLightCount * cascadeCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        int tileSize = atlasSize / split;
        
        shadowBuffer.GetTemporaryRT(dirShadowAtlasId,atlasSize,atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        shadowBuffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        shadowBuffer.ClearRenderTarget(true, false, Color.clear);
        RPUtil.ExecuteBuffer(ref _context, shadowBuffer);
        
        for (int n = 0; n < ShadowedDirectionalLightCount; n++)
        {
            RenderDirectionalShadow(ref cullingResults, tileSize, split, cascadeCount,cascadeRatio,  n);
        }
    }

    private void SendToGPU()
    {
        
    }

    public void CleanUP()
    {
        shadowBuffer.ReleaseTemporaryRT(dirShadowAtlasId);
        RPUtil.ExecuteBuffer(ref _context, shadowBuffer);
    }
    
    void SetTileViewport(int index, int split,int tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        shadowBuffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        RPUtil.ExecuteBuffer(ref _context, shadowBuffer);
    }
    
    

}
