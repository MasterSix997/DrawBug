using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Drawbug
{
    internal struct RenderData : IDisposable
    {
        internal WireBuffer WireBuffer;
        internal SolidBuffer SolidBuffer;
        internal NativeList<DrawCommandBuffer.StyleData> StyleData;
        
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
            SolidBuffer.Dispose();
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
                SolidBuffer = SolidBuffer,
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
    }
}