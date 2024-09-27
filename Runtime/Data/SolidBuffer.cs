using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug
{
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile]
    internal unsafe struct SolidBuffer : IDisposable
    {
        [NativeDisableUnsafePtrRestriction]
        private UnsafeSolidBuffer* _bufferData;
        
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        private static readonly SharedStatic<int> StaticSafetyId = SharedStatic<int>.GetOrCreate<SolidBuffer>();
#endif

        private Allocator _allocatorLabel;

        internal SolidBuffer(int initialVerticesCapacity, int initialTrianglesCapacity, Allocator allocator = Allocator.Persistent)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Native allocation is only valid for Temp, Job and Persistent
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            if (initialVerticesCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialVerticesCapacity), "Initial Capacity must be >= 0");
            if (initialTrianglesCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialTrianglesCapacity), "Initial Capacity must be >= 0");
#endif
            _bufferData = (UnsafeSolidBuffer*)UnsafeUtility.Malloc(sizeof(UnsafeSolidBuffer), UnsafeUtility.AlignOf<UnsafeSolidBuffer>(), allocator);
            *_bufferData = new UnsafeSolidBuffer(initialVerticesCapacity, initialTrianglesCapacity, ref allocator);
            
            _allocatorLabel = allocator;
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = CollectionHelper.CreateSafetyHandle(allocator);
            CollectionHelper.SetStaticSafetyId<SolidBuffer>(ref m_Safety, ref StaticSafetyId.Data);
#endif
        }
        
        internal int VerticesLength => _bufferData->VerticesLength;
        internal int VerticesCapacity => _bufferData->VerticesCapacity;
        internal int TrianglesLength => _bufferData->TrianglesLength;
        internal int TrianglesCapacity => _bufferData->TrianglesCapacity;
        
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
        
        internal void Submit(float3 point, uint dataIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Submit(point, dataIndex);
        }
        
        internal void Submit(NativeArray<float3> points, int length, uint dataIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Submit(points, length, dataIndex);
        }
        
        internal void Submit(float3* points, int length, uint dataIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Submit(points, length, dataIndex);
        }
        
        internal void Submit(int triangle)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Submit(triangle);
        }
        
        [BurstCompile]
        internal void Submit(NativeArray<int> triangles, int length)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Submit(triangles, length);
        }
        
        [BurstCompile]
        internal void Submit(int* triangles, int length)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            _bufferData->Submit(triangles, length);
        }

        [BurstDiscard]
        internal void FillVerticesBuffer(GraphicsBuffer buffer)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif

            _bufferData->FillVerticesBuffer(buffer);
        }
        
        [BurstDiscard]
        internal void FillTrianglesBuffer(GraphicsBuffer buffer)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif

            _bufferData->FillTrianglesBuffer(buffer);
        }

        internal bool IsCreated => _bufferData != null && _bufferData->IsCreated;

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
    
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile]
    internal unsafe struct UnsafeSolidBuffer : IDisposable
    {
        private UnsafeBuffer<PositionData>* _verticesBuffer;
        private UnsafeBuffer<int>* _trianglesBuffer;
        private Allocator _allocatorLabel;
        
        internal int VerticesLength => _verticesBuffer->Length;
        internal int VerticesCapacity => _verticesBuffer->Capacity;
        internal int TrianglesLength => _trianglesBuffer->Length;
        internal int TrianglesCapacity => _trianglesBuffer->Capacity;
        
        internal bool UseMatrix;
        internal float4x4 Matrix;

        internal UnsafeSolidBuffer(int initialVerticesCapacity, int initialTrianglesCapacity, ref Allocator allocator)
        {
            _allocatorLabel = allocator;

            // Aloca memória para os buffers e inicializa-os
            _verticesBuffer = (UnsafeBuffer<PositionData>*)UnsafeUtility.Malloc(
                sizeof(UnsafeBuffer<PositionData>), UnsafeUtility.AlignOf<UnsafeBuffer<PositionData>>(), allocator);
            *_verticesBuffer = new UnsafeBuffer<PositionData>(initialVerticesCapacity, ref allocator);

            _trianglesBuffer = (UnsafeBuffer<int>*)UnsafeUtility.Malloc(
                sizeof(UnsafeBuffer<int>), UnsafeUtility.AlignOf<UnsafeBuffer<int>>(), allocator);
            *_trianglesBuffer = new UnsafeBuffer<int>(initialTrianglesCapacity, ref allocator);
            
            UseMatrix = false;
            Matrix = float4x4.identity;
        }

        internal void Clear()
        {
            _verticesBuffer->Clear();
            _trianglesBuffer->Clear();
            
            UseMatrix = false;
            Matrix = float4x4.identity;
        }

        internal void Submit(float3 point, uint dataIndex)
        {
            _verticesBuffer->Submit(new PositionData
            {
                Position = UseMatrix ? math.mul(Matrix, new float4(point, 1)).xyz : point, 
                DataIndex = dataIndex
            });
        }

        internal void Submit(NativeArray<float3> points, int length, uint dataIndex)
        {
            _verticesBuffer->EnsureCapacity(length);
            for (var i = 0; i < length; i++)
            {
                _verticesBuffer->SubmitWithoutCheckCapacity(new PositionData
                {
                    Position = UseMatrix ? math.mul(Matrix, new float4(points[i], 1)).xyz : points[i], 
                    DataIndex = dataIndex
                });
            }
        }
        
        internal void Submit(float3* points, int length, uint dataIndex)
        {
            _verticesBuffer->EnsureCapacity(length);
            for (var i = 0; i < length; i++)
            {
                _verticesBuffer->SubmitWithoutCheckCapacity(new PositionData
                {
                    Position = UseMatrix ? math.mul(Matrix, new float4(points[i], 1)).xyz : points[i],
                    DataIndex = dataIndex
                });
            }
        }

        internal void Submit(int triangle)
        {
            _trianglesBuffer->Submit(triangle);
        }

        internal void Submit(NativeArray<int> triangles, int length)
        {
            _trianglesBuffer->Submit(triangles, length);
        }
        
        internal void Submit(int* triangles, int length)
        {
            _trianglesBuffer->Submit(triangles, length);
        }

        internal void FillVerticesBuffer(GraphicsBuffer buffer)
        {            
            _verticesBuffer->FillBuffer(buffer);
        }

        internal void FillTrianglesBuffer(GraphicsBuffer buffer)
        {
            _trianglesBuffer->FillBuffer(buffer);
        }

        internal bool IsCreated => _verticesBuffer != null && _trianglesBuffer != null;

        public void Dispose()
        {
            if (!IsCreated) return;

            // Libera a memória dos buffers
            _verticesBuffer->Dispose();
            UnsafeUtility.Free(_verticesBuffer, _allocatorLabel);
            _verticesBuffer = null;

            _trianglesBuffer->Dispose();
            UnsafeUtility.Free(_trianglesBuffer, _allocatorLabel);
            _trianglesBuffer = null;
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile]
    internal unsafe struct UnsafeBuffer<T> where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        private T* _data;
        private int _currentLength;
        private int _capacity;
        private Allocator _allocatorLabel;

        internal UnsafeBuffer(int initialCapacity, ref Allocator allocator)
        {
            _data = (T*)UnsafeUtility.Malloc(sizeof(T) * initialCapacity, UnsafeUtility.AlignOf<T>(), allocator);
            _currentLength = 0;
            _capacity = initialCapacity;
            _allocatorLabel = allocator;
        }

        internal int Length => _currentLength;
        internal int Capacity => _capacity;

        internal void Clear()
        {
            _currentLength = 0;
        }

        internal void EnsureCapacity(int additionalCapacity)
        {
            if (_currentLength + additionalCapacity > _capacity)
            {
                var newCapacity = Math.Max(_capacity * 2, _currentLength + additionalCapacity);
                
                var newBufferPtr = (T*)UnsafeUtility.Malloc(newCapacity * sizeof(T), UnsafeUtility.AlignOf<T>(), _allocatorLabel);
                UnsafeUtility.MemCpy(newBufferPtr, _data, _currentLength * sizeof(T));
                UnsafeUtility.Free(_data, _allocatorLabel);
                
                _data = newBufferPtr;
                _capacity = newCapacity;
            }
        }
        

        [BurstCompile]
        internal void Submit(T value)
        {
            EnsureCapacity(1);
            _data[_currentLength++] = value;
        }

        [BurstCompile]
        internal void Submit(NativeArray<T> values, int length)
        {
            EnsureCapacity(length);
            var valuesPtr = (T*)values.GetUnsafeReadOnlyPtr();
            for (var i = 0; i < length; i++)
            {
                _data[_currentLength++] = valuesPtr[i];
            }
        }
        
        [BurstCompile]
        internal void Submit(T* valuesPtr, int length)
        {
            EnsureCapacity(length);
            for (var i = 0; i < length; i++)
            {
                _data[_currentLength++] = valuesPtr[i];
            }
        }

        [BurstCompile]
        internal void SubmitWithoutCheckCapacity(T value)
        {
            _data[_currentLength++] = value;
        }
        
        [BurstDiscard]
        internal void FillBuffer(GraphicsBuffer buffer)
        {
            var data = (T*)buffer.LockBufferForWrite<T>(0, _currentLength).GetUnsafePtr();
            UnsafeUtility.MemCpy(data, _data, _currentLength * sizeof(T));
            buffer.UnlockBufferAfterWrite<T>(_currentLength);
        }

        internal bool IsCreated => _data != null;

        internal void Dispose()
        {
            if (!IsCreated) return;
            UnsafeUtility.Free(_data, _allocatorLabel);
            _data = null;
        }
    }
}