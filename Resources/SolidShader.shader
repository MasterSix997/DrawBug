Shader "DrawBug/SolidShader"
{
    Properties
    {
        _OccludedOpacity("Ocludded Opacity", Float) = 0
    }
    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    ENDCG

    SubShader
    {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Overlay" }
		ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
		Cull Back//Off
        
        Pass
        {
            Name "Normal"
            ZTest LEqual
            
            CGPROGRAM
            #include "drawbug_common.cginc"

            interpolator vert (const uint vertex_id: SV_VertexID)
            {
                interpolator o;

                float3 pos = _Positions[vertex_id].position;
                o.position = mul(UNITY_MATRIX_VP, float4(pos, 1.0f));
                
                o.color = get_color(vertex_id);
                
                return o;
            }

            fixed4 frag (interpolator i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }

        Pass
        {
            Name "Occluded"
            ZTest Greater
            
            CGPROGRAM
            #include "drawbug_common.cginc"

            float _OccludedOpacity;

            interpolator vert (const uint vertex_id: SV_VertexID)
            {
                interpolator o;
                
                float3 pos = _Positions[vertex_id].position;
                o.position = mul(UNITY_MATRIX_VP, float4(pos, 1.0f));

                float4 color = get_color(vertex_id);

                if (!get_style(vertex_id).forward) {
                    color.a *= _OccludedOpacity;
                }

                o.color = color;
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