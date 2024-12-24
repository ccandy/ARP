#ifndef ARP_SURFACE_INCLUDE
#define ARP_SURFACE_INCLUDE

struct Surface
{
    float3 albedo;
    float3 normal;
    float roughness;
    float metallic; 
    
    float alpha;
};

Surface GetSurface(float3 albedo, float3 normal, float alpha, float roughess, float metallic)
{
    Surface surface;

    surface.albedo = albedo;
    surface.normal = normal;
    surface.alpha = alpha;
    surface.metallic = metallic;
    surface.roughness = roughess;

    return surface;
}




#endif