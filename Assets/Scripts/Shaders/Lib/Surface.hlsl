#ifndef ARP_SURFACE_INCLUDE
#define ARP_SURFACE_INCLUDE

struct Surface
{
    float3 albedo;
    float3 normal;
    float alpha;
};

Surface GetSurface(float3 albedo, float3 normal, float alpha)
{
    Surface surface;

    surface.albedo = albedo;
    surface.normal = normal;
    surface.alpha = alpha;

    return surface;
}

#endif