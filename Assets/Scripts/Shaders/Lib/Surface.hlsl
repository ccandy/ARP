#ifndef ARP_SURFACE
#define ARP_SURFACE

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