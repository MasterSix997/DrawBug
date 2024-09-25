using Unity.Mathematics;
using UnityEngine;

namespace Drawbug.PhysicsExtension
{
    public static class PhysicsDrawings
    {
        #region Physics2D
        
        public static void DrawBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, Color color)
        {
            var finalPosition = direction * distance + origin;
            
            Draw.Color = color;
            Draw.Line(origin, finalPosition);
            Draw.Rectangle(new float3(finalPosition, 0), size, quaternion.Euler(0, 0, math.radians(angle)));
            Draw.Reset();
        }

        public static void DrawCapsuleCast2D(Vector2 origin, Vector2 size, float angle, CapsuleDirection2D capsuleDirection, Vector2 direction, float distance, Color color)
        {
            var finalPosition = direction * distance + origin;
            
            Draw.Color = color;
            Draw.Line(origin, finalPosition);
            Draw.Capsule(new float3(finalPosition, 0), size, quaternion.Euler(0, 0, math.radians(angle)), capsuleDirection == CapsuleDirection2D.Vertical);
            Draw.Reset();
        }

        public static void DrawCircleCast2D(Vector2 origin, float radius, Vector2 direction, float distance, Color color)
        {
            var finalPosition = direction * distance + origin;
            
            Draw.Color = color;
            Draw.Line(origin, finalPosition);
            Draw.Circle(new float3(finalPosition, 0), radius, quaternion.Euler(0, 0, math.radians(radius)));
            Draw.Reset();
        }

        public static void DrawRaycast2D(Vector2 origin, Vector2 direction, float distance, Color color)
        {
            var finalPosition = direction * distance + origin;
            
            Draw.Color = color;
            Draw.Line(origin, finalPosition);
            Draw.Reset();
        }
        
        public static void DrawRaycast2D(Vector2 point1, Vector2 point2, Color color)
        {
            Draw.Color = color;
            Draw.Line(point1, point2);
            Draw.Reset();
        }

        #endregion
        
        #region Physics3D

        internal static void DrawBoxCast(Vector3 origin, Vector3 size, Vector3 direction, float distance, Quaternion orientation, Color color)
        {
            var finalPosition = direction * distance + origin;
            
            Draw.Color = color;
            Draw.Line(origin, finalPosition);
            Draw.Box(finalPosition, size * 2, orientation);
            Draw.Reset();
        }

        internal static void DrawCapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float distance, Color color)
        {
            var midPoint = (point1 + point2) / 2f;
            var height = Vector3.Distance(point1, point2);
            var capsuleDirection = point2 - point1;
            var rotation = point1 == point2 ? Quaternion.identity : Quaternion.LookRotation(capsuleDirection, Vector3.up) * Quaternion.Euler(0, 90, 90);
            var finalPosition = direction * distance + midPoint;
            
            Draw.Color = color;
            Draw.Line(midPoint, finalPosition);
            Draw.Capsule3D(finalPosition, radius, height + radius * 2, rotation);
            Draw.Reset();
        }

        internal static void DrawSphereCast(Vector3 origin, float radius, Vector3 direction, float distance, Color color)
        {
            var finalPosition = direction * distance + origin;
            
            Draw.Color = color;
            Draw.Line(origin, finalPosition);
            Draw.Sphere(finalPosition, radius);
            Draw.Reset();
        }

        internal static void DrawRaycast(Vector3 origin, Vector3 direction, float distance, Color color)
        {
            var finalPosition = direction * distance + origin;
            
            Draw.Color = color;
            Draw.Line(origin, finalPosition);
            Draw.Reset();
        }
        
        public static void DrawRaycast(Vector3 point1, Vector3 point2, Color color)
        {
            Draw.Color = color;
            Draw.Line(point1, point2);
            Draw.Reset();
        }
        
        #endregion
    }
}