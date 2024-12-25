#ifndef ARP_SURFACE_INCLUDE
#define ARP_SURFACE_INCLUDE

struct Surface
{
    float3 albedo;
    float3 normal;
    float3 viewdirection;
    float perceptualroughness;
    float metallic; 
    
    float alpha;
};




#endif