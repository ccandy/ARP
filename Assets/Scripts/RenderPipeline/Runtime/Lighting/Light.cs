using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light 
{
    
}

public struct DirectionalLightsData
{
    public Vector4[] DirectionalLightColors;
    public Vector4[] DirectionalLightDirs;

    public DirectionalLightsData(int count)
    {
        DirectionalLightColors = new Vector4[count];
        DirectionalLightDirs = new Vector4[count];
    }
}

