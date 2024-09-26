using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Drawbug
{
    [BurstCompile]
    internal struct RenderData : IDisposable
    {
        internal WireBuffer WireBuffer;
        internal SolidBuffer SolidBuffer;
        internal NativeList<DrawCommandBuffer.StyleData> StyleData;
        internal NativeArray<float3> TempFloat3Buffer;
        internal NativeArray<int> TempTriangleBuffer;
        
        private JobHandle _submitJob;
        private bool _hasJob;
        
        private unsafe float3* _tempFloat3Ptr;
        private unsafe int* _tempTrianglePtr;

        internal unsafe void CreatePtr()
        {
            _tempFloat3Ptr = (float3*)TempFloat3Buffer.GetUnsafePtr();
            _tempTrianglePtr = (int*)TempTriangleBuffer.GetUnsafePtr();
        }

        public void Dispose()
        {
            if (_hasJob)
            {
                _hasJob = false;
                _submitJob.Complete();
            }
            
            WireBuffer.Dispose();
            SolidBuffer.Dispose();
            StyleData.Dispose();
            TempFloat3Buffer.Dispose();
            TempTriangleBuffer.Dispose();
            unsafe
            {
                _tempFloat3Ptr = null;
                _tempTrianglePtr = null;
            }
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
                SolidBuffer = SolidBuffer,
                StyleData = StyleData,
                temp3 = _tempFloat3Ptr,
                tempT = _tempTrianglePtr
            }.Schedule();
        }

        public void GetCommandResults()
        {
            if (!_hasJob)
                throw new Exception("No jobs are running");

            _hasJob = false;
            _submitJob.Complete();
        }
    }
}