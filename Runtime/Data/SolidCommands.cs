using Unity.Collections;
using Unity.Mathematics;

namespace Drawbug
{
    internal unsafe partial struct ProcessCommandsJob
    {
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

            point1 = math.mul(rotation, point1 - position);
            point1 += position;

            point2 = math.mul(rotation, point2 - position);
            point2 += position;

            point3 = math.mul(rotation, point3 - position);
            point3 += position;

            point4 = math.mul(rotation, point4 - position);
            point4 += position;

            point5 = math.mul(rotation, point5 - position);
            point5 += position;

            point6 = math.mul(rotation, point6 - position);
            point6 += position;

            point7 = math.mul(rotation, point7 - position);
            point7 += position;

            point8 = math.mul(rotation, point8 - position);
            point8 += position;
            
            SolidBuffer.Submit(point1, _currentStyleId);
            SolidBuffer.Submit(point2, _currentStyleId);
            SolidBuffer.Submit(point3, _currentStyleId);
            SolidBuffer.Submit(point4, _currentStyleId);
            SolidBuffer.Submit(point5, _currentStyleId);
            SolidBuffer.Submit(point6, _currentStyleId);
            SolidBuffer.Submit(point7, _currentStyleId);
            SolidBuffer.Submit(point8, _currentStyleId);
            
            //   5---------6
            // 1---------2 |
            // | |       | |
            // | 4_______|_7
            // 0_________3

            //Front Face
            var triangles = new NativeArray<int>(36, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            
            triangles[0] = 0 + vCount;
            triangles[1] = 1 + vCount;
            triangles[2] = 2 + vCount;
            triangles[3] = 0 + vCount;
            triangles[4] = 2 + vCount;
            triangles[5] = 3 + vCount;
            
            //Back Face
            triangles[6] = 7 + vCount;
            triangles[7] = 6 + vCount;
            triangles[8] = 4 + vCount;
            triangles[9] = 4 + vCount;
            triangles[10] = 6 + vCount;
            triangles[11] = 5 + vCount;
            
            //Right Face
            triangles[12] = 3 + vCount;
            triangles[13] = 2 + vCount;
            triangles[14] = 6 + vCount;
            triangles[15] = 3 + vCount;
            triangles[16] = 6 + vCount;
            triangles[17] = 7 + vCount;
                
            //Left Face
            triangles[18] = 4 + vCount;
            triangles[19] = 5 + vCount;
            triangles[20] = 1 + vCount;
            triangles[21] = 4 + vCount;
            triangles[22] = 1 + vCount;
            triangles[23] = 0 + vCount;
                
            //Top Face
            triangles[24] = 1 + vCount;
            triangles[25] = 5 + vCount;
            triangles[26] = 6 + vCount;
            triangles[27] = 1 + vCount;
            triangles[28] = 6 + vCount;
            triangles[29] = 2 + vCount;
                
            //Bottom Face
            triangles[30] = 0 + vCount;
            triangles[31] = 7 + vCount;
            triangles[32] = 4 + vCount;
            triangles[33] = 0 + vCount;
            triangles[34] = 3 + vCount;
            triangles[35] = 7 + vCount;

            SolidBuffer.Submit(triangles, 36);
            triangles.Dispose();
        }
    }
}