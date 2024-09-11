using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug
{
    internal struct RenderData : IDisposable
    {
        internal WireBuffer WireBuffer;
        internal NativeList<CommandBuffer.StyleData> StyleData;
        
        private JobHandle _submitJob;
        private bool _hasJob;

        public void Dispose()
        {
            if (_hasJob)
            {
                _hasJob = false;
                _submitJob.Complete();
            }
            
            WireBuffer.Dispose();
            StyleData.Dispose();
        }

        public unsafe void ProcessCommands(UnsafeAppendBuffer* buffer)
        {
            if (_hasJob)
                throw new Exception("A job is already happening");
            
            _hasJob = true;
            
            _submitJob = new ProcessCommandsJob
            {
                Buffer = buffer,
                WireBuffer = WireBuffer,
                StyleData = StyleData,
            }.Schedule();
        }

        public void GetCommandResults()
        {
            if (!_hasJob)
                throw new Exception("No jobs are running");

            _hasJob = false;
            _submitJob.Complete();
        }
        
        private unsafe struct ProcessCommandsJob : IJob
        {
            //Input
            [NativeDisableUnsafePtrRestriction] public UnsafeAppendBuffer* Buffer;
            
            //Output
            public WireBuffer WireBuffer;
            public NativeList<CommandBuffer.StyleData> StyleData;

            private bool _firstStyle;
            private uint _currentStyleId;

            private void AddStyle(CommandBuffer.StyleData styleData)
            {
                if (!_firstStyle)
                    _currentStyleId++;
                _firstStyle = false;
                
                StyleData.Add(styleData);
            }

            private void AddLine(CommandBuffer.LineData lineData)
            {
                WireBuffer.Submit(lineData.a, lineData.b, _currentStyleId);
            }

            private void AddCube(CommandBuffer.CubeData cubeData)
            {
                // Crie o NativeArray de 24 vértices
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

                // Enviar os dados ao WireBuffer (verifique se WireBuffer está corretamente definido)
                WireBuffer.Submit(data, 24, _currentStyleId);

                // Libere o NativeArray
                data.Dispose();
            }

            
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            private void Next(ref UnsafeAppendBuffer.Reader reader)
            {
                var command = reader.ReadNext<CommandBuffer.Command>();
                switch (command)
                {
                    case CommandBuffer.Command.Style:
                        var data = reader.ReadNext<CommandBuffer.StyleData>();
                        AddStyle(data);
                        break;
                    case CommandBuffer.Command.Line:
                        AddLine(reader.ReadNext<CommandBuffer.LineData>());
                        break;
                    case CommandBuffer.Command.Cube:
                        AddCube(reader.ReadNext<CommandBuffer.CubeData>());
                        break;
                    default:
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                        throw new ArgumentOutOfRangeException(command.ToString(), "Unknown command");
#else
				        break;
#endif
                }
            }
            
            public void Execute()
            {
                WireBuffer.Clear();
                StyleData.Clear();
                _firstStyle = true;
                
                var reader = Buffer->AsReader();
                while (reader.Offset < reader.Size)
                {
                    Next(ref reader);
                }

                if (reader.Offset != reader.Size)
                {
                    throw new Exception("Didn't reach the end of the buffer");
                }
            }
        }
    }
}