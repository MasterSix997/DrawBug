using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Drawbug
{
    internal struct RenderData : IDisposable
    {
        internal WireBuffer WireBuffer;
        
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
        }

        public unsafe void ProcessCommands(UnsafeAppendBuffer* buffer)
        {
            if (_hasJob)
                throw new Exception("A job is already happening");
            
            _hasJob = true;

            //fixed(UnsafeAppendBuffer* bufferPtr = &buffer)
            fixed(WireBuffer* wireBufferPtr = &WireBuffer)
            {
                _submitJob = new ProcessCommandsJob()
                {
                    Buffer = buffer,
                    WireBuffer = wireBufferPtr
                }.Schedule();
            }
        }

        public void GetCommandResults()
        {
            if (!_hasJob)
                throw new Exception("No jobs are running");

            _hasJob = false;
            
            _submitJob.Complete();

            Debug.Log(WireBuffer.Length);
        }
        
        private unsafe struct ProcessCommandsJob : IJob
        {
            //Input
            [NativeDisableUnsafePtrRestriction] public UnsafeAppendBuffer* Buffer;
            
            //Output
            [NativeDisableUnsafePtrRestriction] public WireBuffer* WireBuffer;

            private void AddLine(CommandBuffer.LineData lineData)
            {
                WireBuffer->Submit(lineData.a, lineData.b, 0);
            }
            
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            private void Next(ref UnsafeAppendBuffer.Reader reader)
            {
                var command = reader.ReadNext<CommandBuffer.Command>();
                switch (command)
                {
                    case CommandBuffer.Command.Line:
                        AddLine(reader.ReadNext<CommandBuffer.LineData>());
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
                WireBuffer->Clear();
                
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