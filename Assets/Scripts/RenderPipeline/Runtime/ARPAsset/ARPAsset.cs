using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/ARP Asset")]
public class ARPAsset : RenderPipelineAsset
{
    public bool EnableSRPBatch = true;
    public bool EnableDynamicBatch = false;
    
    protected override RenderPipeline CreatePipeline()
    {
        return new ARP(EnableSRPBatch,EnableDynamicBatch);
    }
}
