using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug
{
    [BurstCompile]
    public unsafe struct WireBuffer : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct PositionData
        {
            public float3 Position;
            public uint DataIndex;
        }

        private PositionData* _bufferData;
        private int _currentLength;
        private int _bufferLength;

        public WireBuffer(int initialSize)
        {
            // _bufferData = (PositionData*)Marshal.AllocHGlobal(initialSize * Marshal.SizeOf(typeof(PositionData)));
            _bufferData = (PositionData*)UnsafeUtility.MallocTracked(sizeof(PositionData) * initialSize, 4, Allocator.Persistent, 4002);
            _currentLength = 0;
            _bufferLength = initialSize;

            // _submitJob = default;
            // _hasJob = false;
        }

        internal bool HasData => _currentLength > 0;

        internal int Length => _currentLength;

        internal void Clear()
        {
            _currentLength = 0;
        }
        
        private void EnsureCapacity(int additionalCapacity)
        {
            if (_currentLength + additionalCapacity > _bufferLength)
            {
                var newCapacity = Math.Max(_bufferLength * 2, _currentLength + additionalCapacity);
        
                // Aloca um novo bloco de memória
                PositionData* newBufferPtr = (PositionData*)UnsafeUtility.MallocTracked(newCapacity * sizeof(PositionData), 4, Allocator.Persistent, 4002);
        
                // Copia os dados antigos para o novo bloco de memória
                UnsafeUtility.MemCpy(newBufferPtr, _bufferData, _currentLength * sizeof(PositionData));
        
                // Libera o antigo bloco de memória
                UnsafeUtility.FreeTracked(_bufferData, Allocator.Persistent);

                // Atualiza o ponteiro do buffer e seu tamanho
                _bufferData = newBufferPtr;
                _bufferLength = newCapacity;

                Debug.Log("Successfully increased size of buffer to: " + _bufferLength);
            }
        }
        
        [BurstCompile]
        internal void Submit(float3 point1, float3 point2, uint dataIndex)
        {
            _bufferData[_currentLength].DataIndex = dataIndex;
            _bufferData[_currentLength++].Position = point1;
            _bufferData[_currentLength].DataIndex = dataIndex;
            _bufferData[_currentLength++].Position = point2;
        }

        // private JobHandle _submitJob;
        // private bool _hasJob;
        
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
            
            // _submitJob = new SubmitPointsJob
            // {
            //     Source = pointsPtr,
            //     Destination = _bufferData,
            //     DataIndex = dataIndex,
            //     CurrentLength = _currentLength,
            //     PairsCount = pairsCount
            // }.Schedule();
            // _hasJob = true;
            //
            // _currentLength += length;
            

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
            
            // _submitJob = new SubmitPointsJob
            // {
            //     Source = pointsPtr,
            //     Destination = _bufferData,
            //     DataIndex = dataIndex,
            //     CurrentLength = _currentLength,
            //     PairsCount = pairsCount
            // }.Schedule();
            // _hasJob = true;
            //
            // _currentLength += length;
            

            for (var i = 0; i < pairsCount; i++)
            {
                var index = i * 2;
                _bufferData[_currentLength].Position = points[index];
                _bufferData[_currentLength].DataIndex = dataIndex;
                _currentLength++;
                _bufferData[_currentLength].Position = points[index + 1];
                _bufferData[_currentLength].DataIndex = dataIndex;
                _currentLength++;
            }
        }

        // internal NativeArray<PositionData> GetBuffer()
        // {
        //     return new UnsafeList(_bufferData, _currentLength).;
        // }
        internal void FillBuffer(ref GraphicsBuffer buffer)
        {
            // if (_hasJob)
            // {
            //     _submitJob.Complete();
            //     _hasJob = false; 
            // }

            // var data = (PositionData*)buffer.LockBufferForWrite<PositionData>(0, _currentLength).GetUnsafePtr();
            // for (int i = 0; i < _currentLength; i++)
            // {
            //     data[i] = _bufferData[i];
            // }
            // buffer.UnlockBufferAfterWrite<PositionData>(_currentLength);
            
            var data = (PositionData*)buffer.LockBufferForWrite<PositionData>(0, _currentLength).GetUnsafePtr();
            UnsafeUtility.MemCpy(data, _bufferData, _currentLength * sizeof(PositionData));
            buffer.UnlockBufferAfterWrite<PositionData>(_currentLength);
        }

        public void Dispose()
        {
            if (_bufferData != null)
            {
                Debug.Log("Buffer memory successfully freed.");
                UnsafeUtility.FreeTracked(_bufferData, Allocator.Persistent);
                _bufferData = null;
            }
            // _bufferData.Dispose();
        }
        
        // [BurstCompile]
        // private struct SubmitPointsJob : IJob
        // {
        //     [NativeDisableUnsafePtrRestriction][ReadOnly] public float3* Source;
        //     [NativeDisableUnsafePtrRestriction] public PositionData* Destination;
        //     [ReadOnly] public uint DataIndex; 
        //     [ReadOnly] public int PairsCount; 
        //     [ReadOnly] public int CurrentLength;
        //     
        //     [BurstCompile]
        //     public void Execute()
        //     {
        //         for (var i = 0; i < PairsCount; i++)
        //         {
        //             var index = i * 2;
        //             Destination[CurrentLength + index].Position = Source[index];
        //             Destination[CurrentLength + index].DataIndex = DataIndex;
        //             
        //             Destination[CurrentLength + index + 1].Position = Source[index + 1];
        //             Destination[CurrentLength + index + 1].DataIndex = DataIndex;
        //         }
        //     }
        // }
    }
}