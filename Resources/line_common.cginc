struct interpolator
{
    float4 pos : SV_POSITION;
    float4 color : COLOR;
};

struct position_data
{
    float3 position : POSITION;
    uint data_index;
};

struct line_data
{
    float4 color : COLOR;
    bool forward;
};

StructuredBuffer<position_data> _Positions;
StructuredBuffer<line_data> _StyleData;

float4 get_color(uint vertexID)
{
    return _StyleData[_Positions[vertexID].data_index].color;
}

