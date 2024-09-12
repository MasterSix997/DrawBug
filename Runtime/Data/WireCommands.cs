using Unity.Collections;
using Unity.Mathematics;

namespace Drawbug
{
    internal unsafe partial struct ProcessCommandsJob
    {
        private void AddLine(CommandBuffer.LineData lineData)
        {
            WireBuffer.Submit(lineData.a, lineData.b, _currentStyleId);
        }

        private void AddLines(float3* linesPtr, int arrayLength)
        {
            WireBuffer.Submit(linesPtr, arrayLength, _currentStyleId);
        }

        private void AddCube(CommandBuffer.CubeData cubeData)
        {
            var data = new NativeArray<float3>(24, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var i = 0;

            // Front
            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, -.5f, .5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, .5f, .5f) * cubeData.size) + cubeData.position);

            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, -.5f, .5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, .5f, .5f) * cubeData.size) + cubeData.position);

            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, .5f, .5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, .5f, .5f) * cubeData.size) + cubeData.position);

            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, -.5f, .5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, -.5f, .5f) * cubeData.size) + cubeData.position);

            // Back
            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, -.5f, -.5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, .5f, -.5f) * cubeData.size) + cubeData.position);

            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, -.5f, -.5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, .5f, -.5f) * cubeData.size) + cubeData.position);

            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, .5f, -.5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, .5f, -.5f) * cubeData.size) + cubeData.position);

            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, -.5f, -.5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, -.5f, -.5f) * cubeData.size) + cubeData.position);

            // Side Connections
            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, -.5f, -.5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, -.5f, .5f) * cubeData.size) + cubeData.position);

            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, .5f, -.5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(-.5f, .5f, .5f) * cubeData.size) + cubeData.position);

            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, -.5f, -.5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, -.5f, .5f) * cubeData.size) + cubeData.position);

            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, .5f, -.5f) * cubeData.size) + cubeData.position);
            data[i++] = math.mul(cubeData.rotation, (new float3(.5f, .5f, .5f) * cubeData.size) + cubeData.position);

            WireBuffer.Submit(data, 24, _currentStyleId);
            data.Dispose();
        }
    }
}