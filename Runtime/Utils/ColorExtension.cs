using UnityEngine;

namespace Drawbug.PhysicsExtension
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
    }
}