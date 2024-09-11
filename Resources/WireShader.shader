Shader "Unlit/WireShader"
{
    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    ENDCG

    SubShader
    {
//        Blend SrcAlpha OneMinusSrcAlpha
//		ZWrite Off
//		Offset -3, -50
		Tags { "IgnoreProjector"="True" "RenderType"="Overlay" }
//		 With line joins some triangles can actually end up backwards, so disable culling
//		Cull Off

        //Front
        Pass
        {
//            ZTest LEqual
            
            CGPROGRAM
            #include "line_common.cginc"

            interpolator vert (uint vertexID: SV_VertexID)
            {
                interpolator o;

                float3 pos = _Positions[vertexID].position;

                o.pos = mul(UNITY_MATRIX_VP, float4(pos, 1.0f));
                o.color = get_color(vertexID);
                
                //if(_LineData[_Positions[vertexID].data_index].forward)
                    //o.color = float4(0, 0, 0, 0);
                
                return o;
            }

            fixed4 frag (interpolator i) : SV_Target
            {
                return float4(1, 1, 1, 1);
                return i.color;
            }
            ENDCG
        }
    }
}