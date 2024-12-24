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
        directionalLightDirsId = Shader.PropertyToID("_directionalLightDirs"),
        directionalLightCountId = Shader.PropertyToID("directionalLightCount");
    
    private int dirLightCount = 0;


    private DirectionalLightsData directionalLights = new DirectionalLightsData(MAXDIRECTIONALLIGHTS);
    
    public void Setup(ref ScriptableRenderContext context, ref CullingResults cullingResults)
    {
        if (context == null || cullingResults == null)
        {
            return;
        }
        
        _cullingResults = cullingResults;
        _context = context;
        
        RPUtil.BeginSample(ref context, lightBuffer);
        SetupDirectionalLights();
        RPUtil.EndSample(ref context, lightBuffer);
    }
    
    public void SetupDirectionalLights()
    {
        NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
        
        for (int n = 0; n < visibleLights.Length; n++)
        {
            VisibleLight visibleLight = visibleLights[n];

            if (n >= MAXDIRECTIONALLIGHTS)
            {
                break;
            }
            directionalLights.DirectionalLightDirs[n] = -visibleLight.localToWorldMatrix.GetColumn(2);
            directionalLights.DirectionalLightColors[n] = visibleLight.finalColor;
            dirLightCount = n;
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
    

}
