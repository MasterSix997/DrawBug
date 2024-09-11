using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug
{
    [BurstCompile]
    internal struct CommandBuffer : IDisposable
    {
        private UnsafeAppendBuffer _buffer;

        internal bool HasData => _buffer.Length > 0;
        
        internal CommandBuffer(int initialSize)
        {
            _buffer = new UnsafeAppendBuffer(initialSize, 4, Allocator.Persistent);
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
        }
        
        internal enum Command
        {
            Line,
            Cube
        }
        
        internal struct LineData
        {
            public float3 a, b;
        }
        
        internal struct LineDataVector3 {
            public Vector3 a, b;
        }
        
        internal struct CubeData
        {
            public float3 position;
            public float3 size;
            public quaternion rotation;
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
                    throw new Exception("CommandBuffer buffer is very large");
                }
#endif
                _buffer.SetCapacity(math.max(newLength, _buffer.Length * 2));
            }
        }
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal void Reserve<T>() where T : struct {
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

        internal void Cube(float3 position, float3 size, quaternion rotation)
        {
            Reserve<CubeData>();
            Add(Command.Cube);
            Add(new CubeData
            {
                position = position,
                size = size,
                rotation = rotation
            });
        }
    }
}