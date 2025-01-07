using System.Collections;
using System.Collections.Generic;
using TMPro;
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
                    ShadowStrength = light.shadowStrength
                };
            } 
        }
    }

    public void Render(ref CullingResults cullingResults)
    {
        if (ShadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadow(ref cullingResults);
        }
    }

    private void RenderDirectionalShadow(ref CullingResults cullingResults)
    {
        int atlasSize = (int)_shadowGlobalSettings.DirectionalShadowSetting.AtlasSize;
        shadowBuffer.GetTemporaryRT(dirShadowAtlasId,atlasSize,atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        shadowBuffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        shadowBuffer.ClearRenderTarget(true, false, Color.clear);
        RPUtil.ExecuteBuffer(ref _context, shadowBuffer);
        
        int split = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;
        
        for (int n = 0; n < ShadowedDirectionalLightCount; n++)
        {
            LocalDirectionalShadowSetting localDirectionalShadowSetting = _localDirectionalShadowSetting[n];
            int visualIndex = localDirectionalShadowSetting.VisiualLightIndex;
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(visualIndex, 0, 1, Vector3.zero,
                tileSize, 0,
                out Matrix4x4 viewMat, out Matrix4x4 projMat, out ShadowSplitData shadowSplitData);
            var shadowDrawSetting = new ShadowDrawingSettings(cullingResults, visualIndex,
                BatchCullingProjectionType.Orthographic
            );
            shadowDrawSetting.splitData = shadowSplitData;
            shadowBuffer.SetViewProjectionMatrices(viewMat,projMat);
            RPUtil.ExecuteBuffer(ref _context, shadowBuffer);
            _context.DrawShadows(ref shadowDrawSetting);
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
        Vector2 offset = new Vector2(index / split, index % split);
        shadowBuffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        RPUtil.ExecuteBuffer(ref _context, shadowBuffer);
    }
    
    

}
