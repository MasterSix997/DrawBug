using Unity.Collections;
using Unity.Mathematics;

namespace Drawbug
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

        private void AddBox(DrawCommandBuffer.BoxData boxData)
        {
            var data = new NativeArray<float3>(24, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var i = 0;
            
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
            
            // Front
            data[i++] = point1;
            data[i++] = point2;

            data[i++] = point2;
            data[i++] = point3;

            data[i++] = point3;
            data[i++] = point4;

            data[i++] = point4;
            data[i++] = point1;
                
            // Back 
            data[i++] = point5;
            data[i++] = point6;
                
            data[i++] = point6;
            data[i++] = point7;
                
            data[i++] = point7;
            data[i++] = point8;
                
            data[i++] = point8;
            data[i++] = point5;
                
            // Side Connections
            data[i++] = point1;
            data[i++] = point5;
            
            data[i++] = point2;
            data[i++] = point6;
    
            data[i++] = point3;
            data[i++] = point7;

            data[i++] = point4;
            data[i] = point8;

            WireBuffer.Submit(data, 24, _currentStyleId);
            data.Dispose();
        }
    }
}