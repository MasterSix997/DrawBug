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
        public SolidBuffer SolidBuffer;
        public NativeList<DrawCommandBuffer.StyleData> StyleData;

        private bool _firstStyle;
        private uint _currentStyleId;
        
        private DrawMode _currentDrawMode;

        private void AddStyle(DrawCommandBuffer.StyleData styleData)
        {
            if (!_firstStyle)
                _currentStyleId++;
            _firstStyle = false;
            
            StyleData.Add(styleData);
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Next(ref UnsafeAppendBuffer.Reader reader)
        {
            var command = reader.ReadNext<DrawCommandBuffer.Command>();
            switch (command)
            {
                case DrawCommandBuffer.Command.Style:
                    AddStyle(reader.ReadNext<DrawCommandBuffer.StyleData>());
                    break;
                case DrawCommandBuffer.Command.DrawMode:
                    _currentDrawMode = reader.ReadNext<DrawMode>();
                    break;
                case DrawCommandBuffer.Command.Line:
                    AddLine(reader.ReadNext<DrawCommandBuffer.LineData>());
                    break;
                case DrawCommandBuffer.Command.Lines:
                    var arrayLength = reader.ReadNext<int>();
                    var sizeOfArray = UnsafeUtility.SizeOf<float3>() * arrayLength;
                    
                    var linesPtr = (float3*)(reader.Ptr + reader.Offset);
                    AddLines(linesPtr, arrayLength);
                    reader.Offset += sizeOfArray;
                    break;
                case DrawCommandBuffer.Command.Box:
                    var boxData = reader.ReadNext<DrawCommandBuffer.BoxData>();
                    if (_currentDrawMode is DrawMode.Wire or DrawMode.Both)
                        AddBox(boxData);
                    if (_currentDrawMode is DrawMode.Solid or DrawMode.Both)
                        AddSolidBox(boxData);
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
            SolidBuffer.Clear();
            StyleData.Clear();
            _firstStyle = true;
            _currentDrawMode = DrawMode.Wire;
            
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