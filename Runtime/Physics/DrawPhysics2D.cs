using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug.PhysicsExtension
{
    public static class DrawPhysics2D
    {
        public static PhysicsStyle Style = new()
        {
            HitColor = Color.green,
            NoHitColor = Color.red,
            PointColor = Color.red
        };
        
        private static Color PhysicsColor(bool collided = true) => collided ? Style.HitColor : Style.NoHitColor;
        private static Color PointColor => Style.PointColor;
        
        private static float _maxDistance = 1000000;

        #region BoxCast

        #region BoxCast Single

        public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction)
        {
            return BoxCast(origin, size, angle, direction, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance)
        {
            return BoxCast(origin, size, angle, direction, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
        public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, LayerMask layerMask)
        {
            return BoxCast(origin, size, angle, direction, distance, layerMask, -_maxDistance, _maxDistance);
        }
        
        public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, LayerMask layerMask, float minDepth)
        {
            return BoxCast(origin, size, angle, direction, distance, layerMask, minDepth, _maxDistance);
        }

        public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, LayerMask layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var hitInfo = Physics2D.BoxCast(origin, size, angle, direction, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            
            distance = Mathf.Min(distance, _maxDistance);

            var collided = hitInfo.collider != null;
            var rot = Quaternion.Euler(0, 0, angle);

            if (collided)
            {
                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
                distance = hitInfo.distance;
            }

            PhysicsDrawings.DrawBoxCast2D(origin, size, angle, direction, distance, PhysicsColor(collided));
#endif
            return hitInfo;
        }

        public static int BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results)
        {
            return BoxCast(origin, size, angle, direction, contactFilter, results, _maxDistance);
        }
        
        public static int BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results, float distance)
        {
            direction.Normalize();
            var count = Physics2D.BoxCast(origin, size, angle, direction, contactFilter, results, distance);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;
            var rot = Quaternion.Euler(0, 0, angle);
    
            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                collided = true;
    
                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;
    
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Rectangle(new float3(origin + direction * hit.distance, 0), size, rot);
            }
    
            distance = Mathf.Min(distance, _maxDistance);
    
            PhysicsDrawings.DrawBoxCast2D(origin, size, angle, direction, distance, PhysicsColor(collided));
#endif
            return count;
        }
        
        public static int BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results)
        {
            return BoxCast(origin, size, angle, direction, contactFilter, results, _maxDistance);
        }
        
        public static int BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results, float distance)
        {
            direction.Normalize();
            var count = Physics2D.BoxCast(origin, size, angle, direction, contactFilter, results, distance);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;
            var rot = Quaternion.Euler(0, 0, angle);
    
            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                collided = true;
                
                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;
    
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Rectangle(new float3(origin + direction * hit.distance, 0), size, rot);
            }
    
            distance = Mathf.Min(distance, _maxDistance);
    
            PhysicsDrawings.DrawBoxCast2D(origin, size, angle, direction, distance, PhysicsColor(collided));
#endif
            return count;
        }
        #endregion
        
        #region BoxCast All

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastAll is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction)
        {
            return BoxCastAll(origin, size, angle, direction, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastAll is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance)
        {
            return BoxCastAll(origin, size, angle, direction, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastAll is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask)
        {
            return BoxCastAll(origin, size, angle, direction, distance, layerMask, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastAll is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask, float minDepth)
        {
            return BoxCastAll(origin, size, angle, direction, distance, layerMask, minDepth, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastAll is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var hitInfo = Physics2D.BoxCastAll(origin, size, angle, direction, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            float maxDistanceRay = 0;

            var collided = false;
            var rot = Quaternion.Euler(0, 0, angle);

            foreach (var hit in hitInfo)
            {
                collided = true;
                
                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;

                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Rectangle(new float3(origin + direction * hit.distance, 0), size, rot);
            }

            distance = Mathf.Min(distance, _maxDistance);

            PhysicsDrawings.DrawBoxCast2D(origin, size, angle, direction, distance, PhysicsColor(collided));
#endif
            return hitInfo;
        }

        #endregion
        
        #region BoxCast non alloc

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastNonAlloc is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results)
        {
            return BoxCastNonAlloc(origin, size, angle, direction, results, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastNonAlloc is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results, float distance)
        {
            return BoxCastNonAlloc(origin, size, angle, direction, results, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastNonAlloc is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask)
        {
            return BoxCastNonAlloc(origin, size, angle, direction, results, distance, layerMask, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastNonAlloc is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth)
        {
            return BoxCastNonAlloc(origin, size, angle, direction, results, distance, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.BoxCastNonAlloc is deprecated. Use Physics2D.BoxCast instead.", false)]
#endif
        public static int BoxCastNonAlloc(Vector2 origin, Vector2 size, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var count = Physics2D.BoxCastNonAlloc(origin, size, angle, direction, results, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;

            var rot = Quaternion.Euler(0, 0, angle);

            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                collided = true;

                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;

                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Rectangle(new float3(origin + direction * hit.distance, 0), size, rot);
            }

            distance = math.min(distance, _maxDistance);

            PhysicsDrawings.DrawBoxCast2D(origin, size, angle, direction, distance, PhysicsColor(collided));
#endif
            return count;
        }

        #endregion
        
        #endregion
        
        #region CapsuleCast

        #region CapsuleCast single

        public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction)
        {
            return CapsuleCast(origin, size, capsuleDirection, angle, direction, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance)
        {
            return CapsuleCast(origin, size, capsuleDirection, angle, direction, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask)
        {
            return CapsuleCast(origin, size, capsuleDirection, angle, direction, distance, layerMask, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask, float minDepth)
        {
            return CapsuleCast(origin, size, capsuleDirection, angle, direction, distance, layerMask, minDepth, _maxDistance);
        }

        public static RaycastHit2D CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var hitInfo = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            distance = Mathf.Min(distance, _maxDistance);
            
            var rot = Quaternion.Euler(0, 0, angle);
            
            if (hitInfo.collider != null)
            {
                collided = true;
                distance = hitInfo.distance;
                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
            }

            PhysicsDrawings.DrawCapsuleCast2D(origin, size, angle, capsuleDirection, direction, distance, PhysicsColor(collided));
#endif
            return hitInfo;
        }

        public static int CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results)
        {
            return CapsuleCast(origin, size, capsuleDirection, angle, direction, contactFilter, results, _maxDistance);
        }
        
        public static int CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results, float distance)
        {
            direction.Normalize();
            var count = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, contactFilter, results, distance);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            distance = Mathf.Min(distance, _maxDistance);
    
            var rot = Quaternion.Euler(0, 0, angle);
            
            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                collided = true;
    
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Capsule(new float3(origin + direction * hit.distance, 0), size, rot, capsuleDirection == CapsuleDirection2D.Vertical);
            }
    
            PhysicsDrawings.DrawCapsuleCast2D(origin, size, angle, capsuleDirection, direction, distance, PhysicsColor(collided));
#endif
            return count;
        }
        
        public static int CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results)
        {
            return CapsuleCast(origin, size, capsuleDirection, angle, direction, contactFilter, results, _maxDistance);
        }
        
        public static int CapsuleCast(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results, float distance)
        {
            direction.Normalize();
            var count = Physics2D.CapsuleCast(origin, size, capsuleDirection, angle, direction, contactFilter, results, distance);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            distance = Mathf.Min(distance, _maxDistance);
    
            var rot = Quaternion.Euler(0, 0, angle);
            
            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                collided = true;
    
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Capsule(new float3(origin + direction * hit.distance, 0), size, rot, capsuleDirection == CapsuleDirection2D.Vertical);
            }
    
            PhysicsDrawings.DrawCapsuleCast2D(origin, size, angle, capsuleDirection, direction, distance, PhysicsColor(collided));
#endif
            return count;
        }

        #endregion

        #region CapsuleCast All
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastAll is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction)
        {
            return CapsuleCastAll(origin, size, capsuleDirection, angle, direction, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastAll is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance)
        {
            return CapsuleCastAll(origin, size, capsuleDirection, angle, direction, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastAll is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask)
        {
            return CapsuleCastAll(origin, size, capsuleDirection, angle, direction, distance, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastAll is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask, float minDepth)
        {
            return CapsuleCastAll(origin, size, capsuleDirection, angle, direction, distance, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastAll is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static RaycastHit2D[] CapsuleCastAll(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var hitInfo = Physics2D.CapsuleCastAll(origin, size, capsuleDirection, angle, direction, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            distance = Mathf.Min(distance, _maxDistance);
    
            var rot = Quaternion.Euler(0, 0, angle);
    
            var collided = false;
            float maxDistanceRay = 0;
    
            foreach (var hit in hitInfo)
            {
                collided = true;
    
                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;
    
                Draw.Color = PointColor;
                Draw.Point(hit.point);Draw.Color = PhysicsColor();
                Draw.Capsule(new float3(origin + direction * hit.distance, 0), size, rot, capsuleDirection == CapsuleDirection2D.Vertical);
            }
    
            PhysicsDrawings.DrawCapsuleCast2D(origin, size, angle, capsuleDirection, direction, distance, PhysicsColor(collided));
#endif
            return hitInfo;
        }

        #endregion

        #region CapsuleCast non alloc
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastNonAlloc is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results)
        {
            return CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results, _maxDistance, Physics2D.AllLayers, -_maxDistance,
                _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastNonAlloc is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results, float distance)
        {
            return CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results, distance, Physics2D.AllLayers, -_maxDistance,
                _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastNonAlloc is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask)
        {
            return CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results, distance, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastNonAlloc is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth)
        {
            return CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results, distance, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CapsuleCastNonAlloc is deprecated. Use Physics2D.CapsuleCast instead.", false)]
#endif
        public static int CapsuleCastNonAlloc(Vector2 origin, Vector2 size, CapsuleDirection2D capsuleDirection, float angle, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var count = Physics2D.CapsuleCastNonAlloc(origin, size, capsuleDirection, angle, direction, results, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;
    
            distance = Mathf.Min(distance, _maxDistance);
    
            var rot = Quaternion.Euler(0, 0, angle);
    
            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                collided = true;
                
                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;
    
                Draw.Color = PointColor;
                Draw.Point(hit.point);Draw.Color = PhysicsColor();
                Draw.Capsule(new float3(origin + direction * hit.distance, 0), size, rot, capsuleDirection == CapsuleDirection2D.Vertical);
            }
    
            PhysicsDrawings.DrawCapsuleCast2D(origin, size, angle, capsuleDirection, direction, distance, PhysicsColor(collided));
#endif
            return count;
        }

        #endregion

        #endregion
        
        #region CircleCast

        #region CircleCast single

        public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction)
        {
            return CircleCast(origin, radius, direction, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction, float distance)
        {
            return CircleCast(origin, radius, direction, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask)
        {
            return CircleCast(origin, radius, direction, distance, layerMask, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask, float minDepth)
        {
            return CircleCast(origin, radius, direction, distance, layerMask, minDepth, _maxDistance);
        }

        public static RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var hitInfo = Physics2D.CircleCast(origin, radius, direction, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            distance = Mathf.Min(distance, _maxDistance);

            var collided = hitInfo.collider != null;

            if (collided)
            {
                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
                distance = hitInfo.distance;
            }

            PhysicsDrawings.DrawCircleCast2D(origin, radius, direction, distance, PhysicsColor(collided));
#endif
            return hitInfo;
        }

        public static int CircleCast(Vector2 origin, float radius, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results)
        {
            return CircleCast(origin, radius, direction, contactFilter, results, _maxDistance);
        }

        public static int CircleCast(Vector2 origin, float radius, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results, float distance)
        {
            direction.Normalize();
            var count = Physics2D.CircleCast(origin, radius, direction, contactFilter, results, distance);
#if !DONT_DRAW_PHYSICS
            distance = Mathf.Min(distance, _maxDistance);

            var collided = false;
            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                collided = true;

                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Circle(new float3(origin + direction * hit.distance, 0), radius, quaternion.identity);
            }

            PhysicsDrawings.DrawCircleCast2D(origin, radius, direction, distance, PhysicsColor(collided));
#endif
            return count;
        }

        public static int CircleCast(Vector2 origin, float radius, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results)
        {
            return CircleCast(origin, radius, direction, contactFilter, results, _maxDistance);
        }

        public static int CircleCast(Vector2 origin, float radius, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results, float distance)
        {
            direction.Normalize();
            var count = Physics2D.CircleCast(origin, radius, direction, contactFilter, results, distance);
#if !DONT_DRAW_PHYSICS
            distance = Mathf.Min(distance, _maxDistance);

            var collided = false;
            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                collided = true;

                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Circle(new float3(origin + direction * hit.distance, 0), radius, quaternion.identity);
            }

            PhysicsDrawings.DrawCircleCast2D(origin, radius, direction, distance, PhysicsColor(collided));
#endif
            return count;
        }

        #endregion

        #region CircleCast All
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastAll is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction)
        {
            return CircleCastAll(origin, radius, direction, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastAll is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction, float distance)
        {
            return CircleCastAll(origin, radius, direction, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastAll is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask)
        {
            return CircleCastAll(origin, radius, direction, distance, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastAll is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask, float minDepth)
        {
            return CircleCastAll(origin, radius, direction, distance, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastAll is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static RaycastHit2D[] CircleCastAll(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var hitInfo = Physics2D.CircleCastAll(origin, radius, direction, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;
        
            foreach (var hit in hitInfo)
            {
                collided = true;
                
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Circle(new float3(origin + direction * hit.distance, 0), radius, quaternion.identity);
            }
        
            distance = Mathf.Min(distance, _maxDistance);
            
            PhysicsDrawings.DrawCircleCast2D(origin, radius, direction, distance, PhysicsColor(collided));
#endif
            return hitInfo;
        }
        
        #endregion
        
        #region CircleCast non alloc
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastNonAlloc is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results)
        {
            return CircleCastNonAlloc(origin, radius, direction, results, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastNonAlloc is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results, float distance)
        {
            return CircleCastNonAlloc(origin, radius, direction, results, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastNonAlloc is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask)
        {
            return CircleCastNonAlloc(origin, radius, direction, results, distance, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastNonAlloc is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth)
        {
            return CircleCastNonAlloc(origin, radius, direction, results, distance, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.CircleCastNonAlloc is deprecated. Use Physics2D.CircleCast instead.", false)]
#endif
        public static int CircleCastNonAlloc(Vector2 origin, float radius, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var count = Physics2D.CircleCastNonAlloc(origin, radius, direction, results, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;
        
            for (var i = 0; i < count; i++)
            {
                var hit = results[i];
                collided = true;
        
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Circle(new float3(origin + direction * hit.distance, 0), radius, quaternion.identity);
            }
        
            distance = Mathf.Min(distance, _maxDistance);
        
            PhysicsDrawings.DrawCircleCast2D(origin, radius, direction, distance, PhysicsColor(collided));
#endif
            return count;
        }
        
        #endregion

        #endregion
        
        #region LineCast

        #region LineCast single

        public static RaycastHit2D Linecast(Vector2 start, Vector2 end)
        {
            return Linecast(start, end, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D Linecast(Vector2 start, Vector2 end, int layerMask)
        {
            return Linecast(start, end, layerMask, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D Linecast(Vector2 start, Vector2 end, int layerMask, float minDepth)
        {
            return Linecast(start, end, layerMask, minDepth, _maxDistance);
        }

        public static RaycastHit2D Linecast(Vector2 start, Vector2 end, int layerMask, float minDepth, float maxDepth)
        {
            var hitInfo = Physics2D.Linecast(start, end, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;

            if (hitInfo.collider != null)
            {
                collided = true;
                end = hitInfo.point;

                Draw.Color = PointColor;
                Draw.Point(end);
            }

            PhysicsDrawings.DrawRaycast2D(start, end, PhysicsColor(collided));
#endif
            return hitInfo;
        }
        

        public static int Linecast(Vector2 start, Vector2 end, ContactFilter2D contactFilter, RaycastHit2D[] results)
        {
            var size = Physics2D.Linecast(start, end, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            var collided = false;

            var previewOrigin = start;
            var sectionOrigin = start;

            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                collided = true;
                
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Line(sectionOrigin, hit.point);

                if ((start - hit.point).sqrMagnitude > (start - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;

                sectionOrigin = hit.point;
            }

            PhysicsDrawings.DrawRaycast2D(start, end, PhysicsColor(collided));
#endif
            return size;
        }
        

        public static int Linecast(Vector2 start, Vector2 end, ContactFilter2D contactFilter, List<RaycastHit2D> results)
        {
            var size = Physics2D.Linecast(start, end, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            var collided = false;

            var previewOrigin = start;
            var sectionOrigin = start;

            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                collided = true;
                
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Line(sectionOrigin, hit.point);

                if ((start - hit.point).sqrMagnitude > (start - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;

                sectionOrigin = hit.point;
            }

            PhysicsDrawings.DrawRaycast2D(start, end, PhysicsColor(collided));
#endif
            return size;
        }

        #endregion

        #region LineCast All
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.LinecastAll is deprecated. Use Physics2D.Linecast instead.", false)]
#endif
        public static RaycastHit2D[] LinecastAll(Vector2 start, Vector2 end)
        {
            return LinecastAll(start, end, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.LinecastAll is deprecated. Use Physics2D.Linecast instead.", false)]
#endif
        public static RaycastHit2D[] LinecastAll(Vector2 start, Vector2 end, int layerMask)
        {
            return LinecastAll(start, end, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.LinecastAll is deprecated. Use Physics2D.Linecast instead.", false)]
#endif
        public static RaycastHit2D[] LinecastAll(Vector2 start, Vector2 end, int layerMask, float minDepth)
        {
            return LinecastAll(start, end, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.LinecastAll is deprecated. Use Physics2D.Linecast instead.", false)]
#endif
        public static RaycastHit2D[] LinecastAll(Vector2 start, Vector2 end, int layerMask, float minDepth, float maxDepth)
        {
            var hitInfo = Physics2D.LinecastAll(start, end, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;
        
            var previewOrigin = start;
            var sectionOrigin = start;
        
            foreach (var hit in hitInfo)
            {
                collided = true;
                
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Line(sectionOrigin, hit.point);
        
                if ((start - hit.point).sqrMagnitude > (start - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;
        
                sectionOrigin = hit.point;
            }
        
            PhysicsDrawings.DrawRaycast2D(start, end, PhysicsColor(collided));
#endif
            return hitInfo;
        }
        
        #endregion
        
        #region LineCast non alloc
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.LinecastNonAlloc is deprecated. Use Physics2D.Linecast instead.", false)]
#endif
        public static int LinecastNonAlloc(Vector2 start, Vector2 end, RaycastHit2D[] results)
        {
            return LinecastNonAlloc(start, end, results, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.LinecastNonAlloc is deprecated. Use Physics2D.Linecast instead.", false)]
#endif
        public static int LinecastNonAlloc(Vector2 start, Vector2 end, RaycastHit2D[] results, int layerMask)
        {
            return LinecastNonAlloc(start, end, results, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.LinecastNonAlloc is deprecated. Use Physics2D.Linecast instead.", false)]
#endif
        public static int LinecastNonAlloc(Vector2 start, Vector2 end, RaycastHit2D[] results, int layerMask, float minDepth)
        {
            return LinecastNonAlloc(start, end, results, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.LinecastNonAlloc is deprecated. Use Physics2D.Linecast instead.", false)]
#endif
        public static int LinecastNonAlloc(Vector2 start, Vector2 end, RaycastHit2D[] results, int layerMask, float minDepth, float maxDepth)
        {
            var size = Physics2D.LinecastNonAlloc(start, end, results, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;
        
            var previewOrigin = start;
            var sectionOrigin = start;
        
            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                collided = true;
                
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Line(sectionOrigin, hit.point);
        
                if ((start - hit.point).sqrMagnitude > (start - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;
        
                sectionOrigin = hit.point;
            }
        
            PhysicsDrawings.DrawRaycast2D(start, end, PhysicsColor(collided));
#endif
            return size;
        }
        
        #endregion

        #endregion
        
        #region OverlapArea

        #region OverlapArea single

        public static Collider2D OverlapArea(Vector2 pointA, Vector2 pointB)
        {
            return OverlapArea(pointA, pointB, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static Collider2D OverlapArea(Vector2 pointA, Vector2 pointB, int layerMask)
        {
            return OverlapArea(pointA, pointB, layerMask, -_maxDistance, _maxDistance);
        }

        public static Collider2D OverlapArea(Vector2 pointA, Vector2 pointB, int layerMask, float minDepth)
        {
            return OverlapArea(pointA, pointB, layerMask, minDepth, _maxDistance);
        }

        public static Collider2D OverlapArea(Vector2 pointA, Vector2 pointB, int layerMask, float minDepth, float maxDepth)
        {
            var collider = Physics2D.OverlapArea(pointA, pointB, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(collider != null);
            Draw.Rectangle(pointA, pointB);
#endif
            return collider;
        }
        
        public static int OverlapArea(Vector2 pointA, Vector2 pointB, ContactFilter2D contactFilter, Collider2D[] results)
        {
            var size = Physics2D.OverlapArea(pointA, pointB, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(size > 0);
            Draw.Rectangle(pointA, pointB);
#endif
            return size;
        }
        
        public static int OverlapArea(Vector2 pointA, Vector2 pointB, ContactFilter2D contactFilter, List<Collider2D> results)
        {
            var size = Physics2D.OverlapArea(pointA, pointB, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(size > 0);
            Draw.Rectangle(pointA, pointB);
#endif
            return size;
        }
        
        #endregion

        #region OverlapArea all
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapAreaAll is deprecated. Use Physics2D.OverlapArea instead.", false)]
#endif
        public static Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB)
        {
            return OverlapAreaAll(pointA, pointB, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapAreaAll is deprecated. Use Physics2D.OverlapArea instead.", false)]
#endif
        public static Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB, int layerMask)
        {
            return OverlapAreaAll(pointA, pointB, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapAreaAll is deprecated. Use Physics2D.OverlapArea instead.", false)]
#endif
        public static Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB, int layerMask, float minDepth)
        {
            return OverlapAreaAll(pointA, pointB, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapAreaAll is deprecated. Use Physics2D.OverlapArea instead.", false)]
#endif
        public static Collider2D[] OverlapAreaAll(Vector2 pointA, Vector2 pointB, int layerMask, float minDepth, float maxDepth)
        {
            var colliders = Physics2D.OverlapAreaAll(pointA, pointB, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(colliders.Length > 0);
            Draw.Rectangle(pointA, pointB);
#endif
            return colliders;
        }
        
        #endregion
        
        #region OverlapArea non alloc
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapAreaNonAlloc is deprecated. Use Physics2D.OverlapArea instead.", false)]
#endif
        public static int OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, Collider2D[] results)
        {
            return OverlapAreaNonAlloc(pointA, pointB, results, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapAreaNonAlloc is deprecated. Use Physics2D.OverlapArea instead.", false)]
#endif
        public static int OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, Collider2D[] results, int layerMask)
        {
            return OverlapAreaNonAlloc(pointA, pointB, results, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapAreaNonAlloc is deprecated. Use Physics2D.OverlapArea instead.", false)]
#endif
        public static int OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, Collider2D[] results, int layerMask, float minDepth)
        {
            return OverlapAreaNonAlloc(pointA, pointB, results, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapAreaNonAlloc is deprecated. Use Physics2D.OverlapArea instead.", false)]
#endif
        public static int OverlapAreaNonAlloc(Vector2 pointA, Vector2 pointB, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
        {
            var size = Physics2D.OverlapAreaNonAlloc(pointA, pointB, results, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(size > 0);
            Draw.Rectangle(pointA, pointB);
#endif
            return size;
        }
        
        #endregion

        #endregion
        
        #region OverlapBox

        #region OverlapBox single

        public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle)
        {
            return OverlapBox(point, size, angle, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle, int layerMask)
        {
            return OverlapBox(point, size, angle, layerMask, -_maxDistance, _maxDistance);
        }

        public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle, int layerMask, float minDepth)
        {
            return OverlapBox(point, size, angle, layerMask, minDepth, _maxDistance);
        }

        public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle, int layerMask, float minDepth, float maxDepth)
        {
            var collider = Physics2D.OverlapBox(point, size, angle, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(collider != null);
            Draw.Rectangle(new float3(point, 0), size, rot);
#endif
            return collider;
        }

        public static int OverlapBox(Vector2 point, Vector2 size, float angle, ContactFilter2D contactFilter, Collider2D[] results)
        {
            var count = Physics2D.OverlapBox(point, size, angle, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(count > 0);
            Draw.Rectangle(new float3(point, 0), size, rot);
#endif
            return count;
        }
        
        public static int OverlapBox(Vector2 point, Vector2 size, float angle, ContactFilter2D contactFilter, List<Collider2D> results)
        {
            var count = Physics2D.OverlapBox(point, size, angle, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(count > 0);
            Draw.Rectangle(new float3(point, 0), size, rot);
#endif
            return count;
        }
        
        #endregion
        
        #region OverlapBox all
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapBoxAll is deprecated. Use Physics2D.OverlapBox instead.", false)]
#endif
        public static Collider2D[] OverlapBoxAll(Vector2 point, Vector2 size, float angle)
        {
            return OverlapBoxAll(point, size, angle, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapBoxAll is deprecated. Use Physics2D.OverlapBox instead.", false)]
#endif
        public static Collider2D[] OverlapBoxAll(Vector2 point, Vector2 size, float angle, int layerMask)
        {
            return OverlapBoxAll(point, size, angle, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapBoxAll is deprecated. Use Physics2D.OverlapBox instead.", false)]
#endif
        public static Collider2D[] OverlapBoxAll(Vector2 point, Vector2 size, float angle, int layerMask, float minDepth)
        {
            return OverlapBoxAll(point, size, angle, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapBoxAll is deprecated. Use Physics2D.OverlapBox instead.", false)]
#endif
        public static Collider2D[] OverlapBoxAll(Vector2 point, Vector2 size, float angle, int layerMask, float minDepth, float maxDepth)
        {
            var colliders = Physics2D.OverlapBoxAll(point, size, angle, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(colliders.Length > 0);
            Draw.Rectangle(new float3(point, 0), size, rot);
#endif
            return colliders;
        }
        
        #endregion
        
        #region OverlapBox non alloc
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapBoxNonAlloc is deprecated. Use Physics2D.OverlapBox instead.", false)]
#endif
        public static int OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, Collider2D[] results)
        {
            return OverlapBoxNonAlloc(point, size, angle, results, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapBoxNonAlloc is deprecated. Use Physics2D.OverlapBox instead.", false)]
#endif
        public static int OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, Collider2D[] results, int layerMask)
        {
            return OverlapBoxNonAlloc(point, size, angle, results, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapBoxNonAlloc is deprecated. Use Physics2D.OverlapBox instead.", false)]
#endif
        public static int OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, Collider2D[] results, int layerMask, float minDepth)
        {
            return OverlapBoxNonAlloc(point, size, angle, results, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapBoxNonAlloc is deprecated. Use Physics2D.OverlapBox instead.", false)]
#endif
        public static int OverlapBoxNonAlloc(Vector2 point, Vector2 size, float angle, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
        {
            var count = Physics2D.OverlapBoxNonAlloc(point, size, angle, results, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(count > 0);
            Draw.Rectangle(new float3(point, 0), size, rot);
#endif
            return count;
        }
        
        #endregion

        #endregion
        
        #region OverlapCapsule

        #region OverlapCapsule single

        public static Collider2D OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle)
        {
            return OverlapCapsule(point, size, direction, angle, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static Collider2D OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask)
        {
            return OverlapCapsule(point, size, direction, angle, layerMask, -_maxDistance, _maxDistance);
        }

        public static Collider2D OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask, float minDepth)
        {
            return OverlapCapsule(point, size, direction, angle, layerMask, minDepth, _maxDistance);
        }

        public static Collider2D OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask, float minDepth, float maxDepth)
        {
            var collider = Physics2D.OverlapCapsule(point, size, direction, angle, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(collider);
            Draw.Capsule(new float3(point, 0), size, rot, direction == CapsuleDirection2D.Vertical);
#endif
            return collider;
        }

        public static int OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, ContactFilter2D contactFilter, Collider2D[] results)
        {
            var count = Physics2D.OverlapCapsule(point, size, direction, angle, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(count > 0);
            Draw.Capsule(new float3(point, 0), size, rot, direction == CapsuleDirection2D.Vertical);
#endif
            return count;
        }
        
        public static int OverlapCapsule(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, ContactFilter2D contactFilter, List<Collider2D> results)
        {
            var count = Physics2D.OverlapCapsule(point, size, direction, angle, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(count > 0);
            Draw.Capsule(new float3(point, 0), size, rot, direction == CapsuleDirection2D.Vertical);
#endif
            return count;
        }
        
        #endregion
        
        #region OverlapCapsule all
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCapsuleAll is deprecated. Use Physics2D.OverlapCapsule instead.", false)]
#endif
        public static Collider2D[] OverlapCapsuleAll(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle)
        {
            return OverlapCapsuleAll(point, size, direction, angle, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCapsuleAll is deprecated. Use Physics2D.OverlapCapsule instead.", false)]
#endif
        public static Collider2D[] OverlapCapsuleAll(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask)
        {
            return OverlapCapsuleAll(point, size, direction, angle, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCapsuleAll is deprecated. Use Physics2D.OverlapCapsule instead.", false)]
#endif
        public static Collider2D[] OverlapCapsuleAll(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask, float minDepth)
        {
            return OverlapCapsuleAll(point, size, direction, angle, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCapsuleAll is deprecated. Use Physics2D.OverlapCapsule instead.", false)]
#endif
        public static Collider2D[] OverlapCapsuleAll(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, int layerMask, float minDepth, float maxDepth)
        {
            var colliders = Physics2D.OverlapCapsuleAll(point, size, direction, angle, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(colliders.Length > 0);
            Draw.Capsule(new float3(point, 0), size, rot, direction == CapsuleDirection2D.Vertical);
#endif
            return colliders;
        }
        
        #endregion
        
        #region OverlapCapsule non alloc
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCapsuleNonAlloc is deprecated. Use Physics2D.OverlapCapsule instead.", false)]
#endif
        public static int OverlapCapsuleNonAlloc(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, Collider2D[] results)
        {
            return OverlapCapsuleNonAlloc(point, size, direction, angle, results, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCapsuleNonAlloc is deprecated. Use Physics2D.OverlapCapsule instead.", false)]
#endif
        public static int OverlapCapsuleNonAlloc(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, Collider2D[] results, int layerMask)
        {
            return OverlapCapsuleNonAlloc(point, size, direction, angle, results, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCapsuleNonAlloc is deprecated. Use Physics2D.OverlapCapsule instead.", false)]
#endif
        public static int OverlapCapsuleNonAlloc(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, Collider2D[] results, int layerMask, float minDepth,  PhysicsStyle physicsStyle = default)
        {
            return OverlapCapsuleNonAlloc(point, size, direction, angle, results, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCapsuleNonAlloc is deprecated. Use Physics2D.OverlapCapsule instead.", false)]
#endif
        public static int OverlapCapsuleNonAlloc(Vector2 point, Vector2 size, CapsuleDirection2D direction, float angle, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
        {
            var count = Physics2D.OverlapCapsuleNonAlloc(point, size, direction, angle, results, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var rot = Quaternion.Euler(0, 0, angle);
            Draw.Color = PhysicsColor(count > 0);
            Draw.Capsule(new float3(point, 0), size, rot, direction == CapsuleDirection2D.Vertical);
#endif
            return count;
        }
        
        #endregion
        
        #endregion
        
        #region OverlapCircle
        
        #region OverlapCircle single
        
        public static Collider2D OverlapCircle(Vector2 point, float radius)
        {
            return OverlapCircle(point, radius, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
        public static Collider2D OverlapCircle(Vector2 point, float radius, int layerMask)
        {
            return OverlapCircle(point, radius, layerMask, -_maxDistance, _maxDistance);
        }
        
        public static Collider2D OverlapCircle(Vector2 point, float radius, int layerMask, float minDepth)
        {
            return OverlapCircle(point, radius, layerMask, minDepth, _maxDistance);
        }
        
        public static Collider2D OverlapCircle(Vector2 point, float radius, int layerMask, float minDepth, float maxDepth)
        {
            var collider = Physics2D.OverlapCircle(point, radius, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(collider);
            Draw.Circle(new float3(point, 0), radius);
#endif
            return collider;
        }
        
        public static int OverlapCircle(Vector2 point, float radius, ContactFilter2D contactFilter, Collider2D[] results)
        {
            var count = Physics2D.OverlapCircle(point, radius, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(count > 0);
            Draw.Circle(new float3(point, 0), radius);
#endif
            return count;
        }
        
        public static int OverlapCircle(Vector2 point, float radius, ContactFilter2D contactFilter, List<Collider2D> results)
        {
            var count = Physics2D.OverlapCircle(point, radius, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(count > 0);
            Draw.Circle(new float3(point, 0), radius);
#endif
            return count;
        }
        
        #endregion
        
        #region OverlapCircle all
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapCircle instead.", false)]
#endif
        public static Collider2D[] OverlapCircleAll(Vector2 point, float radius)
        {
            return OverlapCircleAll(point, radius, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapCircle instead.", false)]
#endif
        public static Collider2D[] OverlapCircleAll(Vector2 point, float radius, int layerMask)
        {
            return OverlapCircleAll(point, radius, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapCircle instead.", false)]
#endif
        public static Collider2D[] OverlapCircleAll(Vector2 point, float radius, int layerMask, float minDepth)
        {
            return OverlapCircleAll(point, radius, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapCircle instead.", false)]
#endif
        public static Collider2D[] OverlapCircleAll(Vector2 point, float radius, int layerMask, float minDepth, float maxDepth)
        {
            var colliders = Physics2D.OverlapCircleAll(point, radius, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(colliders.Length > 0);
            Draw.Circle(new float3(point, 0), radius);
#endif
            return colliders;
        }
        
        #endregion
        
        #region OverlapCircle non alloc
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapCircle instead.", false)]
#endif
        public static int OverlapCircleNonAlloc(Vector2 point, float radius, Collider2D[] results)
        {
            return OverlapCircleNonAlloc(point, radius, results, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapCircle instead.", false)]
#endif
        public static int OverlapCircleNonAlloc(Vector2 point, float radius, Collider2D[] results, int layerMask)
        {
            return OverlapCircleNonAlloc(point, radius, results, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapCircle instead.", false)]
#endif
        public static int OverlapCircleNonAlloc(Vector2 point, float radius, Collider2D[] results, int layerMask, float minDepth)
        {
            return OverlapCircleNonAlloc(point, radius, results, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapCircle instead.", false)]
#endif
        public static int OverlapCircleNonAlloc(Vector2 point, float radius, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
        {
            var count = Physics2D.OverlapCircleNonAlloc(point, radius, results, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(count > 0);
            Draw.Circle(new float3(point, 0), radius);
#endif
            return count;
        }
        
        #endregion
        
        #endregion
        
        #region OverlapPoint
        
        #region OverlapPoint single
        
        public static Collider2D OverlapPoint(Vector2 point)
        {
            return OverlapPoint(point, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
        public static Collider2D OverlapPoint(Vector2 point, int layerMask)
        {
            return OverlapPoint(point, layerMask, -_maxDistance, _maxDistance);
        }
        
        public static Collider2D OverlapPoint(Vector2 point, int layerMask, float minDepth)
        {
            return OverlapPoint(point, layerMask, minDepth, _maxDistance);
        }
        
        public static Collider2D OverlapPoint(Vector2 point, int layerMask, float minDepth, float maxDepth)
        {
            var collider = Physics2D.OverlapPoint(point, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(collider);
            Draw.Point(point);
#endif
            return collider;
        }
        
        public static int OverlapPoint(Vector2 point, ContactFilter2D contactFilter, Collider2D[] results)
        {
            var count = Physics2D.OverlapPoint(point, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(count > 0);
            Draw.Point(point);
#endif
            return count;
        }
        
        public static int OverlapPoint(Vector2 point, ContactFilter2D contactFilter, List<Collider2D> results)
        {
            var count = Physics2D.OverlapPoint(point, contactFilter, results);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(count > 0);
            Draw.Point(point);
#endif
            return count;
        }
        
        #endregion
        
        #region OverlapPoint all
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapPoint instead.", false)]
#endif
        public static Collider2D[] OverlapPointAll(Vector2 point)
        {
            return OverlapPointAll(point, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapPoint instead.", false)]
#endif
        public static Collider2D[] OverlapPointAll(Vector2 point, int layerMask)
        {
            return OverlapPointAll(point, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapPoint instead.", false)]
#endif
        public static Collider2D[] OverlapPointAll(Vector2 point, int layerMask, float minDepth)
        {
            return OverlapPointAll(point, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapCircleAll is deprecated. Use Physics2D.OverlapPoint instead.", false)]
#endif
        public static Collider2D[] OverlapPointAll(Vector2 point, int layerMask, float minDepth, float maxDepth)
        {
            var colliders = Physics2D.OverlapPointAll(point, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(colliders.Length > 0);
            Draw.Point(point);
#endif
            return colliders;
        }
        
        #endregion
        
        #region OverlapPoint non alloc
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapPointNonAlloc is deprecated. Use Physics2D.OverlapPoint instead.", false)]
#endif
        public static int OverlapPointNonAlloc(Vector2 point, Collider2D[] results)
        {
            return OverlapPointNonAlloc(point, results, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapPointNonAlloc is deprecated. Use Physics2D.OverlapPoint instead.", false)]
#endif
        public static int OverlapPointNonAlloc(Vector2 point, Collider2D[] results, int layerMask)
        {
            return OverlapPointNonAlloc(point, results, layerMask, -_maxDistance, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapPointNonAlloc is deprecated. Use Physics2D.OverlapPoint instead.", false)]
#endif
        public static int OverlapPointNonAlloc(Vector2 point, Collider2D[] results, int layerMask, float minDepth)
        {
            return OverlapPointNonAlloc(point, results, layerMask, minDepth, _maxDistance);
        }
        
#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.OverlapPointNonAlloc is deprecated. Use Physics2D.OverlapPoint instead.", false)]
#endif
        public static int OverlapPointNonAlloc(Vector2 point, Collider2D[] results, int layerMask, float minDepth, float maxDepth)
        {
            var count = Physics2D.OverlapPointNonAlloc(point, results, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(count > 0);
            Draw.Point(point);
#endif
            return count;
        }
        
        #endregion
        
        #endregion
        
        #region RayCast

        #region RayCast single

        public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction)
        {
            return Raycast(origin, direction, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance)
        {
            return Raycast(origin, direction, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask)
        {
            return Raycast(origin, direction, distance, layerMask, -_maxDistance, _maxDistance);
        }

        public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask, float minDepth)
        {
            return Raycast(origin, direction, distance, layerMask, minDepth, _maxDistance);
        }

        public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
        {
            direction.Normalize();
            var hitInfo = Physics2D.Raycast(origin, direction, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            Vector3 end = origin + direction * math.min(distance, _maxDistance);
            var collided = false;

            if (hitInfo.collider != null)
            {
                collided = true;
                end = hitInfo.point;
                Draw.Color = PointColor;
                Draw.Point(end);
            }

            PhysicsDrawings.DrawRaycast2D(origin, end, PhysicsColor(collided));
#endif
            return hitInfo;
        }
        
        public static int Raycast(Vector2 origin, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results)
        {
            return Raycast(origin, direction, contactFilter, results, _maxDistance);
        }

        public static int Raycast(Vector2 origin, Vector2 direction, ContactFilter2D contactFilter, RaycastHit2D[] results, float distance)
        {
            direction.Normalize();
            var size = Physics2D.Raycast(origin, direction, contactFilter, results, distance);
#if !DONT_DRAW_PHYSICS
            Vector3 end = origin + direction * math.min(distance, _maxDistance);
            var collided = false;

            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                collided = true;
                end = hit.point;
                Draw.Color = PointColor;
                Draw.Point(end);
            }

            PhysicsDrawings.DrawRaycast2D(origin, end, PhysicsColor(collided));
#endif
            return size;
        }

        public static int Raycast(Vector2 origin, Vector2 direction, ContactFilter2D contactFilter, List<RaycastHit2D> results, float distance)
        {
            direction.Normalize();
            var size = Physics2D.Raycast(origin, direction, contactFilter, results, distance);
#if !DONT_DRAW_PHYSICS
            Vector3 end = origin + direction * math.min(distance, _maxDistance);
            var collided = false;

            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                collided = true;
                end = hit.point;
                Draw.Color = PointColor;
                Draw.Point(end);
            }

            PhysicsDrawings.DrawRaycast2D(origin, end, PhysicsColor(collided));
#endif
            return size;
        }

        #endregion
        
        #region RayCast all

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastAll is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction)
        {
            return RaycastAll(origin, direction, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastAll is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance)
        {
            return RaycastAll(origin, direction, distance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastAll is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance, int layerMask)
        {
            return RaycastAll(origin, direction, distance, layerMask, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastAll is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance, int layerMask, float minDepth)
        {
            return RaycastAll(origin, direction, distance, layerMask, minDepth, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastAll is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance, int layerMask, float minDepth, float maxDepth)
        {
            var raycastInfo = Physics2D.RaycastAll(origin, direction, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            var previewOrigin = origin;
            var sectionOrigin = origin;

            foreach (var hit in raycastInfo)
            {
                collided = true;
                
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                PhysicsDrawings.DrawRaycast2D(sectionOrigin, hit.point, PhysicsColor());

                if ((origin - hit.point).sqrMagnitude > (origin - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;

                sectionOrigin = hit.point;
            }
            
            PhysicsDrawings.DrawRaycast2D(previewOrigin, origin + direction * distance, PhysicsColor(collided));
#endif
            return raycastInfo;
        }

        #endregion
        
        #region RayCast non alloc

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastNonAlloc is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results)
        {
            return RaycastNonAlloc(origin, direction, results, _maxDistance, Physics2D.AllLayers, -_maxDistance, _maxDistance);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastNonAlloc is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results, float distance)
        {
            return RaycastNonAlloc(origin, direction, results, distance, Physics2D.AllLayers);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastNonAlloc is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask)
        {
            return RaycastNonAlloc(origin, direction, results, distance, layerMask);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastNonAlloc is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth)
        {
            return RaycastNonAlloc(origin, direction, results, distance, layerMask, minDepth);
        }

#if UNITY_2023_1_OR_NEWER
        [Obsolete("Physics2D.RaycastNonAlloc is deprecated. Use Physics2D.Raycast instead.", false)]
#endif
        public static int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask, float minDepth, float maxDepth)
        {
            var size = Physics2D.RaycastNonAlloc(origin, direction, results, distance, layerMask, minDepth, maxDepth);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            var previewOrigin = origin;
            var sectionOrigin = origin;

            for (var i = 0; i < size; i++)
            {
                collided = true;
                var hit = results[i];
                
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                PhysicsDrawings.DrawRaycast2D(sectionOrigin, hit.point, PhysicsColor());

                if ((origin - hit.point).sqrMagnitude > (origin - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;

                sectionOrigin = hit.point;
            }
            
            PhysicsDrawings.DrawRaycast2D(previewOrigin, origin + direction * distance, PhysicsColor(collided));
#endif
            return size;
        }

        #endregion

        #endregion
    }
}