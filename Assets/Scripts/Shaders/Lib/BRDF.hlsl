#ifndef ARP_BRDF_INCLUDE
#define ARP_BRDF_INCLUDE

#define PI 3.1415926
#define MAX_REFLECTIVITY 0.04

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"


float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float3 FresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}

float GetF0(float3 albedo, float metallic)
{
    return lerp(float3(MAX_REFLECTIVITY, MAX_REFLECTIVITY, MAX_REFLECTIVITY), albedo, metallic);
}

struct BRDF
{
    float3 diffuse;
    float3 specular;
    float roughness;
    float metallic;
};

BRDF GetBRDF(Surface surface)
{
    BRDF brdf;

    brdf.diffuse = surface.albedo;
    brdf.specular = 1;
    brdf.roughness = PerceptualRoughnessToRoughness(surface.perceptualroughness);
    brdf.metallic = surface.metallic;

    return brdf;
}

float3 GetBRDFDiffuse(BRDF brdf, float cosTheta)
{
    float3 albedo = brdf.diffuse;
    float metallic = brdf.metallic;
    float F0 = GetF0(albedo, metallic);
    float F = FresnelSchlick(cosTheta, F0);

    float3 kd = (1.0 - F) * (1 - metallic);
    float3 diffuse = kd * albedo/PI;

    return diffuse;
}

float3 GetBRDFSpecular()
{
    
}







#endif