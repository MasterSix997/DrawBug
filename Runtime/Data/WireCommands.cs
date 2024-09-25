using Unity.Collections;
using Unity.Mathematics;

namespace Drawbug.PhysicsExtension
{
    internal unsafe partial struct ProcessCommandsJob
    {
        private void AddLine(DrawCommandBuffer.LineData lineData)
        {
            WireBuffer.Submit(lineData.a, lineData.b, _currentStyleId);
        }

        private void AddLines(float3* linesPtr, int arrayLength)
        {
            WireBuffer.Submit(linesPtr, arrayLength, _currentStyleId);
        }
        
        // 2D Shapes
        private void AddRectangle(DrawCommandBuffer.RectangleData boxData)
        {
            var position = boxData.position;
            var rotation = boxData.rotation;
            var size = boxData.size;
            
            size *= 0.5f;

            var point1 = position + math.mul(rotation, new float3(-size.x, -size.y, 0));
            var point2 = position + math.mul(rotation, new float3(size.x, -size.y, 0));
            var point3 = position + math.mul(rotation, new float3(size.x, size.y, 0));
            var point4 = position + math.mul(rotation, new float3(-size.x, size.y, 0));

            WireBuffer.Submit(point1, point2, _currentStyleId);
            WireBuffer.Submit(point2, point3, _currentStyleId);
            WireBuffer.Submit(point3, point4, _currentStyleId);
            WireBuffer.Submit(point4, point1, _currentStyleId);
        }

        private int CircleShape(int tempOffset, float3 position, float radius, quaternion rotation)
        {
            const int segments = 16;
            
            var step = 360f / segments;
            
            for (var i = 0; i < segments; i++)
            {
                var cx = math.cos(math.radians(step) * i) * radius;
                var cy = math.sin(math.radians(step) * i) * radius;
                var current = new float3(cx, cy, 0f);

                var nx = math.cos(math.radians(step) * (i + 1)) * radius;
                var ny = math.sin(math.radians(step) * (i + 1)) * radius;
                var next = new float3(nx, ny, 0f);
                
                temp3[tempOffset + i * 2] = position + math.mul(rotation, current);
                temp3[tempOffset + i * 2 + 1] = position + math.mul(rotation, next);
            }

            return segments * 2;
        }
        
        private void AddCircle(DrawCommandBuffer.CircleData circleData)
        {
            var count = CircleShape(0, circleData.position, circleData.radius, circleData.rotation);
            WireBuffer.Submit(temp3, count, _currentStyleId);
        }

        private void AddHollowCircle(DrawCommandBuffer.HollowCircleData hollowCircleData)
        {
            var count = CircleShape(0, hollowCircleData.position, hollowCircleData.innerRadius, hollowCircleData.rotation);
            count += CircleShape(count, hollowCircleData.position, hollowCircleData.outerRadius, hollowCircleData.rotation);
            
            WireBuffer.Submit(temp3, count, _currentStyleId);
            
            // AddCircle(new DrawCommandBuffer.CircleData
            // {
            //     position = hollowCircleData.position,
            //     radius = hollowCircleData.innerRadius,
            //     rotation = hollowCircleData.rotation
            // });
            //
            // AddCircle(new DrawCommandBuffer.CircleData
            // {
            //     position = hollowCircleData.position,
            //     radius = hollowCircleData.outerRadius,
            //     rotation = hollowCircleData.rotation
            // });
        }

        private int CapsuleShape(int tempOffset, float3 position, float2 size, quaternion rotation, bool isVertical)
        {
            const int segments = 16;
            
            var step = 360f / segments;

            size /= 2;
            var point1 = float3.zero;
            var point2 = float3.zero;

            float radius;
            if (isVertical)
            {
                if (size.y > size.x)
                {
                    point1 = new float3(0, 0 - size.y + size.x, 0f);
                    point2 = new float3(0, 0 + size.y - size.x, 0f);
                }
                radius = size.x;
            }
            else
            {
                if (size.x > size.y)
                {
                    point1 = new float3(0, 0 + size.y - size.x, 0f);
                    point2 = new float3(0, 0 - size.y + size.x, 0f);
                }
                radius = size.y;
                rotation = math.mul(rotation, quaternion.RotateZ(math.radians(90)));
            }
        
            for (var i = 0; i < segments; i++)
            {
                var cx = math.cos(math.radians(step) * i) * radius;
                var cy = math.sin(math.radians(step) * i) * radius;
                var current = new float3(cx, cy, 0f);

                var nx = math.cos(math.radians(step) * (i + 1)) * radius;
                var ny = math.sin(math.radians(step) * (i + 1)) * radius;
                var next = new float3(nx, ny, 0f);

                if (current.y < -0.00001f || next.y < -0.00001f)
                {
                    current += point1;
                    next += point1;
                }
                else
                {
                    current += point2;
                    next += point2;
                }

                temp3[tempOffset + i * 2] = position + math.mul(rotation, current);
                temp3[tempOffset + i * 2 + 1] = position + math.mul(rotation, next);
            }

            if ((isVertical && size.y <= size.x) || (!isVertical && size.x <= size.y))
            {
                //WireBuffer.Submit(temp3, segments * 2, _currentStyleId);
                return segments * 2;
            }
            
            float3 topRight;
            float3 topLeft;
            float3 downRight;
            float3 downLeft;
            if (isVertical)
            {
                topRight = math.mul(rotation, new float3 (point2.x + size.x, point2.y, point2.z)) + position;
                topLeft = math.mul(rotation, new float3 (point2.x - size.x, point2.y, point2.z)) + position;
                downRight = math.mul(rotation, new float3 (point1.x + size.x, point1.y, point1.z)) + position;
                downLeft = math.mul(rotation, new float3 (point1.x - size.x, point1.y, point1.z)) + position;
            }
            else
            {
                topRight = math.mul(rotation, new float3 (point2.x + size.y, point2.y, point2.z)) + position;
                topLeft = math.mul(rotation, new float3 (point2.x - size.y, point2.y, point2.z)) + position;
                downRight = math.mul(rotation, new float3 (point1.x + size.y, point1.y, point1.z)) + position;
                downLeft = math.mul(rotation, new float3 (point1.x - size.y, point1.y, point1.z)) + position;
            }
            
            WireBuffer.Submit(topRight, downRight, _currentStyleId);
            WireBuffer.Submit(topLeft, downLeft, _currentStyleId);
            
            temp3[tempOffset + segments * 2] = topRight;
            temp3[tempOffset + segments * 2 + 1] = downRight;
            temp3[tempOffset + segments * 2 + 2] = topLeft;
            temp3[tempOffset + segments * 2 + 3] = downLeft;
            
            //WireBuffer.Submit(temp3, segments * 2 + 4, _currentStyleId);
            return segments * 2 + 4;
        }
        
        private void AddCapsule(DrawCommandBuffer.CapsuleData capsuleData)
        {
            var count = CapsuleShape(0, capsuleData.position, capsuleData.size, capsuleData.rotation, capsuleData.isVertical);
            WireBuffer.Submit(temp3, count, _currentStyleId);
        }

        // 3D Shapes
        private void AddBox(DrawCommandBuffer.BoxData boxData)
        {
            var position = boxData.position;
            var rotation = boxData.rotation;
            var size = boxData.size;

            size *= 0.5f;

            var point1 = new float3(position.x - size.x, position.y - size.y, position.z - size.z);
            var point2 = new float3(position.x - size.x, position.y + size.y, position.z - size.z);
            var point3 = new float3(position.x + size.x, position.y + size.y, position.z - size.z);
            var point4 = new float3(position.x + size.x, position.y - size.y, position.z - size.z);

            var point5 = new float3(position.x - size.x, position.y - size.y, position.z + size.z);
            var point6 = new float3(position.x - size.x, position.y + size.y, position.z + size.z);
            var point7 = new float3(position.x + size.x, position.y + size.y, position.z + size.z);
            var point8 = new float3(position.x + size.x, position.y - size.y, position.z + size.z);
            
            point1 = math.mul(rotation, point1 - position) + position;
            point2 = math.mul(rotation, point2 - position) + position;
            point3 = math.mul(rotation, point3 - position) + position;
            point4 = math.mul(rotation, point4 - position) + position;
            point5 = math.mul(rotation, point5 - position) + position;
            point6 = math.mul(rotation, point6 - position) + position;
            point7 = math.mul(rotation, point7 - position) + position;
            point8 = math.mul(rotation, point8 - position) + position;
            
            //   6---------7
            // 2---------3 |
            // | |       | |
            // | 5_______|_8
            // 1_________4
            
            var i = 0;
            // Front
            temp3[i++] = point1;
            temp3[i++] = point2;

            temp3[i++] = point2;
            temp3[i++] = point3;

            temp3[i++] = point3;
            temp3[i++] = point4;

            temp3[i++] = point4;
            temp3[i++] = point1;
                
            // Back 
            temp3[i++] = point5;
            temp3[i++] = point6;
                
            temp3[i++] = point6;
            temp3[i++] = point7;
                
            temp3[i++] = point7;
            temp3[i++] = point8;
                
            temp3[i++] = point8;
            temp3[i++] = point5;
                
            // Side Connections
            temp3[i++] = point1;
            temp3[i++] = point5;
            
            temp3[i++] = point2;
            temp3[i++] = point6;
    
            temp3[i++] = point3;
            temp3[i++] = point7;

            temp3[i++] = point4;
            temp3[i] = point8;

            WireBuffer.Submit(temp3, 24, _currentStyleId);
        }

        private void AddSphere(DrawCommandBuffer.SphereData sphereData)
        {
            var count = CircleShape(0, sphereData.position, sphereData.radius, quaternion.Euler(0, 0, 0));
            count += CircleShape(count, sphereData.position, sphereData.radius, quaternion.Euler(math.radians(90), 0, 0));
            count += CircleShape(count, sphereData.position, sphereData.radius, quaternion.Euler(0, math.radians(90), math.radians(90)));
            
            WireBuffer.Submit(temp3, count, _currentStyleId);
            // AddCircle(new DrawCommandBuffer.CircleData
            // {
            //     position = sphereData.position,
            //     radius = sphereData.radius,
            //     rotation = quaternion.Euler(0, 0, 0)
            // });
            //
            // AddCircle(new DrawCommandBuffer.CircleData
            // {
            //     position = sphereData.position,
            //     radius = sphereData.radius,
            //     rotation = quaternion.Euler(math.radians(90), 0, 0)
            // });
            //
            // AddCircle(new DrawCommandBuffer.CircleData
            // {
            //     position = sphereData.position,
            //     radius = sphereData.radius,
            //     rotation = quaternion.Euler(0, math.radians(90), math.radians(90))
            // });
        }

        private void AddCylinder(DrawCommandBuffer.CylinderData cylinderData)
        {
            var position = cylinderData.position;
            var radius = cylinderData.radius;
            var height = cylinderData.height;
            var rotation = cylinderData.rotation;
            
            height /= 2;

            var circleRotation = math.mul(rotation, quaternion.RotateX(math.radians(90)));

            AddCircle(new DrawCommandBuffer.CircleData
            {
                position = position + math.mul(rotation, new float3(0, height, 0)),
                radius = radius,
                rotation = circleRotation
            });
            
            AddCircle(new DrawCommandBuffer.CircleData
            {
                position = position + math.mul(rotation, new float3(0, -height, 0)),
                radius = radius,
                rotation = circleRotation
            });

            var step = 360f / 4;

            for (var i = 0; i < 4; i++)
            {
                var angle = math.radians(step) * i;
                var x = math.cos(angle) * radius;
                var y = math.sin(angle) * radius;

                var startPoint = position + math.mul(rotation, new float3(x, height, y));
                var endPoint = position + math.mul(rotation, new float3(x, -height, y));

                WireBuffer.Submit(startPoint, endPoint, _currentStyleId);
            }
        }

        private void AddCapsule3D(DrawCommandBuffer.CylinderData capsuleData)
        {
            var position = capsuleData.position;
            var radius = capsuleData.radius;
            var height = capsuleData.height;
            var rotation = capsuleData.rotation;
            
            var size = new float2(radius * 2, height);

            var count = CapsuleShape(0, position, size, rotation, true);
            count += CapsuleShape(count, position, size, math.mul(rotation, quaternion.RotateY(math.radians(90))), true);
            // AddCapsule(new DrawCommandBuffer.CapsuleData
            // {
            //     position = position,
            //     size = size,
            //     rotation = rotation,
            //     isVertical = true
            // });
            // AddCapsule(new DrawCommandBuffer.CapsuleData
            // {
            //     position = position,
            //     size = size,
            //     rotation = math.mul(rotation, quaternion.RotateY(math.radians(90))),
            //     isVertical = true
            // });

            size /= 2;
            if (height > radius * 2)
            {
                count += CircleShape(count, position + math.mul(rotation, new float3(0, size.y - size.x, 0)), radius, math.mul(rotation, quaternion.RotateX(math.radians(90))));
                count += CircleShape(count, position + math.mul(rotation, new float3(0, -size.y + size.x, 0)), radius, math.mul(rotation, quaternion.RotateX(math.radians(90))));
                // AddCircle(new DrawCommandBuffer.CircleData
                // {
                //     position = position + math.mul(rotation, new float3(0, size.y - size.x, 0)),
                //     radius = radius,
                //     rotation = math.mul(rotation, quaternion.RotateX(math.radians(90)))
                // });
                // AddCircle(new DrawCommandBuffer.CircleData
                // {
                //     position = position + math.mul(rotation, -new float3(0, size.y - size.x, 0)),
                //     radius = radius,
                //     rotation = math.mul(rotation, quaternion.RotateX(math.radians(90)))
                // });
            }
            else
            {
                count += CircleShape(count, position, radius, math.mul(rotation, quaternion.RotateX(math.radians(90))));
                // AddCircle(new DrawCommandBuffer.CircleData
                // {
                //     position = position,
                //     radius = radius,
                //     rotation = math.mul(rotation, quaternion.RotateX(math.radians(90)))
                // });
            }
            
            WireBuffer.Submit(temp3, count, _currentStyleId);
        }
    }
}