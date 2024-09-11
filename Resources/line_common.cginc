struct interpolator
{
    float4 pos : SV_POSITION;
    float4 color : COLOR;
};

uniform float4x4 _ObjectToWorld;

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

            
// StructuredBuffer<float3> _Positions;
// StructuredBuffer<float4> _Colors;

StructuredBuffer<position_data> _Positions;
//StructuredBuffer<line_data> _LineData;

float4 get_color(uint vertexID)
{
    //return _LineData[_Positions[vertexID].data_index].color;
    return float4(1, 1, 1, 1);
}

