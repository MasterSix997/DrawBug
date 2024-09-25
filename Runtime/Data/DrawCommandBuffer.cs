// ReSharper disable InconsistentNaming
using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug.PhysicsExtension
{
    public enum DrawMode
    {
        Wire,
        Solid,
        Both
    } 
    
    [StructLayout(LayoutKind.Sequential)]
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

            CurrentDrawMode = PhysicsExtension.DrawMode.Wire;
            CurrentMatrix = float4x4.identity;
            
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
            
            CurrentDrawMode = PhysicsExtension.DrawMode.Wire;
            
            ResetStyle();
        }
        
        internal enum Command
        {
            Style,
            DrawMode,
            Matrix,
            Line,
            Lines,
            Rectangle,
            Circle,
            HollowCircle,
            Capsule,
            Box, 
            Sphere,
            Cylinder,
            Capsule3D,
        }
        
        internal struct LineData
        {
            public float3 a, b;
        }
        
        internal struct LineDataVector3 {
            public Vector3 a, b;
        }
        
        internal struct CircleData
        {
            public float3 position;
            public quaternion rotation;
            public float radius;
        }
        
        internal struct HollowCircleData
        {
            public float3 position;
            public quaternion rotation;
            public float innerRadius;
            public float outerRadius;
        }
        
        internal struct CapsuleData
        {
            public float3 position;
            public float2 size;
            public quaternion rotation;
            public bool isVertical;
        }
        
        internal struct BoxData
        {
            public float3 position;
            public float3 size;
            public quaternion rotation;
        }
        
        internal struct RectangleData
        {
            public float3 position;
            public float2 size;
            public quaternion rotation;
        }
        
        internal struct SphereData
        {
            public float3 position;
            public float radius;
        }
        
        internal struct CylinderData
        {
            public float3 position;
            public float radius;
            public float height;
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
        
        internal float4x4 CurrentMatrix;
        
        internal void Matrix(float4x4 matrix)
        {
            Reserve<float4x4>();
            Add(Command.Matrix);
            Add(matrix);

            CurrentMatrix = matrix;
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
        
        [BurstDiscard]
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

        internal void Point(float3 position, float size)
        {
            Line(position + new float3(-size, 0, 0), position + new float3(size, 0, 0));
            Line(position + new float3(0, -size, 0), position + new float3(0, size, 0));
            Line(position + new float3(0, 0, -size), position + new float3(0, 0, size));
        }
        
        internal void Point(float2 position, float size)
        {
            var position3 = new float3(position.x, position.y, 0);
            Line(position3 + new float3(-size, 0, 0), position3 + new float3(size, 0, 0));
            Line(position3 + new float3(0, -size, 0), position3 + new float3(0, size, 0));
        }
        
        internal void Rectangle(float3 position, float2 size, quaternion rotation)
        {
            Reserve<RectangleData>();
            Add(Command.Rectangle);
            Add(new RectangleData
            {
                position = position,
                size = size,
                rotation = rotation
            });
        }
        
        internal void Rectangle(float3 point1, float3 point2, quaternion rotation)
        {
            var center = new float3((point1 + point2) / 2);
            var size = math.abs(point2 - point1).xy;

            Reserve<RectangleData>();
            Add(Command.Rectangle);
            Add(new RectangleData
            {
                position = center,
                size = size,
                rotation = rotation
            });
        }
        
        internal void Circle(float3 position, float radius, quaternion rotation)
        {
            Reserve<CircleData>();
            Add(Command.Circle);
            Add(new CircleData
            {
                position = position,
                radius = radius,
                rotation = rotation
            });
        }
        
        internal void HollowCircle(float3 position, float innerRadius, float outerRadius, quaternion rotation)
        {
            Reserve<HollowCircleData>();
            Add(Command.HollowCircle);
            Add(new HollowCircleData
            {
                position = position,
                innerRadius = innerRadius,
                outerRadius = outerRadius,
                rotation = rotation
            });
        }
        
        internal void Capsule(float3 position, float2 size, quaternion rotation, bool isVertical)
        {
            Reserve<CapsuleData>();
            Add(Command.Capsule);
            Add(new CapsuleData
            {
                position = position,
                size = size,
                rotation = rotation,
                isVertical = isVertical
            });
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
        
        internal void Sphere(float3 position, float radius)
        {
            Reserve<SphereData>();
            Add(Command.Sphere);
            Add(new SphereData
            {
                position = position,
                radius = radius,
            });
        }
        
        internal void Cylinder(float3 position, float radius, float height, quaternion rotation)
        {
            Reserve<CylinderData>();
            Add(Command.Cylinder);
            Add(new CylinderData
            {
                position = position,
                radius = radius,
                height = height,
                rotation = rotation
            });
        }
        
        internal void Capsule3D(float3 position, float radius, float height, quaternion rotation)
        {
            Reserve<CylinderData>();
            Add(Command.Capsule3D);
            Add(new CylinderData
            {
                position = position,
                radius = radius,
                height = height,
                rotation = rotation
            });
        }

        internal void AnotherBuffer(DrawCommandBuffer commandBuffer)
        {
            var bufferLength = commandBuffer._buffer.Length;
            Reserve(bufferLength);
            unsafe
            {
                void* destinationPtr = _buffer.Ptr + _buffer.Length;
                void* sourcePtr = commandBuffer._buffer.Ptr;
                UnsafeUtility.MemCpy(destinationPtr, sourcePtr, bufferLength);
                _buffer.Length += bufferLength;
            }
        }
        
        internal unsafe void AnotherBuffer(DrawCommandBuffer* commandBuffer)
        {
            var bufferLength = commandBuffer->_buffer.Length;
            Reserve(bufferLength);
            void* destinationPtr = _buffer.Ptr + _buffer.Length;
            void* sourcePtr = commandBuffer->_buffer.Ptr;
            UnsafeUtility.MemCpy(destinationPtr, sourcePtr, bufferLength);
            _buffer.Length += bufferLength;
        }
    }
}