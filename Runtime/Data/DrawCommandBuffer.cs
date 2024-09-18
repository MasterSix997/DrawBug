using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Drawbug
{
    public enum DrawMode
    {
        Wire,
        Solid,
        Both
    } 
    
    [BurstCompile]
    internal struct DrawCommandBuffer : IDisposable
    {
        private UnsafeAppendBuffer _buffer;

        internal bool HasData => _buffer.Length > 0;
        
        internal DrawCommandBuffer(int initialSize)
        {
            _buffer = new UnsafeAppendBuffer(initialSize, 4, Allocator.Persistent);
            
            _hasPendingStyle = default;
            PendingStyle = default;

            CurrentDrawMode = Drawbug.DrawMode.Wire;
            
            ResetStyle();
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }

        internal unsafe UnsafeAppendBuffer* GetBuffer()
        {
            fixed(UnsafeAppendBuffer* bufferPtr = &_buffer)
                return bufferPtr;
        }

        internal void Clear()
        {
            _buffer.Reset();
            
            CurrentDrawMode = Drawbug.DrawMode.Wire;
            
            ResetStyle();
        }
        
        internal enum Command
        {
            Style,
            DrawMode,
            Line,
            Lines,
            Box
        }
        
        internal struct LineData
        {
            public float3 a, b;
        }
        
        internal struct LineDataVector3 {
            public Vector3 a, b;
        }
        
        internal struct BoxData
        {
            public float3 position;
            public float3 size;
            public quaternion rotation;
        }
        
        internal struct StyleData
        {
            public Color color;
            public bool forward;
        }
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void Reserve (int additionalSpace) 
        {
            var newLength = _buffer.Length + additionalSpace;
            if (newLength > _buffer.Capacity) 
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                const int MAX_BUFFER_SIZE = 1024 * 1024 * 256; // 256 MB
                if (_buffer.Length * 2 > MAX_BUFFER_SIZE) {
                    throw new Exception("DrawCommandBuffer buffer is very large");
                }
#endif
                _buffer.SetCapacity(math.max(newLength, _buffer.Length * 2));
            }
        }
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal void Reserve<T>() where T : struct {
            ApplyPendingStyle();
            Reserve(UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<T>());
        }
        
        internal unsafe void Add<T>(T value) where T : struct {
            var valueSize = UnsafeUtility.SizeOf<T>();
            var bufferSize = _buffer.Length;
            // We assume this because the Reserve function has already taken care of that.
            // This removes a few branches from the assembly when running in burst.
            Unity.Burst.CompilerServices.Hint.Assume(_buffer.Ptr != null);
            Unity.Burst.CompilerServices.Hint.Assume(_buffer.Ptr + bufferSize != null);

            UnsafeUtility.CopyStructureToPtr(ref value, _buffer.Ptr + bufferSize);
            _buffer.Length = bufferSize + valueSize;
        }
        
        //================= Style =================

        private bool _hasPendingStyle;
        internal StyleData PendingStyle;

        internal void StyleColor(Color color)
        {
            _hasPendingStyle = true;
            PendingStyle.color = color;
        }

        internal void StyleForward(bool forward)
        {
            _hasPendingStyle = true;
            PendingStyle.forward = forward;
        }

        private void ApplyPendingStyle()
        {
            if (!_hasPendingStyle)
                return;
            
            Reserve(UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<StyleData>());
            Add(Command.Style);
            Add(PendingStyle);

            _hasPendingStyle = false;
        }

        internal void ResetStyle()
        {
            _hasPendingStyle = true;
            PendingStyle = new StyleData
            {
                color = Color.white,
                forward = false
            };
        }
        
        //=========================================

        internal DrawMode CurrentDrawMode;
        
        internal void DrawMode(DrawMode drawMode)
        {
            Reserve<DrawMode>();
            Add(Command.DrawMode);
            Add(drawMode);
            
            CurrentDrawMode = drawMode;
        }
        
        //=========================================

        internal void Line(float3 a, float3 b)
        {
            Reserve<LineData>();
            Add(Command.Line);
            Add(new LineData {a = a, b = b});
        }
        
        internal void Line(Vector3 a, Vector3 b) {
            Reserve<LineData>();
            var bufferSize = _buffer.Length;

            unsafe {
                var newLen = bufferSize + 4 + 24;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                UnityEngine.Assertions.Assert.IsTrue(newLen <= _buffer.Capacity);
#endif
                var ptr = _buffer.Ptr + bufferSize;
                *(Command*)ptr = Command.Line;
                var lineData = (LineDataVector3*)(ptr + 4);
                lineData->a = a;
                lineData->b = b;
                _buffer.Length = newLen;
            }
        }
        
        internal void Lines(float3[] linesArray)
        {
            var arrayLength = linesArray.Length;
            var sizeOfArray = UnsafeUtility.SizeOf<float3>() * arrayLength;
            if (arrayLength % 2 != 0)
            {
                throw new ArgumentException("Length must be even to submit pairs of points.");
            }
            
            //Reserve for Command, Size of array and array
            ApplyPendingStyle();
            Reserve(UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<int>() + sizeOfArray);
            
            Add(Command.Lines);
            Add(arrayLength);

            unsafe
            {
                void* destinationPtr = _buffer.Ptr + _buffer.Length;
                void* sourcePtr = UnsafeUtility.AddressOf(ref linesArray[0]);
                UnsafeUtility.MemCpy(destinationPtr, sourcePtr, sizeOfArray);
                _buffer.Length += sizeOfArray;
            }
        }
        
        internal void Lines(NativeArray<float3> linesArray)
        {
            var arrayLength = linesArray.Length;
            var sizeOfArray = UnsafeUtility.SizeOf<float3>() * arrayLength;
            if (arrayLength % 2 != 0)
            {
                throw new ArgumentException("Length must be even to submit pairs of points.");
            }
            
            //Reserve for Command, Size of array and array
            ApplyPendingStyle();
            Reserve(UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<int>() + sizeOfArray);
            
            Add(Command.Lines);
            Add(arrayLength);

            unsafe
            {
                void* destinationPtr = _buffer.Ptr + _buffer.Length;
                void* sourcePtr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(linesArray);
                UnsafeUtility.MemCpy(destinationPtr, sourcePtr, sizeOfArray);
                _buffer.Length += sizeOfArray;
            }
        }

        internal void Box(float3 position, float3 size, quaternion rotation)
        {
            Reserve<BoxData>();
            Add(Command.Box);
            Add(new BoxData
            {
                position = position,
                size = size,
                rotation = rotation
            });
        }
    }
}