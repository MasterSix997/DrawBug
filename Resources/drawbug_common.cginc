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

