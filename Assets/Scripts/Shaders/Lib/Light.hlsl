#ifndef ARP_LIGHT_INCLUDE
#define ARP_LIGHT_INCLUDE


#define MAX_DIRECTIONAL_LIGHTS 4

CBUFFER_START(CustomLight)

float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHTS];
float4 _DirectionalLightDirs[MAX_DIRECTIONAL_LIGHTS];
int _DirectionalLightCount;

CBUFFER_END

struct Light
{
    float3 color;
    float3 direction;
};


Light GetDirectionalLight(int n)
{
    Light light;

    light.color = _DirectionalLightColors[n].rgb;
    light.direction = _DirectionalLightDirs[n].rgb;
    
    return light;
}

int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}


#endif