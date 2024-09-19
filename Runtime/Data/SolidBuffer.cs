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
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile]
    public unsafe struct SolidBuffer : IDisposable
    {
        [NativeDisableUnsafePtrRestriction]
        private UnsafeSolidBuffer* _bufferData;
        
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        private static readonly SharedStatic<int> StaticSafetyId = SharedStatic<int>.GetOrCreate<SolidBuffer>();
#endif

        private Allocator _allocatorLabel;

        public SolidBuffer(int initialVerticesCapacity, int initialTrianglesCapacity, Allocator allocator = Allocator.Persistent)
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

        public UnsafeSolidBuffer(int initialVerticesCapacity, int initialTrianglesCapacity, ref Allocator allocator)
        {
            _allocatorLabel = allocator;

            // Aloca memória para os buffers e inicializa-os
            _verticesBuffer = (UnsafeBuffer<PositionData>*)UnsafeUtility.Malloc(
                sizeof(UnsafeBuffer<PositionData>), UnsafeUtility.AlignOf<UnsafeBuffer<PositionData>>(), allocator);
            *_verticesBuffer = new UnsafeBuffer<PositionData>(initialVerticesCapacity, ref allocator);

            _trianglesBuffer = (UnsafeBuffer<int>*)UnsafeUtility.Malloc(
                sizeof(UnsafeBuffer<int>), UnsafeUtility.AlignOf<UnsafeBuffer<int>>(), allocator);
            *_trianglesBuffer = new UnsafeBuffer<int>(initialTrianglesCapacity, ref allocator);
        }

        public void Clear()
        {
            _verticesBuffer->Clear();
            _trianglesBuffer->Clear();
        }

        public void Submit(float3 point, uint dataIndex)
        {
            _verticesBuffer->Submit(new PositionData { Position = point, DataIndex = dataIndex });
        }

        public void Submit(NativeArray<float3> points, int length, uint dataIndex)
        {
            for (var i = 0; i < length; i++)
            {
                _verticesBuffer->Submit(new PositionData { Position = points[i], DataIndex = dataIndex });
            }
        }
        
        public void Submit(float3* points, int length, uint dataIndex)
        {
            for (var i = 0; i < length; i++)
            {
                _verticesBuffer->Submit(new PositionData { Position = points[i], DataIndex = dataIndex });
            }
        }

        public void Submit(int triangle)
        {
            _trianglesBuffer->Submit(triangle);
        }

        public void Submit(NativeArray<int> triangles, int length)
        {
            _trianglesBuffer->Submit(triangles, length);
        }
        
        public void Submit(int* triangles, int length)
        {
            _trianglesBuffer->Submit(triangles, length);
        }

        public void FillVerticesBuffer(GraphicsBuffer buffer)
        {            
            _verticesBuffer->FillBuffer(buffer);
        }

        public void FillTrianglesBuffer(GraphicsBuffer buffer)
        {
            _trianglesBuffer->FillBuffer(buffer);
        }

        public bool IsCreated => _verticesBuffer != null && _trianglesBuffer != null;

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

        public UnsafeBuffer(int initialCapacity, ref Allocator allocator)
        {
            _data = (T*)UnsafeUtility.Malloc(sizeof(T) * initialCapacity, UnsafeUtility.AlignOf<T>(), allocator);
            _currentLength = 0;
            _capacity = initialCapacity;
            _allocatorLabel = allocator;
        }

        public int Length => _currentLength;
        public int Capacity => _capacity;

        public void Clear()
        {
            _currentLength = 0;
        }

        private void EnsureCapacity(int additionalCapacity)
        {
            if (_currentLength + additionalCapacity > _capacity)
            {
                var newCapacity = Math.Max(_capacity * 2, _currentLength + additionalCapacity);
                T* newBufferPtr = (T*)UnsafeUtility.Malloc(newCapacity * sizeof(T), UnsafeUtility.AlignOf<T>(), _allocatorLabel);
                UnsafeUtility.MemCpy(newBufferPtr, _data, _currentLength * sizeof(T));
                UnsafeUtility.Free(_data, _allocatorLabel);
                _data = newBufferPtr;
                _capacity = newCapacity;
            }
        }

        [BurstCompile]
        public void Submit(T value)
        {
            EnsureCapacity(1);
            _data[_currentLength++] = value;
        }

        [BurstCompile]
        public void Submit(NativeArray<T> values, int length)
        {
            EnsureCapacity(length);
            var valuesPtr = (T*)values.GetUnsafeReadOnlyPtr();
            for (var i = 0; i < length; i++)
            {
                _data[_currentLength++] = valuesPtr[i];
            }
        }
        
        [BurstCompile]
        public void Submit(T* valuesPtr, int length)
        {
            EnsureCapacity(length);
            for (var i = 0; i < length; i++)
            {
                _data[_currentLength++] = valuesPtr[i];
            }
        }

        [BurstDiscard]
        public void FillBuffer(GraphicsBuffer buffer)
        {
            var data = (T*)buffer.LockBufferForWrite<T>(0, _currentLength).GetUnsafePtr();
            UnsafeUtility.MemCpy(data, _data, _currentLength * sizeof(T));
            buffer.UnlockBufferAfterWrite<T>(_currentLength);
        }

        public bool IsCreated => _data != null;

        public void Dispose()
        {
            if (!IsCreated) return;
            UnsafeUtility.Free(_data, _allocatorLabel);
            _data = null;
        }
    }

    
    // [StructLayout(LayoutKind.Sequential)]
    // [BurstCompile]
    // internal unsafe struct UnsafeSolidBuffer : IDisposable
    // {
    //     [NativeDisableUnsafePtrRestriction]
    //     private PositionData* _verticesData;
    //     private int _currentVerticesLength;
    //     private int _verticesCapacity;
    //     
    //     [NativeDisableUnsafePtrRestriction]
    //     private int* _trianglesData;
    //     private int _currentTrianglesLength;
    //     private int _trianglesCapacity;
    //     
    //
    //     private Allocator _allocatorLabel;
    //     
    //
    //     public UnsafeSolidBuffer(int initialVerticesCapacity, int initialTrianglesCapacity, ref Allocator allocator)
    //     {
    //         _verticesData = (PositionData*)UnsafeUtility.Malloc(sizeof(PositionData) * initialVerticesCapacity, UnsafeUtility.AlignOf<PositionData>(), allocator);
    //         _trianglesData = (int*)UnsafeUtility.Malloc(sizeof(int) * initialTrianglesCapacity, UnsafeUtility.AlignOf<int>(), allocator);
    //         
    //         _currentVerticesLength = 0;
    //         _verticesCapacity = initialVerticesCapacity;
    //         
    //         _currentTrianglesLength = 0;
    //         _trianglesCapacity = initialTrianglesCapacity;
    //         
    //         _allocatorLabel = allocator;
    //     }
    //     
    //     internal int VerticesLength => _currentVerticesLength;
    //     internal int VerticesCapacity => _verticesCapacity;
    //     internal int TrianglesLength => _currentTrianglesLength;
    //     internal int TrianglesCapacity => _trianglesCapacity;
    //
    //     internal void Clear()
    //     {
    //         _currentVerticesLength = 0;
    //         _currentTrianglesLength = 0;
    //     }
    //     
    //     private void EnsureVerticesCapacity(int additionalCapacity)
    //     {
    //         if (_currentVerticesLength + additionalCapacity > _verticesCapacity)
    //         {
    //             var newCapacity = Math.Max(_verticesCapacity * 2, _currentVerticesLength + additionalCapacity);
    //     
    //             // Aloca um novo bloco de memória
    //             PositionData* newBufferPtr = (PositionData*)UnsafeUtility.Malloc(newCapacity * sizeof(PositionData), UnsafeUtility.AlignOf<PositionData>(), _allocatorLabel);
    //     
    //             // Copia os dados antigos para o novo bloco de memória
    //             UnsafeUtility.MemCpy(newBufferPtr, _verticesData, _currentVerticesLength * sizeof(PositionData));
    //     
    //             // Libera o antigo bloco de memória
    //             UnsafeUtility.FreeTracked(_verticesData, _allocatorLabel);
    //
    //             // Atualiza o ponteiro do buffer e seu tamanho
    //             _verticesData = newBufferPtr;
    //             _verticesCapacity = newCapacity;
    //
    //             Debug.Log("Successfully increased size of vertices buffer to: " + _verticesCapacity);
    //         }
    //     }
    //     
    //     private void EnsureTrianglesCapacity(int additionalCapacity)
    //     {
    //         if (_currentTrianglesLength + additionalCapacity > _trianglesCapacity)
    //         {
    //             var newCapacity = Math.Max(_trianglesCapacity * 2, _currentTrianglesLength + additionalCapacity);
    //     
    //             // Aloca um novo bloco de memória
    //             int* newBufferPtr = (int*)UnsafeUtility.Malloc(newCapacity * sizeof(int), UnsafeUtility.AlignOf<int>(), _allocatorLabel);
    //     
    //             // Copia os dados antigos para o novo bloco de memória
    //             UnsafeUtility.MemCpy(newBufferPtr, _trianglesData, _currentTrianglesLength * sizeof(int));
    //     
    //             // Libera o antigo bloco de memória
    //             UnsafeUtility.FreeTracked(_trianglesData, _allocatorLabel);
    //
    //             // Atualiza o ponteiro do buffer e seu tamanho
    //             _trianglesData = newBufferPtr;
    //             _trianglesCapacity = newCapacity;
    //
    //             Debug.Log("Successfully increased size of triangles buffer to: " + _trianglesCapacity);
    //         }
    //     }
    //     
    //     [BurstCompile]
    //     internal void Submit(float3 point, uint dataIndex)
    //     {
    //         EnsureVerticesCapacity(1);
    //         _verticesData[_currentVerticesLength].DataIndex = dataIndex;
    //         _verticesData[_currentVerticesLength++].Position = point;
    //     }
    //     
    //     [BurstCompile]
    //     internal void Submit(NativeArray<float3> points, int length, uint dataIndex)
    //     {
    //         EnsureVerticesCapacity(length);
    //         
    //         var pointsPtr = (float3*)points.GetUnsafeReadOnlyPtr();
    //         
    //         for (var i = 0; i < length; i++)
    //         {
    //             _verticesData[_currentVerticesLength].Position = pointsPtr[i];
    //             _verticesData[_currentVerticesLength++].DataIndex = dataIndex;
    //         }
    //     }
    //     
    //     [BurstCompile]
    //     internal void Submit(int index)
    //     {
    //         EnsureTrianglesCapacity(1);
    //         _trianglesData[_currentVerticesLength++] = index;
    //     }
    //     
    //     [BurstCompile]
    //     internal void Submit(NativeArray<int> triangles, int length)
    //     {
    //         EnsureTrianglesCapacity(length);
    //         
    //         var trianglesPtr = (int*)triangles.GetUnsafeReadOnlyPtr();
    //         
    //         for (var i = 0; i < length; i++)
    //         {
    //             _trianglesData[_currentTrianglesLength++] = trianglesPtr[i];
    //         }
    //     }
    //
    //     [BurstDiscard]
    //     internal void FillVerticesBuffer(GraphicsBuffer buffer)
    //     {
    //         var data = (PositionData*)buffer.LockBufferForWrite<PositionData>(0, _currentVerticesLength).GetUnsafePtr();
    //         UnsafeUtility.MemCpy(data, _verticesData, _currentVerticesLength * sizeof(PositionData));
    //         buffer.UnlockBufferAfterWrite<PositionData>(_currentVerticesLength);
    //     }
    //     
    //     [BurstDiscard]
    //     internal void FillTrianglesBuffer(GraphicsBuffer buffer)
    //     {
    //         var data = (int*)buffer.LockBufferForWrite<int>(0, _currentTrianglesLength).GetUnsafePtr();
    //         UnsafeUtility.MemCpy(data, _trianglesData, _currentTrianglesLength * sizeof(int));
    //         buffer.UnlockBufferAfterWrite<int>(_currentTrianglesLength);
    //     }
    //
    //     public bool IsCreated => _verticesData != null && _trianglesData != null;
    //
    //     public void Dispose()
    //     {
    //         if(!IsCreated)
    //             return;
    //         
    //         Debug.Log("Buffer memory successfully freed.");
    //         UnsafeUtility.FreeTracked(_verticesData, _allocatorLabel);
    //         _verticesData = null;
    //         UnsafeUtility.FreeTracked(_trianglesData, _allocatorLabel);
    //         _trianglesData = null;
    //     }
    // }
}