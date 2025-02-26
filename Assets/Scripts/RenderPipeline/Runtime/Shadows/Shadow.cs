using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadow
{
    private static int dirShadowAtlasId = Shader.PropertyToID("_DIRECTIONAL_SHADOWMAP");
    private static int cullingSpheresId = Shader.PropertyToID("_CullingSphere");
    private static int dirWorldToShadowMatricsId = Shader.PropertyToID("_DirWorldToShadowMatrix");
    private static int dirShadowSettingsId = Shader.PropertyToID("_DirShadowSettings");
    
    private const string BUFFERNAME = "Shadow Buffer";
    private const int MAX_DIRECTION_SHADOW_COUNT = 4;
    private const int MAX_CASCADE_COUNT = 4;
    
    private int ShadowedDirectionalLightCount;
    private Matrix4x4[] _dirViewProjectionMatrices = new Matrix4x4[MAX_DIRECTION_SHADOW_COUNT * MAX_CASCADE_COUNT];
    private Vector4[] _cullingSpheres = new Vector4[MAX_DIRECTION_SHADOW_COUNT];
    private Vector4[] _dirShadowSettings = new Vector4[MAX_DIRECTION_SHADOW_COUNT];
    
    
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

                Vector4 dirShadowSetting = new Vector4();
                dirShadowSetting.x = n;
                dirShadowSetting.y = light.shadowStrength;
                dirShadowSetting.z = light.shadowNearPlane;
                _dirShadowSettings[n] = dirShadowSetting;

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
            Vector2 offset = SetTileViewport(index * cascadeCount  + n , split, tileSize);
            shadowBuffer.SetViewProjectionMatrices(viewMat,projMat);
            RPUtil.ExecuteBuffer(ref _context, shadowBuffer);
            _context.DrawShadows(ref shadowDrawSetting);

            if (index == 0)
            {
                Vector4 cullingSphere = shadowSplitData.cullingSphere;
                cullingSphere.w *= cullingSphere.w;
                _cullingSpheres[n] = cullingSphere;
            }
            Matrix4x4 projViewMat = projMat * viewMat;
            Matrix4x4 worldToShadowMat = ConvertMatrix(projViewMat, split, offset);
            
            _dirViewProjectionMatrices[index * cascadeCount + n] = worldToShadowMat;
        }
        
        SendToGPU();
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
        shadowBuffer.SetGlobalMatrixArray(dirWorldToShadowMatricsId, _dirViewProjectionMatrices);
        shadowBuffer.SetGlobalVectorArray(cullingSpheresId, _cullingSpheres);
        shadowBuffer.SetGlobalVectorArray(dirShadowSettingsId, _dirShadowSettings);
        
        _context.ExecuteCommandBuffer(shadowBuffer);
        shadowBuffer.Clear();
    }

    private Matrix4x4 ConvertMatrix(Matrix4x4 m, int split, Vector2 offset)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        
        float scale = 1.0f / split;
        
        //x
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;;
        //y
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale; 
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;; 
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;; 
        //z
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        
        return m;
    }
    
    public void CleanUP()
    {
        shadowBuffer.ReleaseTemporaryRT(dirShadowAtlasId);
        RPUtil.ExecuteBuffer(ref _context, shadowBuffer);


        int n = 0;

        for (; n < MAX_CASCADE_COUNT; n++)
        {
            _cullingSpheres[n] = Vector4.zero;
        }

        for (; n < _dirViewProjectionMatrices.Length; n++)
        {
            _dirViewProjectionMatrices[n] = Matrix4x4.identity;
        }
    }
    
    private Vector2 SetTileViewport(int index, int split,int tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        shadowBuffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        RPUtil.ExecuteBuffer(ref _context, shadowBuffer);

        return offset;
    }
    
    

}
