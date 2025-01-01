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
    }

    public Directional DirectionalShadowSetting = new Directional
    {
        AtlasSize = ShadowMapSize._1028
    };
    

}
