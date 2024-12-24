using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ARP : RenderPipeline
{
    private CameraRender _cameraRender = new CameraRender();
    private bool _enableDynamicBatch;
    public ARP(bool enableSRPBatch, bool enableDynamicBatch)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = enableSRPBatch;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        for (int n = 0; n < cameras.Length; n++)
        {
            Camera camera = cameras[n];
            _cameraRender.Render(ref context, camera,_enableDynamicBatch );
        }
    }
}
