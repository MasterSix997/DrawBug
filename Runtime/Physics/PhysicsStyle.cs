using UnityEngine;

namespace Drawbug.PhysicsExtension
{
    public struct PhysicsStyle
    {
        public Color HitColor;
        public Color NoHitColor;
        public Color PointColor;

        public static implicit operator PhysicsStyle(DrawbugSettings settings)
        {
            return new PhysicsStyle
            {
                HitColor = settings.hitColor,
                NoHitColor = settings.noHitColor,
                PointColor = settings.pointColor
            };
        }
    }
}