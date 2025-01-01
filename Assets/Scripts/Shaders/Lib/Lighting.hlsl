#ifndef ARP_LIGHTING_INCLUDE
#define ARP_LIGHTING_INCLUDE


float3 GetDirectionalLighting(Surface surface, BRDF brdf, Light light)
{
    float3 V = surface.viewdirection;
    float3 L = normalize(light.direction); // Example light direction
    float3 H = normalize(V + L);
    float3 N = surface.normal;
    float HdotV = dot(H,V);
    
    float3 diffuse = GetBRDFDiffuse(brdf, HdotV);
    float3 specular = GetBRDFSpecular(brdf, H, L, surface);
    
    float NdotL = max(dot(N, L), 0.0);
    float3 finalCol = (diffuse + specular) * NdotL * light.color;
    
    return finalCol;
}

float3 GetIncomingLights(Surface surface, BRDF brdf)
{
    int dirlightCount = GetDirectionalLightCount();
    float3 lightColor = 0;
    for(int n = 0; n < dirlightCount; n++)
    {
        Light dirLight = GetDirectionalLight(n);
        lightColor += GetDirectionalLighting(surface, brdf, dirLight) ;
    }
    return lightColor;
}








#endif