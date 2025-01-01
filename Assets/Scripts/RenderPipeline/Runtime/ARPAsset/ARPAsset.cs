using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/ARP Asset")]
public class ARPAsset : RenderPipelineAsset
{
    [SerializeField]
    public bool EnableSRPBatch = true;
    [SerializeField]
    public bool EnableDynamicBatch = false;
    [SerializeField]
    ShadowGlobalSettings ShadowSettings = default;
    
    
    protected override RenderPipeline CreatePipeline()
    {
        return new ARP(EnableSRPBatch,EnableDynamicBatch, ShadowSettings);
    }
}
