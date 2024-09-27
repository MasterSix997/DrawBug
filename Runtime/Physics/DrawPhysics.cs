using Unity.Mathematics;
using UnityEngine;

namespace Drawbug.PhysicsExtension
{
    public static class DrawPhysics
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
        private static Quaternion _orientation = Quaternion.identity;
        private static int _layerMask = -1;
        private static QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
        
        #region BoxCast

        #region Boxcast single
        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction)
        {
            return BoxCast(center, halfExtents, direction, out _, _orientation, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation)
        {
            return BoxCast(center, halfExtents, direction, out _, orientation, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit rayInfo)
        {
            return BoxCast(center, halfExtents, direction, out rayInfo, _orientation, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance)
        {
            return BoxCast(center, halfExtents, direction, out _, orientation, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit rayInfo, Quaternion orientation)
        {
            return BoxCast(center, halfExtents, direction, out rayInfo, orientation, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, int layerMask)
        {
            return BoxCast(center, halfExtents, direction, out _, orientation, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit rayInfo, Quaternion orientation, float maxDistance)
        {
            return BoxCast(center, halfExtents, direction, out rayInfo, orientation, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return BoxCast(center, halfExtents, direction, out _, orientation, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit rayInfo, Quaternion orientation, float maxDistance, int layerMask)
        {
            return BoxCast(center, halfExtents, direction, out rayInfo, orientation, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool BoxCast(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hitInfo,
            Quaternion orientation, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var collided = Physics.BoxCast(center, halfExtents, direction, out hitInfo, orientation, maxDistance,
                layerMask, queryTriggerInteraction);

#if !DONT_DRAW_PHYSICS
            maxDistance = Mathf.Min(maxDistance, _maxDistance);

            if (collided)
            {
                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
                maxDistance = hitInfo.distance;
            }

            PhysicsDrawings.DrawBoxCast(center, halfExtents, direction, maxDistance, orientation, PhysicsColor(collided));
#endif
        return collided;
        }
        #endregion

        #region Boxcast all
        public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction)
        {
            return BoxCastAll(center, halfExtents, direction, _orientation, _maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation)
        {
            return BoxCastAll(center, halfExtents, direction, orientation, _maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance)
        {
            return BoxCastAll(center, halfExtents, direction, orientation, maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, LayerMask layerMask)
        {
            return BoxCastAll(center, halfExtents, direction, orientation, maxDistance, layerMask, _queryTriggerInteraction);
        }
        
        public static RaycastHit[] BoxCastAll(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var hitInfo = Physics.BoxCastAll(center, halfExtents, direction, orientation, maxDistance, layerMask, queryTriggerInteraction);
        
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;
            
            foreach (var hit in hitInfo)
            {
                collided = true;
    
                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;
    
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Box(center + direction * hit.distance, halfExtents * 2, orientation);
            }
        
            PhysicsDrawings.DrawBoxCast(center, halfExtents, direction, maxDistance, orientation, PhysicsColor(collided));
#endif
            return hitInfo;
        }
        #endregion
        
        #region Boxcast non alloc
        public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results)
        {
            return BoxCastNonAlloc(center, halfExtents, direction, results, _orientation, _maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, Quaternion orientation)
        {
            return BoxCastNonAlloc(center, halfExtents, direction, results, orientation, _maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, Quaternion orientation, float maxDistance)
        {
            return BoxCastNonAlloc(center, halfExtents, direction, results, orientation, maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, Quaternion orientation, float maxDistance, int layerMask)
        {
            return BoxCastNonAlloc(center, halfExtents, direction, results, orientation, maxDistance, layerMask, _queryTriggerInteraction);
        }
        
        public static int BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, RaycastHit[] results, Quaternion orientation, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var size = Physics.BoxCastNonAlloc(center, halfExtents, direction, results, orientation, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;
    
            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                collided = true;
    
                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;
    
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Box(center + direction * hit.distance, halfExtents * 2, orientation);
            }
    
            PhysicsDrawings.DrawBoxCast(center, halfExtents, direction, maxDistance, orientation, PhysicsColor(collided));
#endif
            return size;
        }
        #endregion

        #endregion
        
        #region Capsule Cast

        #region Capsulecast single
        public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction)
        {
            return CapsuleCast(point1, point2, radius, direction, out _, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance)
        {
            return CapsuleCast(point1, point2, radius, direction, out _, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo)
        {
            return CapsuleCast(point1, point2, radius, direction, out hitInfo, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask)
        {
            return CapsuleCast(point1, point2, radius, direction, out _, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance)
        {
            return CapsuleCast(point1, point2, radius, direction, out hitInfo, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return CapsuleCast(point1, point2, radius, direction, out _, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            return CapsuleCast(point1, point2, radius, direction, out hitInfo, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool CapsuleCast(Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var collided = Physics.CapsuleCast(point1, point2, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            
            maxDistance = Mathf.Min(maxDistance, _maxDistance);

            if (collided)
            {
                maxDistance = hitInfo.distance;
                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
            }

            PhysicsDrawings.DrawCapsuleCast(point1, point2, radius, direction, maxDistance, PhysicsColor(collided));
#endif
            return collided;
        }
        #endregion

        #region Capsulecast all
        public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction)
        {
            return CapsuleCastAll(point1, point2, radius, direction, _maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance)
        {
            return CapsuleCastAll(point1, point2, radius, direction, maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask)
        {
            return CapsuleCastAll(point1, point2, radius, direction, maxDistance, layerMask, _queryTriggerInteraction);
        }
        
        public static RaycastHit[] CapsuleCastAll(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var hitInfo = Physics.CapsuleCastAll(point1, point2, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;
        
            foreach (var hit in hitInfo)
            {
                collided = true;
        
                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;
        
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                
                var midPoint = (point1 + point2) / 2f;
                var height = Vector3.Distance(point1, point2);
                var capsuleDirection = point2 - point1;
                var rotation = point1 == point2 ? Quaternion.identity : Quaternion.LookRotation(capsuleDirection, Vector3.up) * Quaternion.Euler(0, 90, 90);
                var finalPosition = direction * hit.distance + midPoint;
                Draw.Color = PhysicsColor();
                Draw.Capsule3D(finalPosition, radius, height + radius * 2, rotation);
            }
        
            maxDistance = Mathf.Min(maxDistance, _maxDistance);
            
            PhysicsDrawings.DrawCapsuleCast(point1, point2, radius, direction, maxDistance, PhysicsColor(collided));
#endif
            return hitInfo;
        }
        #endregion
        
        #region Capsulecast non alloc
        public static int CapsuleCastNonAlloc(Vector3 point1, Vector3 point2, float radius, Vector3 direction, RaycastHit[] results)
        {
            return CapsuleCastNonAlloc(point1, point2, radius, direction, results, _maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static int CapsuleCastNonAlloc(Vector3 point1, Vector3 point2, float radius, Vector3 direction, RaycastHit[] results, float maxDistance)
        {
            return CapsuleCastNonAlloc(point1, point2, radius, direction, results, maxDistance, _layerMask, _queryTriggerInteraction);
        }
        
        public static int CapsuleCastNonAlloc(Vector3 point1, Vector3 point2, float radius, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask)
        {
            return CapsuleCastNonAlloc(point1, point2, radius, direction, results, maxDistance, layerMask, _queryTriggerInteraction);
        }
        
        public static int CapsuleCastNonAlloc(Vector3 point1, Vector3 point2, float radius, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var size = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;
        
            for (var i = 0; i < size; i++)
            {
                collided = true;
        
                var hit = results[i];
        
                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;
        
                Draw.Color = PointColor;
                Draw.Point(hit.point);
                
                var midPoint = (point1 + point2) / 2f;
                var height = Vector3.Distance(point1, point2);
                var capsuleDirection = point2 - point1;
                var rotation = point1 == point2 ? Quaternion.identity : Quaternion.LookRotation(capsuleDirection, Vector3.up) * Quaternion.Euler(0, 90, 90);
                var finalPosition = direction * hit.distance + midPoint;
                Draw.Color = PhysicsColor();
                Draw.Capsule3D(finalPosition, radius, height + radius * 2, rotation);
            }
        
            maxDistance = Mathf.Min(maxDistance, _maxDistance);
            
            PhysicsDrawings.DrawCapsuleCast(point1, point2, radius, direction, maxDistance, PhysicsColor(collided));
#endif
            return size;
        }
        #endregion

        #endregion
        
        #region Check Box
        public static bool CheckBox(Vector3 center, Vector3 halfExtents)
        {
            return CheckBox(center, halfExtents, _orientation, _layerMask, _queryTriggerInteraction);
        }

        public static bool CheckBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
        {
            return CheckBox(center, halfExtents, orientation, _layerMask, _queryTriggerInteraction);
        }

        public static bool CheckBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask)
        {
            return CheckBox(center, halfExtents, orientation, layerMask, _queryTriggerInteraction);
        }

        public static bool CheckBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var collided = Physics.CheckBox(center, halfExtents, orientation, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(collided);
            Draw.Box(center, halfExtents * 2, orientation);
#endif
            return collided;
        }
        #endregion
        
        #region Check Capsule
        public static bool CheckCapsule(Vector3 start, Vector3 end, float radius)
        {
            return CheckCapsule(start, end, radius, _layerMask, _queryTriggerInteraction);
        }

        public static bool CheckCapsule(Vector3 start, Vector3 end, float radius, int layerMask)
        {
            return CheckCapsule(start, end, radius, layerMask, _queryTriggerInteraction);
        }

        public static bool CheckCapsule(Vector3 start, Vector3 end, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var collided = Physics.CheckCapsule(start, end, radius, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var midPoint = (start + end) / 2f;
            var height = Vector3.Distance(start, end);
            var capsuleDirection = end - start;
            var rotation = start == end ? Quaternion.identity : Quaternion.LookRotation(capsuleDirection, Vector3.up) * Quaternion.Euler(0, 90, 90);
            
            Draw.Color = PhysicsColor(collided);
            Draw.Capsule3D(midPoint, radius, height + radius * 2, rotation);
#endif
            return collided;
        }
        #endregion

        #region Check Sphere
        public static bool CheckSphere(Vector3 position, float radius)
        {
            return CheckSphere(position, radius, _layerMask, _queryTriggerInteraction);
        }

        public static bool CheckSphere(Vector3 position, float radius, int layerMask)
        {
            return CheckSphere(position, radius, layerMask, _queryTriggerInteraction);
        }

        public static bool CheckSphere(Vector3 position, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var collided = Physics.CheckSphere(position, radius, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(collided);
            Draw.Sphere(position, radius);
#endif
            return collided;
        }
        #endregion

        #region Linecast
        public static bool Linecast(Vector3 start, Vector3 end)
        {
            return Linecast(start, end, out _, _layerMask, _queryTriggerInteraction);
        }

        public static bool Linecast(Vector3 start, Vector3 end, int layerMask)
        {
            return Linecast(start, end, out _, layerMask, _queryTriggerInteraction);
        }

        public static bool Linecast(Vector3 start, Vector3 end, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return Linecast(start, end, out _, layerMask, queryTriggerInteraction);
        }

        public static bool Linecast(Vector3 start, Vector3 end, out RaycastHit hitInfo)
        {
            return Linecast(start, end, out hitInfo, _layerMask, _queryTriggerInteraction);
        }

        public static bool Linecast(Vector3 start, Vector3 end, out RaycastHit hitInfo, int layerMask)
        {
            return Linecast(start, end, out hitInfo, layerMask, _queryTriggerInteraction);
        }

        public static bool Linecast(Vector3 start, Vector3 end, out RaycastHit hitInfo, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var collided = Physics.Linecast(start, end, out hitInfo, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            
            if (collided)
            {
                end = hitInfo.point;

                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
            }

            Draw.Color = PhysicsColor(collided);
            Draw.Line(start, end);
#endif
            return collided;
        }
        #endregion

        #region Overlap Box
        #region OverlapBox alloc
        public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents)
        {
            return OverlapBox(center, halfExtents, _orientation, _layerMask, _queryTriggerInteraction);
        }

        public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
        {
            return OverlapBox(center, halfExtents, orientation, _layerMask, _queryTriggerInteraction);
        }

        public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask)
        {
            return OverlapBox(center, halfExtents, orientation, layerMask, _queryTriggerInteraction);
        }

        public static Collider[] OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var colliders = Physics.OverlapBox(center, halfExtents, orientation, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(colliders.Length > 0);
            Draw.Box(center, halfExtents * 2, orientation);
#endif
            return colliders;
        }
        #endregion

        #region OverlapBox non alloc
        public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results)
        {
            return OverlapBoxNonAlloc(center, halfExtents, results, _orientation, _layerMask, _queryTriggerInteraction);
        }

        public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation)
        {
            return OverlapBoxNonAlloc(center, halfExtents, results, orientation, _layerMask, _queryTriggerInteraction);
        }

        public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation, int layerMask)
        {
            return OverlapBoxNonAlloc(center, halfExtents, results, orientation, layerMask, _queryTriggerInteraction);
        }

        public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var size = Physics.OverlapBoxNonAlloc(center, halfExtents, results, orientation, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(size > 0);
            Draw.Box(center, halfExtents * 2, orientation);
#endif
            return size;
        }
        #endregion
        #endregion

        #region Overlap Capsule
        #region OverlapCapsule alloc
        public static Collider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius)
        {
            return OverlapCapsule(point0, point1, radius, _layerMask, _queryTriggerInteraction);
        }

        public static Collider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius, int layerMask)
        {
            return OverlapCapsule(point0, point1, radius, layerMask, _queryTriggerInteraction);
        }

        public static Collider[] OverlapCapsule(Vector3 point0, Vector3 point1, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var colliders = Physics.OverlapCapsule(point0, point1, radius, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var midPoint = (point0 + point1) / 2f;
            var height = Vector3.Distance(point0, point1);
            var capsuleDirection = point1 - point0;
            var rotation = point0 == point1 ? Quaternion.identity : Quaternion.LookRotation(capsuleDirection, Vector3.up) * Quaternion.Euler(0, 90, 90);
            Draw.Color = PhysicsColor(colliders.Length > 0);
            Draw.Capsule3D(midPoint, radius, height + radius * 2, rotation);
#endif
            return colliders;
        }
        #endregion

        #region OverlapCapsule non alloc
        public static int OverlapCapsuleNonAlloc(Vector3 point0, Vector3 point1, float radius, Collider[] results)
        {
            return OverlapCapsuleNonAlloc(point0, point1, radius, results, _layerMask, _queryTriggerInteraction);
        }

        public static int OverlapCapsuleNonAlloc(Vector3 point0, Vector3 point1, float radius, Collider[] results, int layerMask)
        {
            return OverlapCapsuleNonAlloc(point0, point1, radius, results, layerMask, _queryTriggerInteraction);
        }

        public static int OverlapCapsuleNonAlloc(Vector3 point0, Vector3 point1, float radius, Collider[] results, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var size = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, results, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var midPoint = (point0 + point1) / 2f;
            var height = Vector3.Distance(point0, point1);
            var capsuleDirection = point1 - point0;
            var rotation = point0 == point1 ? Quaternion.identity : Quaternion.LookRotation(capsuleDirection, Vector3.up) * Quaternion.Euler(0, 90, 90);
            Draw.Color = PhysicsColor(size > 0);
            Draw.Capsule3D(midPoint, radius, height + radius * 2, rotation);
#endif
            return size;
        }
        #endregion
        #endregion

        #region Overlap Sphere
        #region OverlapSphere alloc
        public static Collider[] OverlapSphere(Vector3 position, float radius)
        {
            return OverlapSphere(position, radius, _layerMask, _queryTriggerInteraction);
        }

        public static Collider[] OverlapSphere(Vector3 position, float radius, int layerMask)
        {
            return OverlapSphere(position, radius, layerMask, _queryTriggerInteraction);
        }

        public static Collider[] OverlapSphere(Vector3 position, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var colliders = Physics.OverlapSphere(position, radius, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(colliders.Length > 0);
            Draw.Sphere(position, radius);
#endif
            return colliders;
        }
        #endregion

        #region OverlapSphere non alloc
        public static int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results)
        {
            return OverlapSphereNonAlloc(position, radius, results, _layerMask, _queryTriggerInteraction);
        }

        public static int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, int layerMask)
        {
            return OverlapSphereNonAlloc(position, radius, results, layerMask, _queryTriggerInteraction);
        }

        public static int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var size = Physics.OverlapSphereNonAlloc(position, radius, results, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            Draw.Color = PhysicsColor(size > 0);
            Draw.Sphere(position, radius);
#endif
            return size;
        }
        #endregion
        #endregion
        
        #region Raycast

        #region Raycast single
        #region Vector3
        public static bool Raycast(Vector3 origin, Vector3 direction)
        {
            return Raycast(origin, direction, out _, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance)
        {
            return Raycast(origin, direction, out _, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask)
        {
            return Raycast(origin, direction, out _, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return Raycast(origin, direction, out _, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo)
        {
            return Raycast(origin, direction, out hitInfo, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance)
        {
            return Raycast(origin, direction, out hitInfo, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            return Raycast(origin, direction, out hitInfo, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var collided = Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            
            var end = origin + direction * math.min(maxDistance, _maxDistance);

            if (collided)
            {
                end = hitInfo.point;
                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
            }

            PhysicsDrawings.DrawRaycast(origin, end, PhysicsColor(collided));
#endif
            return collided;
        }
        #endregion

        #region Ray
        public static bool Raycast(Ray ray)
        {
            return Raycast(ray, out _, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Ray ray, float maxDistance)
        {
            return Raycast(ray, out _, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Ray ray, out RaycastHit hitInfo)
        {
            return Raycast(ray, out hitInfo, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Ray ray, float maxDistance, int layerMask)
        {
            return Raycast(ray, out _, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
        {
            return Raycast(ray, out hitInfo, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Ray ray, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return Raycast(ray, out _, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            return Raycast(ray, out hitInfo, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var collided = Physics.Raycast(ray, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            
            var end = ray.origin + ray.direction * math.min(maxDistance, _maxDistance);

            if (collided)
            {
                end = hitInfo.point;

                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
            }

            PhysicsDrawings.DrawRaycast(ray.origin, end, PhysicsColor(collided));
#endif
            return collided;
        }

        #endregion
        #endregion
        
        #region Raycast all
        #region Vector3
        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction)
        {
            return RaycastAll(origin, direction, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance)
        {
            return RaycastAll(origin, direction, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask)
        {
            return RaycastAll(origin, direction, maxDistance, (int)layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var raycastInfo = Physics.RaycastAll(origin, direction, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var end = origin + direction * math.min(maxDistance, _maxDistance);
            var previewOrigin = origin;
            var sectionOrigin = origin;

            foreach (var hit in raycastInfo)
            {
                Draw.Color = PointColor;
                Draw.Point(hit.point);

                PhysicsDrawings.DrawRaycast(sectionOrigin, hit.point, PhysicsColor());

                if ((origin - hit.point).sqrMagnitude > (origin - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;

                sectionOrigin = hit.point;
            }

            PhysicsDrawings.DrawRaycast(previewOrigin, end, PhysicsColor(raycastInfo.Length > 0));
#endif
            return raycastInfo;
        }
        #endregion

        #region Ray
        public static RaycastHit[] RaycastAll(Ray ray)
        {
            return RaycastAll(ray, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] RaycastAll(Ray ray, float maxDistance)
        {
            return RaycastAll(ray, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] RaycastAll(Ray ray, float maxDistance, LayerMask layerMask)
        {
            return RaycastAll(ray, maxDistance, (int)layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] RaycastAll(Ray ray, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var raycastInfo = Physics.RaycastAll(ray, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var end = ray.origin + ray.direction * math.min(maxDistance, _maxDistance);
            var previewOrigin = ray.origin;
            var sectionOrigin = ray.origin;

            foreach (var hit in raycastInfo)
            {
                Draw.Color = PointColor;
                Draw.Point(hit.point);

                PhysicsDrawings.DrawRaycast(sectionOrigin, hit.point, PhysicsColor());

                if ((ray.origin - hit.point).sqrMagnitude > (ray.origin - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;

                sectionOrigin = hit.point;
            }

            PhysicsDrawings.DrawRaycast(previewOrigin, end, PhysicsColor(raycastInfo.Length > 0));
#endif
            return raycastInfo;
        }
        #endregion
        #endregion

        #region Raycast non alloc
        #region Vector3
        public static int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results)
        {
            return RaycastNonAlloc(origin, direction, results, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results, float maxDistance)
        {
            return RaycastNonAlloc(origin, direction, results, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results, float maxDistance, LayerMask layerMask)
        {
            return RaycastNonAlloc(origin, direction, results, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var size = Physics.RaycastNonAlloc(origin, direction, results, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var end = origin + direction * math.min(maxDistance, _maxDistance);
            var previewOrigin = origin;
            var sectionOrigin = origin;

            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                Draw.Color = PointColor;
                Draw.Point(hit.point);

                PhysicsDrawings.DrawRaycast(sectionOrigin, hit.point, PhysicsColor());

                if ((origin - hit.point).sqrMagnitude > (origin - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;

                sectionOrigin = hit.point;
            }

            PhysicsDrawings.DrawRaycast(previewOrigin, end, PhysicsColor(size > 0));
#endif
            return size;
        }
        #endregion

        #region Ray
        public static int RaycastNonAlloc(Ray ray, RaycastHit[] results)
        {
            return RaycastNonAlloc(ray, results, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static int RaycastNonAlloc(Ray ray, RaycastHit[] results, float maxDistance)
        {
            return RaycastNonAlloc(ray, results, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static int RaycastNonAlloc(Ray ray, RaycastHit[] results, float maxDistance, LayerMask layerMask)
        {
            return RaycastNonAlloc(ray, results, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static int RaycastNonAlloc(Ray ray, RaycastHit[] results, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var size = Physics.RaycastNonAlloc(ray, results, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var end = ray.origin + ray.direction * math.min(maxDistance, _maxDistance);
            var previewOrigin = ray.origin;
            var sectionOrigin = ray.origin;

            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                Draw.Color = PointColor;
                Draw.Point(hit.point);

                PhysicsDrawings.DrawRaycast(sectionOrigin, hit.point, PhysicsColor());

                if ((ray.origin - hit.point).sqrMagnitude > (ray.origin - previewOrigin).sqrMagnitude)
                    previewOrigin = hit.point;

                sectionOrigin = hit.point;
            }

            PhysicsDrawings.DrawRaycast(previewOrigin, end, PhysicsColor(size > 0));
#endif
            return size;
        }
        #endregion
        #endregion

        #endregion

        #region Sphere Cast
        #region Spherecast single
        #region Vector3
        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction)
        {
            return SphereCast(origin, radius, direction, out _, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, float maxDistance)
        {
            return SphereCast(origin, radius, direction, out _, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask)
        {
            return SphereCast(origin, radius, direction, out _, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return SphereCast(origin, radius, direction, out _, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo)
        {
            return SphereCast(origin, radius, direction, out hitInfo, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance)
        {
            return SphereCast(origin, radius, direction, out hitInfo, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            return SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var collided = Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            
            maxDistance = Mathf.Min(maxDistance, _maxDistance);

            if (collided)
            {
                maxDistance = hitInfo.distance;
                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
            }

            PhysicsDrawings.DrawSphereCast(origin, radius, direction, maxDistance, PhysicsColor(collided));
#endif
            return collided;
        }
        #endregion

        #region Ray
        public static bool SphereCast(Ray ray, float radius)
        {
            return SphereCast(ray, radius, out _, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Ray ray, float radius, float maxDistance)
        {
            return SphereCast(ray, radius, out _, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Ray ray, float radius, out RaycastHit hitInfo)
        {
            return SphereCast(ray, radius, out hitInfo, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Ray ray, float radius, float maxDistance, int layerMask)
        {
            return SphereCast(ray, radius, out _, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Ray ray, float radius, out RaycastHit hitInfo, float maxDistance)
        {
            return SphereCast(ray, radius, out hitInfo, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Ray ray, float radius, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return SphereCast(ray, radius, out _, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static bool SphereCast(Ray ray, float radius, out RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            return SphereCast(ray, radius, out hitInfo, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static bool SphereCast(Ray ray, float radius, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var collided = Physics.SphereCast(ray, radius, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            
            maxDistance = Mathf.Min(maxDistance, _maxDistance);

            if (collided)
            {
                maxDistance = hitInfo.distance;
                Draw.Color = PointColor;
                Draw.Point(hitInfo.point);
            }
            
            PhysicsDrawings.DrawSphereCast(ray.origin, radius, ray.direction, maxDistance, PhysicsColor(collided));
#endif
            return collided;
        }
        #endregion
        #endregion

        #region Spherecast all
        #region Vector3
        public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction)
        {
            return SphereCastAll(origin, radius, direction, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction, float maxDistance)
        {
            return SphereCastAll(origin, radius, direction, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask)
        {
            return SphereCastAll(origin, radius, direction, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] SphereCastAll(Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var hitInfo = Physics.SphereCastAll(origin, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;

            foreach (var hit in hitInfo)
            {
                collided = true;

                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;

                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Sphere(origin + direction * hit.distance, radius);
            }

            maxDistance = Mathf.Min(maxDistance, _maxDistance);

            PhysicsDrawings.DrawSphereCast(origin, radius, direction, maxDistance, PhysicsColor(collided));
#endif
            return hitInfo;
        }
        #endregion

        #region Ray
        public static RaycastHit[] SphereCastAll(Ray ray, float radius)
        {
            return SphereCastAll(ray, radius, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] SphereCastAll(Ray ray, float radius, float maxDistance)
        {
            return SphereCastAll(ray, radius, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] SphereCastAll(Ray ray, float radius, float maxDistance, int layerMask)
        {
            return SphereCastAll(ray, radius, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static RaycastHit[] SphereCastAll(Ray ray, float radius, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var hitInfo = Physics.SphereCastAll(ray, radius, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;

            foreach (var hit in hitInfo)
            {
                collided = true;

                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;

                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Sphere(ray.origin + ray.direction * hit.distance, radius);
            }

            maxDistance = Mathf.Min(maxDistance, _maxDistance);
            
            PhysicsDrawings.DrawSphereCast(ray.origin, radius, ray.direction, maxDistance, PhysicsColor(collided));
#endif
            return hitInfo;
        }
        #endregion
        #endregion

        #region Spherecast non alloc
        #region Vector3
        public static int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] results)
        {
            return SphereCastNonAlloc(origin, radius, direction, results, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] results, float maxDistance)
        {
            return SphereCastNonAlloc(origin, radius, direction, results, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask)
        {
            return SphereCastNonAlloc(origin, radius, direction, results, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            direction.Normalize();
            var size = Physics.SphereCastNonAlloc(origin, radius, direction, results, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;

            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                collided = true;

                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;

                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Sphere(origin + direction * hit.distance, radius);
            }

            maxDistance = Mathf.Min(maxDistance, _maxDistance);
            
            PhysicsDrawings.DrawSphereCast(origin, radius, direction, maxDistance, PhysicsColor(collided));
#endif
            return size;
        }
        #endregion

        #region Ray
        public static int SphereCastNonAlloc(Ray ray, float radius, RaycastHit[] results)
        {
            return SphereCastNonAlloc(ray, radius, results, _maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static int SphereCastNonAlloc(Ray ray, float radius, RaycastHit[] results, float maxDistance)
        {
            return SphereCastNonAlloc(ray, radius, results, maxDistance, _layerMask, _queryTriggerInteraction);
        }

        public static int SphereCastNonAlloc(Ray ray, float radius, RaycastHit[] results, float maxDistance, int layerMask)
        {
            return SphereCastNonAlloc(ray, radius, results, maxDistance, layerMask, _queryTriggerInteraction);
        }

        public static int SphereCastNonAlloc(Ray ray, float radius, RaycastHit[] results, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var size = Physics.SphereCastNonAlloc(ray, radius, results, maxDistance, layerMask, queryTriggerInteraction);
#if !DONT_DRAW_PHYSICS
            var collided = false;
            float maxDistanceRay = 0;

            for (var i = 0; i < size; i++)
            {
                var hit = results[i];
                collided = true;

                if (hit.distance > maxDistanceRay)
                    maxDistanceRay = hit.distance;

                Draw.Color = PointColor;
                Draw.Point(hit.point);
                Draw.Color = PhysicsColor();
                Draw.Sphere(ray.origin + ray.direction * hit.distance, radius);
            }

            maxDistance = Mathf.Min(maxDistance, _maxDistance);
            
            PhysicsDrawings.DrawSphereCast(ray.origin, radius, ray.direction, maxDistance, PhysicsColor(collided));
#endif
            return size;
        }
        #endregion
        #endregion
        
        #endregion
    }
}