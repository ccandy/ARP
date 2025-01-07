using UnityEngine;

[System.Serializable]
public class ShadowGlobalSettings
{
    [Min(0f)] public float MaxShadowDistance = 20;
    public enum ShadowMapSize
    {
        _256 = 256,
        _512 = 512,
        _1028 = 1028,
        _2048 = 2048,
        _4096 = 4096
    }
    
    

    [System.Serializable]
    public struct Directional
    {
        public ShadowMapSize AtlasSize;
        [Range(1, 4)] public int CascadeCount;
        [Range(0f, 1f)] public float CascadeRatio1, CascadeRatio2, CascadeRatio3;


    }

    public Directional DirectionalShadowSetting = new Directional
    {
        AtlasSize = ShadowMapSize._1028,
        CascadeCount = 4,
        CascadeRatio1 = 0.1f,
        CascadeRatio2 = 0.25f,
        CascadeRatio3 = 0.5f
    };
    

}
