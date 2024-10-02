using Unity.Mathematics;
using UnityEngine;

namespace Drawbug
{
    public static class ColorExtension
    {
        public static Color WithRed(this Color color, float red)
        {
            color.r = red;
            return color;
        }
        
        public static Color WithGreen(this Color color, float green)
        {
            color.g = green;
            return color;
        }
        
        public static Color WithBlue(this Color color, float blue)
        {
            color.b = blue;
            return color;
        }
        
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Color WithAlphaMultiplied(this Color color, float multiplier)
        {
            color.a *= multiplier;
            return color;
        }
        
        //Temporary solution for easier conversion between float2 and float3
        public static float3 to3(this float2 float2)
        {
            return new float3(float2.x, float2.y, 0);
        }
    }
}