#ifndef ARP_LIGHTING_INCLUDE
#define ARP_LIGHTING_INCLUDE


float3 GetDirectionalLighting(Surface surface, BRDF brdf, Light light)
{
    float3 V = surface.viewdirection;
    float3 L = normalize(light.direction); // Example light direction
    float3 H = normalize(V + L);
    
    float3 diffuse = GetBRDFDiffuse(surface.albedo, brdf.metallic, H) * light.color;

    return light.color;;
}

float3 GetIncomingLights(Surface surface, BRDF brdf)
{
    int dirlightCount = GetDirectionalLightCount();
    float lightColor = 0;
    for(int n = 0; n < dirlightCount; n++)
    {
        Light dirLight = GetDirectionalLight(n);
        lightColor += GetDirectionalLighting(surface, brdf, dirLight) ;
    }
    return lightColor;
}








#endif