Shader "Unlit/SolidShader"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcFactor("Src Factor", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstFactor("Dst Factor", Float) = 10
        [Enum(UnityEngine.Rendering.BlendOp)]
        _Opp("Dst Factor", Float) = 0
    }
    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    ENDCG

    SubShader
    {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Overlay" }
        Pass
        {
		    ZWrite Off
            //Blend SrcAlpha OneMinusSrcAlpha
		    Blend [_SrcFactor] [_DstFactor]
		    BlendOp [_Opp]
		    Cull Off
            ZTest LEqual
            
            CGPROGRAM
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

            //StructuredBuffer<int> _Indices;
            StructuredBuffer<position_data> _Positions;
            StructuredBuffer<line_data> _StyleData;

            float4 get_color(uint vertexID)
            {
                return _StyleData[_Positions[vertexID].data_index].color;
            }

            interpolator vert (uint vertexID: SV_VertexID)
            {
                interpolator o;

                //uint index = _Indices[vertexID];
                float3 pos = _Positions[vertexID].position;

                o.pos = mul(UNITY_MATRIX_VP, float4(pos, 1.0f));
                o.color = get_color(vertexID);
                
                //if(_LineData[_Positions[vertexID].data_index].forward)
                    //o.color = float4(0, 0, 0, 0);
                
                return o;
            }

            fixed4 frag (interpolator i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}