using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Drawbug
{
    // [NativeContainer]
    // [DebuggerDisplay("Length = {_bufferData == null ? default : Length}, Capacity = {_bufferData == null ? default : Capacity}")]
    // [DebuggerTypeProxy(typeof(WireBufferDebugView))]
    // [GenerateTestsForBurstCompatibility(GenericTypeArguments = new [] { typeof(int) })]
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile]
    public unsafe struct WireBuffer : IDisposable
    {
        [NativeDisableUnsafePtrRestriction]
        private UnsafeWireBuffer* _bufferData;
        
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        private static readonly SharedStatic<int> StaticSafetyId = SharedStatic<int>.GetOrCreate<WireBuffer>();
#endif

        private Allocator _allocatorLabel;

        public WireBuffer(int initialCapacity, Allocator allocator = Allocator.Persistent)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Initial Capacity must be >= 0");
#endif
            _bufferData = (UnsafeWireBuffer*)UnsafeUtility.Malloc(sizeof(UnsafeWireBuffer), UnsafeUtility.AlignOf<UnsafeWireBuffer>(), allocator);
            *_bufferData = new UnsafeWireBuffer(initialCapacity, ref allocator);
            
            _allocatorLabel = allocator;
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = CollectionHelper.CreateSafetyHandle(allocator);
            CollectionHelper.SetStaticSafetyId<WireBuffer>(ref m_Safety, ref StaticSafetyId.Data);
#endif
        }
        
        internal int Length => _bufferData->Length;
        internal int Capacity => _bufferData->Capacity;

        internal bool UseMatrix => _bufferData->UseMatrix;

        internal float4x4 Matrix
        {
            get => _bufferData->Matrix;
            set
            {
                _bufferData->Matrix = value;
                _bufferData->UseMatrix = !value.Equals(float4x4.identity);
            }
        }

        internal void Clear()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Clear();
        }
        
        internal void Submit(float3 point1, float3 point2, uint dataIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // If the container is currently not allowed to write to the buffer
            // then this will throw an exception.
            // This handles all cases, from already disposed containers
            // to safe multithreaded access.
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Submit(point1, point2, dataIndex);
        }
        
        [BurstCompile]
        internal void Submit(NativeArray<float3> points, int length, uint dataIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // If the container is currently not allowed to write to the buffer
            // then this will throw an exception.
            // This handles all cases, from already disposed containers
            // to safe multithreaded access.
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Submit(points, length, dataIndex);
        }
        
        [BurstCompile]
        internal void Submit(float3* points, int length, uint dataIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // If the container is currently not allowed to write to the buffer
            // then this will throw an exception.
            // This handles all cases, from already disposed containers
            // to safe multithreaded access.
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Submit(points, length, dataIndex);
        }

        [BurstDiscard]
        internal void FillBuffer(GraphicsBuffer buffer)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // If the container is currently not allowed to read from the buffer
            // then this will throw an exception.
            // This handles all cases, from already disposed containers
            // to safe multithreaded access.
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif

            _bufferData->FillBuffer(buffer);
        }
        
        internal PositionData[] ToArray()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif

            return _bufferData->ToArray();
        }

        public bool IsCreated => _bufferData != null && _bufferData->IsCreated;

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            CollectionHelper.DisposeSafetyHandle(ref m_Safety);
#endif
            if (_bufferData == null) 
                return;
            
            _bufferData->Dispose();
            UnsafeUtility.Free(_bufferData, _allocatorLabel);
            _bufferData = null;
        }
    }
    
    internal sealed class WireBufferDebugView
    {
        private WireBuffer _buffer;

        public WireBufferDebugView(WireBuffer buffer)
        {
            _buffer = buffer;
        }

        public PositionData[] Items => _buffer.ToArray();
    }
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct PositionData
    {
        public float3 Position;
        public uint DataIndex;
    }

    [DebuggerDisplay("Length = {_bufferData == null ? default : Length}, Capacity = {_bufferData == null ? default : Capacity}")]
    [DebuggerTypeProxy(typeof(WireBufferDebugView))]
    [GenerateTestsForBurstCompatibility(GenericTypeArguments = new [] { typeof(int) })]
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile]
    internal unsafe struct UnsafeWireBuffer : IDisposable
    {
        [NativeDisableUnsafePtrRestriction]
        private PositionData* _bufferData;
        private int _currentLength;
        private int _bufferCapacity;

        private Allocator _allocatorLabel;

        public UnsafeWireBuffer(int initialCapacity, ref Allocator allocator)
        {
            _bufferData = (PositionData*)UnsafeUtility.Malloc(sizeof(PositionData) * initialCapacity, UnsafeUtility.AlignOf<PositionData>(), allocator);
            
            _currentLength = 0;
            _bufferCapacity = initialCapacity;
            _allocatorLabel = allocator;
            UseMatrix = false;
            Matrix = float4x4.identity;
        }
        
        internal int Length => _currentLength;
        internal int Capacity => _bufferCapacity;
        
        internal bool UseMatrix;
        internal float4x4 Matrix;

        internal void Clear()
        {
            _currentLength = 0;
            UseMatrix = false;
            Matrix = float4x4.identity;
        }
        
        private void EnsureCapacity(int additionalCapacity)
        {
            if (_currentLength + additionalCapacity > _bufferCapacity)
            {
                var newCapacity = Math.Max(_bufferCapacity * 2, _currentLength + additionalCapacity);
        
                var newBufferPtr = (PositionData*)UnsafeUtility.Malloc(newCapacity * sizeof(PositionData), UnsafeUtility.AlignOf<PositionData>(), _allocatorLabel);
                UnsafeUtility.MemCpy(newBufferPtr, _bufferData, _currentLength * sizeof(PositionData));
                UnsafeUtility.FreeTracked(_bufferData, _allocatorLabel);

                _bufferData = newBufferPtr;
                _bufferCapacity = newCapacity;
            }
        }
        
        [BurstCompile]
        internal void Submit(float3 point1, float3 point2, uint dataIndex)
        {
            EnsureCapacity(2);
            _bufferData[_currentLength].DataIndex = dataIndex;
            _bufferData[_currentLength++].Position = UseMatrix ? math.mul(Matrix, new float4(point1, 1)).xyz : point1;
            _bufferData[_currentLength].DataIndex = dataIndex;
            _bufferData[_currentLength++].Position = UseMatrix ? math.mul(Matrix, new float4(point2, 1)).xyz : point2;
        }
        
        [BurstCompile]
        internal void Submit(NativeArray<float3> points, int length, uint dataIndex)
        {
            if (length % 2 != 0)
            {
                throw new ArgumentException("Length must be even to submit pairs of points.");
            }
            EnsureCapacity(length);
            
            var pairsCount = length / 2;
            var pointsPtr = (float3*)points.GetUnsafeReadOnlyPtr();
            
            for (var i = 0; i < pairsCount; i++)
            {
                var index = i * 2;
                _bufferData[_currentLength].Position = pointsPtr[index];
                _bufferData[_currentLength].DataIndex = dataIndex;
                _currentLength++;
                _bufferData[_currentLength].Position = pointsPtr[index + 1];
                _bufferData[_currentLength].DataIndex = dataIndex;
                _currentLength++;
            }
        }
        
        [BurstCompile]
        internal void Submit(float3* points, int length, uint dataIndex)
        {
            if (length % 2 != 0)
            {
                throw new ArgumentException("Length must be even to submit pairs of points.");
            }
            EnsureCapacity(length);
            
            var pairsCount = length / 2;
            
            for (var i = 0; i < pairsCount; i++)
            {
                var index = i * 2;
                _bufferData[_currentLength].Position = UseMatrix ? math.mul(Matrix, new float4(points[index], 1)).xyz : points[index];
                _bufferData[_currentLength].DataIndex = dataIndex;
                _currentLength++;
                _bufferData[_currentLength].Position = UseMatrix ? math.mul(Matrix, new float4(points[index + 1], 1)).xyz : points[index + 1];
                _bufferData[_currentLength].DataIndex = dataIndex;
                _currentLength++;
            }
        }

        [BurstDiscard]
        internal void FillBuffer(GraphicsBuffer buffer)
        {
            var data = (PositionData*)buffer.LockBufferForWrite<PositionData>(0, _currentLength).GetUnsafePtr();
            UnsafeUtility.MemCpy(data, _bufferData, _currentLength * sizeof(PositionData));
            buffer.UnlockBufferAfterWrite<PositionData>(_currentLength);
        }
        
        internal PositionData[] ToArray()
        {
            var array = new PositionData[Length];
            for (var i = 0; i < Length; i++)
                array[i] = UnsafeUtility.ReadArrayElement<PositionData>(_bufferData, i);
            return array;
        }

        public bool IsCreated => _bufferData != null;

        public void Dispose()
        {
            if(!IsCreated)
                return;
            
            UnsafeUtility.FreeTracked(_bufferData, _allocatorLabel);
            _bufferData = null;
        }
    }
}