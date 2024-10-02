#ifndef UNITY_COLORSPACE_GAMMA
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#endif

struct interpolator
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

struct position_data
{
    float3 position : POSITION;
    uint data_index;
};

struct style_data
{
    float4 color : COLOR;
    bool forward;
};

StructuredBuffer<position_data> _Positions;
StructuredBuffer<style_data> _StyleData;

style_data get_style(const uint vertex_id)
{
    return _StyleData[_Positions[vertex_id].data_index];
}

float4 get_color(const uint vertex_id)
{
    #ifndef UNITY_COLORSPACE_GAMMA
    return FastSRGBToLinear(_StyleData[_Positions[vertex_id].data_index].color);
    #else
    return _StyleData[_Positions[vertex_id].data_index].color;
    #endif
}

