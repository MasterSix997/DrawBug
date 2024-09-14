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

            // Front
            data[i++] = math.mul(boxData.rotation, new float3(-.5f, -.5f, .5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(-.5f, .5f, .5f) * boxData.size + boxData.position);

            data[i++] = math.mul(boxData.rotation, new float3(.5f, -.5f, .5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(.5f, .5f, .5f) * boxData.size + boxData.position);

            data[i++] = math.mul(boxData.rotation, new float3(-.5f, .5f, .5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(.5f, .5f, .5f) * boxData.size + boxData.position);

            data[i++] = math.mul(boxData.rotation, new float3(-.5f, -.5f, .5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(.5f, -.5f, .5f) * boxData.size + boxData.position);

            // Back
            data[i++] = math.mul(boxData.rotation, new float3(-.5f, -.5f, -.5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(-.5f, .5f, -.5f) * boxData.size + boxData.position);

            data[i++] = math.mul(boxData.rotation, new float3(.5f, -.5f, -.5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(.5f, .5f, -.5f) * boxData.size + boxData.position);

            data[i++] = math.mul(boxData.rotation, new float3(-.5f, .5f, -.5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(.5f, .5f, -.5f) * boxData.size + boxData.position);

            data[i++] = math.mul(boxData.rotation, new float3(-.5f, -.5f, -.5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(.5f, -.5f, -.5f) * boxData.size + boxData.position);

            // Side Connections
            data[i++] = math.mul(boxData.rotation, new float3(-.5f, -.5f, -.5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(-.5f, -.5f, .5f) * boxData.size + boxData.position);

            data[i++] = math.mul(boxData.rotation, new float3(-.5f, .5f, -.5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(-.5f, .5f, .5f) * boxData.size + boxData.position);

            data[i++] = math.mul(boxData.rotation, new float3(.5f, -.5f, -.5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(.5f, -.5f, .5f) * boxData.size + boxData.position);

            data[i++] = math.mul(boxData.rotation, new float3(.5f, .5f, -.5f) * boxData.size + boxData.position);
            data[i++] = math.mul(boxData.rotation, new float3(.5f, .5f, .5f) * boxData.size + boxData.position);

            WireBuffer.Submit(data, 24, _currentStyleId);
            data.Dispose();
        }
    }
}