using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Drawbug
{
    internal unsafe partial struct ProcessCommandsJob : IJob
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


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Next(ref UnsafeAppendBuffer.Reader reader)
        {
            var command = reader.ReadNext<CommandBuffer.Command>();
            switch (command)
            {
                case CommandBuffer.Command.Style:
                    AddStyle(reader.ReadNext<CommandBuffer.StyleData>());
                    break;
                case CommandBuffer.Command.Line:
                    AddLine(reader.ReadNext<CommandBuffer.LineData>());
                    break;
                case CommandBuffer.Command.Lines:
                    int arrayLength = reader.ReadNext<int>();
                    // var linesArray = new NativeArray<float3>(arrayLength, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                    int sizeOfArray = UnsafeUtility.SizeOf<float3>() * arrayLength;

                    // void* sourcePtr = reader.Ptr + reader.Offset;
                    // void* destinationPtr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(linesArray);
                    // UnsafeUtility.MemCpy(destinationPtr, sourcePtr, sizeOfArray);
                    
                    var linesPtr = (float3*)(reader.Ptr + reader.Offset);
                    AddLines(linesPtr, arrayLength);
                    reader.Offset += sizeOfArray;
                    
                    
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