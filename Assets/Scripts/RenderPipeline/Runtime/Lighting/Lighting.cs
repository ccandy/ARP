using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const int MAXDIRECTIONALLIGHTS = 4;
    
    private const string bufferName = "Lighting buffer";

    private CommandBuffer lightBuffer = new CommandBuffer()
    {
        name = bufferName
    };

    private CullingResults _cullingResults;
    private ScriptableRenderContext _context;

    private static int
        directionalLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        directionalLightDirsId = Shader.PropertyToID("_DirectionalLightDirs"),
        directionalLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    
    private int dirLightCount = 0;
    
    private DirectionalLightsData directionalLights = new DirectionalLightsData(MAXDIRECTIONALLIGHTS);

    private Shadow _shadow = new Shadow();
    
    public void Setup(ref ScriptableRenderContext context, ref CullingResults cullingResults, ShadowGlobalSettings shadowGlobalSettings)
    {
        if (context == null || cullingResults == null)
        {
            return;
        }
        
        _cullingResults = cullingResults;
        _context = context;
        
        RPUtil.BeginSample(ref context, lightBuffer);
        _shadow.Setup(context, ref cullingResults, shadowGlobalSettings);
        SetupLights();
        _shadow.Render(ref _cullingResults);
        RPUtil.EndSample(ref context, lightBuffer);
    }
    
    public void SetupLights()
    {
        SetupDirectionalLights();
    }
    
    public void SetupDirectionalLights()
    {
        NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
        
        for (int n = 0; n < visibleLights.Length; n++)
        {
            VisibleLight visibleLight = visibleLights[n];
            if (visibleLight.lightType == LightType.Directional)
            {
                if (dirLightCount >= MAXDIRECTIONALLIGHTS)
                {
                    break;
                }
                directionalLights.DirectionalLightDirs[dirLightCount] = -visibleLight.localToWorldMatrix.GetColumn(2);
                directionalLights.DirectionalLightColors[dirLightCount] = visibleLight.finalColor;
                dirLightCount++;
            }
        }
        SendDataToGPU();
    }

    private void SendDataToGPU()
    {
        lightBuffer.SetGlobalInt(directionalLightCountId, dirLightCount);
        lightBuffer.SetGlobalVectorArray(directionalLightColorsId, directionalLights.DirectionalLightColors);
        lightBuffer.SetGlobalVectorArray(directionalLightDirsId,directionalLights.DirectionalLightDirs);
        
        RPUtil.ExecuteBuffer(ref _context, lightBuffer);
        
    }

    public void CleanUP()
    {
        dirLightCount = 0;
        int i = 0;
        for (; i < directionalLights.DirectionalLightColors.Length; i++)
        {
            directionalLights.DirectionalLightColors[i] = Vector4.zero;
        }

        i = 0;
        for (; i < directionalLights.DirectionalLightColors.Length; i++)
        {
            directionalLights.DirectionalLightColors[i] = Vector4.zero;
        }
        
        _shadow.CleanUP();
    }

    

}
