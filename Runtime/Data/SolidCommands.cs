using Unity.Collections;
using Unity.Mathematics;

namespace Drawbug
{
    internal unsafe partial struct ProcessCommandsJob
    {
        // 2D Shapes
        private void AddSolidRectangle(DrawCommandBuffer.BoxData boxData)
        {
            var position = boxData.position;
            var rotation = boxData.rotation;
            var size = boxData.size;
            
            size *= 0.5f;

            var point1 = position + math.mul(rotation, new float3(-size.x, -size.y, 0));
            var point2 = position + math.mul(rotation, new float3(size.x, -size.y, 0));
            var point3 = position + math.mul(rotation, new float3(size.x, size.y, 0));
            var point4 = position + math.mul(rotation, new float3(-size.x, size.y, 0));

            var vCount = SolidBuffer.VerticesLength;
            
            SolidBuffer.Submit(point1, _currentStyleId);
            SolidBuffer.Submit(point2, _currentStyleId);
            SolidBuffer.Submit(point3, _currentStyleId);
            SolidBuffer.Submit(point4, _currentStyleId);
            
            SolidBuffer.Submit(vCount + 0);
            SolidBuffer.Submit(vCount + 2);
            SolidBuffer.Submit(vCount + 1);
            
            SolidBuffer.Submit(vCount + 0);
            SolidBuffer.Submit(vCount + 3);
            SolidBuffer.Submit(vCount + 2);
        }

        private int2 SolidCircleShape(int2 tempOffset, float3 position, float radius, quaternion rotation)
        {
            const int segments = 16;
            
            var step = 360f / segments;
            
            var vCount = SolidBuffer.VerticesLength + tempOffset.x;
            var currentTriangle = 0;
            
            for (var i = 0; i < segments; i++)
            {
                var cx = math.cos(math.radians(step) * i) * radius;
                var cy = math.sin(math.radians(step) * i) * radius;
                var current = new float3(cx, cy, 0f);
                
                temp3[tempOffset.x + i] = position + math.mul(rotation, current);
                if (i + 2 < segments)
                {
                    tempT[tempOffset.y + currentTriangle++] = vCount + 0;
                    tempT[tempOffset.y + currentTriangle++] = i + vCount + 2;
                    tempT[tempOffset.y + currentTriangle++] = i + vCount + 1;
                }
            }

            return new int2(segments, currentTriangle);
        }
        
        private void AddSolidCircle(DrawCommandBuffer.CircleData circleData)
        {
            var count = SolidCircleShape(int2.zero, circleData.position, circleData.radius, circleData.rotation);
            SolidBuffer.Submit(temp3, count.x, _currentStyleId);
            SolidBuffer.Submit(tempT, count.y);
        }
        
        private void AddSolidHollowCircle(DrawCommandBuffer.HollowCircleData circleData)
        {
            const int segments = 16;
            
            var position = circleData.position;
            var rotation = circleData.rotation;
            var innerRadius = circleData.innerRadius;
            var outerRadius = circleData.outerRadius;
            
            var step = 360f / segments;
            
            var vCount = SolidBuffer.VerticesLength;
            var currentTriangle = 0;
            
            for (var i = 0; i < segments; i++)
            {
                var innerX = math.cos(math.radians(step) * i) * innerRadius;
                var innerY = math.sin(math.radians(step) * i) * innerRadius;
                var innerPos = new float3(innerX, innerY, 0f);
                
                var outerX = math.cos(math.radians(step) * i) * outerRadius;
                var outerY = math.sin(math.radians(step) * i) * outerRadius;
                var outerPos = new float3(outerX, outerY, 0f);
                
                var outerIndex = i;
                var innerIndex = i + segments;
                
                temp3[outerIndex] = position + math.mul(rotation, innerPos);
                temp3[innerIndex] = position + math.mul(rotation, outerPos);
                
                tempT[currentTriangle++] = vCount + outerIndex;
                tempT[currentTriangle++] = (i + 1) % segments + vCount;
                tempT[currentTriangle++] = innerIndex + vCount;
                
                tempT[currentTriangle++] = vCount + outerIndex;
                tempT[currentTriangle++] = vCount + outerIndex + segments;
                tempT[currentTriangle++] = segments + vCount + (segments + i - 1) % segments;
            }
            
            SolidBuffer.Submit(temp3, segments * 2, _currentStyleId);
            SolidBuffer.Submit(tempT, currentTriangle);
        }

        private void AddSolidCapsule(DrawCommandBuffer.CapsuleData capsuleData)
        {
            const int segments = 16;

            var position = capsuleData.position;
            var size = capsuleData.size;
            var rotation = capsuleData.rotation;
            var isVertical = capsuleData.isVertical;
            
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

            var vCount = SolidBuffer.VerticesLength;
            var currentTriangle = 0;
            
            var extraVerticesPos = float3.zero;
            var extraVerticesPos2 = float3.zero;
            var extraVerticesindex = 0;
            var extraVerticesindex2 = 0;
            
            for (var i = 0; i < segments; i++)
            {
                var cx = math.cos(math.radians(step) * i) * radius;
                var cy = math.sin(math.radians(step) * i) * radius;
                var current = new float3(cx, cy, 0f);

                if (current.y < -0.00001f)
                {
                    current += point1;
                }
                else
                {
                    current += point2;
                }
                temp3[i] = position + math.mul(rotation, current);

                if (i == (segments + 2) / 2)
                {
                    var bx = math.cos(math.radians(step) * (i - 1)) * radius;
                    var by = math.sin(math.radians(step) * (i - 1)) * radius;
                    var before = new float3(bx, by, 0f);
                    before += point1;
                    extraVerticesPos = position + math.mul(rotation, before);
                    extraVerticesindex = i;
                }

                if (i + 1 == segments)
                {
                    var nx = math.cos(math.radians(step) * (0)) * radius;
                    var ny = math.sin(math.radians(step) * (0)) * radius;
                    var next = new float3(nx, ny, 0f);
                    next += point1;
                    extraVerticesPos2 = position + math.mul(rotation, next);
                    extraVerticesindex2 = i;
                }
                
                if (i + 2 < segments)
                {
                    tempT[currentTriangle++] = vCount;
                    tempT[currentTriangle++] = i + vCount + 2;
                    tempT[currentTriangle++] = i + vCount + 1;
                }
            }

            temp3[segments] = extraVerticesPos;
            temp3[segments + 1] = extraVerticesPos2;
            
            tempT[currentTriangle++] = vCount + extraVerticesindex; 
            tempT[currentTriangle++] = vCount + segments; 
            tempT[currentTriangle++] = vCount + extraVerticesindex - 1;
            
            tempT[currentTriangle++] = vCount; 
            tempT[currentTriangle++] = vCount + segments + 1; 
            tempT[currentTriangle++] = vCount + extraVerticesindex2; 

            SolidBuffer.Submit(temp3, segments + 2, _currentStyleId);
            SolidBuffer.Submit(tempT, currentTriangle);
        }
        
        private void AddSolidBox(DrawCommandBuffer.BoxData boxData)
        {
            var vCount = SolidBuffer.VerticesLength;
            
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
            
            SolidBuffer.Submit(point1, _currentStyleId);
            SolidBuffer.Submit(point2, _currentStyleId);
            SolidBuffer.Submit(point3, _currentStyleId);
            SolidBuffer.Submit(point4, _currentStyleId);
            SolidBuffer.Submit(point5, _currentStyleId);
            SolidBuffer.Submit(point6, _currentStyleId);
            SolidBuffer.Submit(point7, _currentStyleId);
            SolidBuffer.Submit(point8, _currentStyleId);
            
            //   6---------7
            // 2---------3 |
            // | |       | |
            // | 5_______|_8
            // 1_________4

            //Front Face
            
            tempT[0] = 0 + vCount;
            tempT[1] = 1 + vCount;
            tempT[2] = 2 + vCount;
            tempT[3] = 0 + vCount;
            tempT[4] = 2 + vCount;
            tempT[5] = 3 + vCount;
            
            //Back Face
            tempT[6] = 7 + vCount;
            tempT[7] = 6 + vCount;
            tempT[8] = 4 + vCount;
            tempT[9] = 4 + vCount;
            tempT[10] = 6 + vCount;
            tempT[11] = 5 + vCount;
            
            //Right Face
            tempT[12] = 3 + vCount;
            tempT[13] = 2 + vCount;
            tempT[14] = 6 + vCount;
            tempT[15] = 3 + vCount;
            tempT[16] = 6 + vCount;
            tempT[17] = 7 + vCount;
                
            //Left Face
            tempT[18] = 4 + vCount;
            tempT[19] = 5 + vCount;
            tempT[20] = 1 + vCount;
            tempT[21] = 4 + vCount;
            tempT[22] = 1 + vCount;
            tempT[23] = 0 + vCount;
                
            //Top Face
            tempT[24] = 1 + vCount;
            tempT[25] = 5 + vCount;
            tempT[26] = 6 + vCount;
            tempT[27] = 1 + vCount;
            tempT[28] = 6 + vCount;
            tempT[29] = 2 + vCount;
                
            //Bottom Face
            tempT[30] = 0 + vCount;
            tempT[31] = 7 + vCount;
            tempT[32] = 4 + vCount;
            tempT[33] = 0 + vCount;
            tempT[34] = 3 + vCount;
            tempT[35] = 7 + vCount;

            SolidBuffer.Submit(tempT, 36);
        }

        private int2 SolidDomeShape(int2 tempOffset, float3 position, float radius, quaternion rotation)
        {
            const int latitudeSegments = 16 / 2;
            const int longitudeSegments = 16;

            var vCount = SolidBuffer.VerticesLength + tempOffset.x;
            var currentVertex = 0;
            var currentTriangle = 0;

            for (var lat = 0; lat <= latitudeSegments; lat++)
            {
                var normalizedLat = lat / (float)latitudeSegments;
                var theta = normalizedLat * math.PI * 0.5f;

                for (var lon = 0; lon <= longitudeSegments; lon++)
                {
                    var normalizedLon = lon / (float)longitudeSegments;
                    var phi = normalizedLon * 2 * math.PI;

                    var x = math.sin(theta) * math.cos(phi);
                    var y = math.cos(theta);
                    var z = math.sin(theta) * math.sin(phi);

                    var current = new float3(x, y, z) * radius;

                    temp3[tempOffset.x + currentVertex] = position + math.mul(rotation, current);

                    if (lat < latitudeSegments && lon < longitudeSegments)
                    {
                        var currentRow = lat * (longitudeSegments + 1);
                        var nextRow = (lat + 1) * (longitudeSegments + 1);

                        tempT[tempOffset.y + currentTriangle++] = vCount + currentRow + lon;
                        tempT[tempOffset.y + currentTriangle++] = vCount + currentRow + lon + 1;
                        tempT[tempOffset.y + currentTriangle++] = vCount + nextRow + lon + 1;

                        tempT[tempOffset.y + currentTriangle++] = vCount + currentRow + lon;
                        tempT[tempOffset.y + currentTriangle++] = vCount + nextRow + lon + 1;
                        tempT[tempOffset.y + currentTriangle++] = vCount + nextRow + lon;
                    }

                    currentVertex++;
                }
            }

            var totalVertices = (latitudeSegments + 1) * (longitudeSegments + 1);
            return new int2(totalVertices, currentTriangle);
        }
        
        private void AddSolidSphere(DrawCommandBuffer.SphereData sphereData)
        {
            const int latitudeSegments = 16;
            const int longitudeSegments = 16;
            
            var position = sphereData.position;
            var radius = sphereData.radius;

            var vCount = SolidBuffer.VerticesLength;
            var currentVertex = 0;
            var currentTriangle = 0;

            for (var lat = 0; lat <= latitudeSegments; lat++)
            {
                var normalizedLat = lat / (float)latitudeSegments;
                var theta = normalizedLat * math.PI;

                for (var lon = 0; lon <= longitudeSegments; lon++)
                {
                    var normalizedLon = lon / (float)longitudeSegments;
                    var phi = normalizedLon * 2 * math.PI;

                    var x = math.sin(theta) * math.cos(phi);
                    var y = math.sin(theta) * math.sin(phi);
                    var z = math.cos(theta);

                    var current = new float3(x, y, z) * radius;

                    temp3[currentVertex] = position +  current;

                    if (lat < latitudeSegments && lon < longitudeSegments)
                    {
                        var currentRow = lat * (longitudeSegments + 1);
                        var nextRow = (lat + 1) * (longitudeSegments + 1);

                        tempT[currentTriangle++] = vCount + currentRow + lon;
                        tempT[currentTriangle++] = vCount + nextRow + lon + 1;
                        tempT[currentTriangle++] = vCount + currentRow + lon + 1;

                        tempT[currentTriangle++] = vCount + currentRow + lon;
                        tempT[currentTriangle++] = vCount + nextRow + lon;
                        tempT[currentTriangle++] = vCount + nextRow + lon + 1;
                    }

                    currentVertex++;
                }
            }

            var totalVertices = (latitudeSegments + 1) * (longitudeSegments + 1);
            
            SolidBuffer.Submit(temp3, totalVertices, _currentStyleId);
            SolidBuffer.Submit(tempT, currentTriangle);
        }

        private int2 SolidCylinderShape(int2 tempOffset, float3 position, float radius, float height, quaternion rotation)
        {
            const int segments = 16;

            height /= 2;
            
            var vCount = SolidBuffer.VerticesLength + tempOffset.x;
            var currentVertex = 0;
            var currentTriangle = 0;

            var angleStep = 360f / segments;

            // Draw the sides
            for (var i = 0; i < segments; i++)
            {
                var angle = math.radians(angleStep) * i;
                var x = math.cos(angle) * radius;
                var z = math.sin(angle) * radius;

                var currentTop = new float3(x, height, z);
                var currentBottom = new float3(x, -height, z);

                temp3[tempOffset.x + currentVertex] = position + math.mul(rotation, currentTop);
                temp3[tempOffset.x + currentVertex + 1] = position + math.mul(rotation, currentBottom);

                currentVertex += 2;

                var nextIndex = (i + 1) % segments;

                var topIndex1 = i * 2;
                var topIndex2 = nextIndex * 2;
                var bottomIndex1 = i * 2 + 1;
                var bottomIndex2 = nextIndex * 2 + 1;

                // Draw the first triangle
                tempT[tempOffset.y + currentTriangle++] = vCount + topIndex1;
                tempT[tempOffset.y + currentTriangle++] = vCount + topIndex2;
                tempT[tempOffset.y + currentTriangle++] = vCount + bottomIndex1;

                // Draw the second triangle
                tempT[tempOffset.y + currentTriangle++] = vCount + topIndex2;
                tempT[tempOffset.y + currentTriangle++] = vCount + bottomIndex2;
                tempT[tempOffset.y + currentTriangle++] = vCount + bottomIndex1;
            }

            var totalVertices = segments * 2; // Two vertices per segment
            var totalTriangles = segments * 2 * 3; // Two triangles per segment, each with 3 vertices

            return new int2(totalVertices, totalTriangles);
            // SolidBuffer.Submit(temp3, totalVertices, _currentStyleId);
            // SolidBuffer.Submit(tempT, totalTriangles);
        }
        
        private void AddSolidCylinder(DrawCommandBuffer.CylinderData cylinderData)
        {
            var count = SolidCylinderShape(new int2(0, 0), cylinderData.position, cylinderData.radius, cylinderData.height, cylinderData.rotation);
            count += SolidCircleShape(count, cylinderData.position + new float3(0, cylinderData.height / 2, 0), cylinderData.radius, math.mul(cylinderData.rotation, quaternion.RotateX(math.radians(90))));
            count += SolidCircleShape(count, cylinderData.position - new float3(0, cylinderData.height / 2, 0), cylinderData.radius, math.mul(cylinderData.rotation, quaternion.RotateX(math.radians(-90))));
            
            SolidBuffer.Submit(temp3, count.x, _currentStyleId);
            SolidBuffer.Submit(tempT, count.y);
        }

        private void AddSolidCapsule3D(DrawCommandBuffer.CylinderData capsuleData)
        {
            var position = capsuleData.position;
            var radius = capsuleData.radius;
            var height = capsuleData.height;
            var rotation = capsuleData.rotation;
            
            var size = new float2(radius * 2, height);
            
            size /= 2;
            var count = new int2();
            if (height > radius * 2)
            {
                count += SolidCylinderShape(count, position, size.x, (size.y - size.x) * 2, rotation);
                count += SolidDomeShape(count, position + math.mul(rotation, new float3(0, size.y - size.x, 0)), radius, rotation);
                count += SolidDomeShape(count, position + math.mul(rotation, new float3(0, -size.y + size.x, 0)), radius, math.mul(rotation, quaternion.RotateX(math.radians(180))));
            }
            else
            {
                count += SolidDomeShape(count, position, radius, rotation);
                count += SolidDomeShape(count, position, radius, math.mul(rotation, quaternion.RotateX(math.radians(180))));
            }
            
            SolidBuffer.Submit(temp3, count.x, _currentStyleId);
            SolidBuffer.Submit(tempT, count.y);
        }
    }
}