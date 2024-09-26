using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Drawbug
{
    [StructLayout(LayoutKind.Sequential)]
     internal unsafe struct DrawCommandBufferInfo
     {
         internal DrawCommandBuffer* Buffer;
         internal float RemainingTime;
         internal bool IsActive;
         internal bool IsCreated => Buffer != null;
     }
     
     [StructLayout(LayoutKind.Sequential)]
     [BurstCompile]
     internal unsafe struct DrawCommandBufferTimed : IDisposable
     {
         private DrawCommandBufferInfo[] _bufferPool;

         private int _capacity;
         private int _activeBuffersCount;

         internal int Capacity => _capacity;
         internal int ActiveBuffersCount => _activeBuffersCount;
         
         internal DrawCommandBufferTimed(int initialCapacity)
         {
             _capacity = initialCapacity;
             _activeBuffersCount = 0;

             // _bufferPool = (DrawCommandBufferInfo*)UnsafeUtility.Malloc(sizeof(DrawCommandBufferInfo) * _capacity, UnsafeUtility.AlignOf<DrawCommandBufferInfo>(), Allocator.Persistent);
             _bufferPool = new DrawCommandBufferInfo[_capacity];

             InitializePool();
         }

         internal DrawCommandBuffer* GetBuffer(float duration)
         {
             // ReSharper disable all CompareOfFloatsByEqualityOperator
             for (int i = 0; i < _capacity; i++)
             {
                 if (_activeBuffersCount == _capacity)
                     break;
                 
                 if (_bufferPool[i].IsActive && _bufferPool[i].RemainingTime == duration)
                 {
                     return _bufferPool[i].Buffer;
                 }
                 
                 if (!_bufferPool[i].IsActive)
                 {
                     _bufferPool[i].RemainingTime = duration;
                     _bufferPool[i].IsActive = true;
                     _activeBuffersCount++;
                     return _bufferPool[i].Buffer;
                 }
             }

             if (_bufferPool[^1].RemainingTime != duration)
             {
                 _bufferPool[^1].Buffer->Clear();
             }
             
             _bufferPool[^1].RemainingTime = duration;
             return _bufferPool[^1].Buffer;
         }

         internal void SendCommandsToBuffer(ref DrawCommandBuffer buffer)
         {
             var iteratedCount = 0;
             for (var i = 0; i < _capacity; i++)
             {
                 if (iteratedCount >= _activeBuffersCount)
                     return;
                 
                 if (_bufferPool[i].IsActive)
                 {
                     iteratedCount++;
                     buffer.AnotherBuffer(_bufferPool[i].Buffer);
                 }
             }
         }

         internal void UpdateTimes(float deltaTime)
         {
             var iteratedCount = 0;
             for (int i = 0; i < _capacity; i++)
             {
                 if (iteratedCount >= _activeBuffersCount)
                     return;
                 
                 if (_bufferPool[i].IsActive)
                 {
                     iteratedCount++;
                     _bufferPool[i].RemainingTime -= deltaTime;

                     if (_bufferPool[i].RemainingTime <= 0)
                     {
                         _bufferPool[i].RemainingTime = 0;
                         _bufferPool[i].IsActive = false;
                         _bufferPool[i].Buffer->Clear();
                         _activeBuffersCount--;
                         iteratedCount--;
                     }

                 }
             }
         }
         
         private void InitializePool(int from = 0)
         {
             for (var i = from; i < _capacity; i++)
             {
                 _bufferPool[i] = new DrawCommandBufferInfo
                 {
                     Buffer = (DrawCommandBuffer*)UnsafeUtility.Malloc(
                         sizeof(DrawCommandBuffer),
                         UnsafeUtility.AlignOf<DrawCommandBuffer>(),
                         Allocator.Persistent),
                     RemainingTime = 0,
                     IsActive = false
                 };
                 *_bufferPool[i].Buffer = new DrawCommandBuffer(512);
             }
         }
         
    //      private void ExpandPool()
    //      {
    //          var newCapacity = _capacity * 2;
    //          
    // #if ENABLE_UNITY_COLLECTIONS_CHECKS
    //          const int MAX_BUFFER_SIZE = 1024 * 1024 * 256; // 256 MB
    //          if (newCapacity > MAX_BUFFER_SIZE) {
    //              throw new Exception("DrawCommandBuffer pool is very large");
    //          }
    // #endif
    //
    //          var newBufferPool = (DrawCommandBufferInfo*)UnsafeUtility.Malloc(sizeof(DrawCommandBufferInfo) * newCapacity, UnsafeUtility.AlignOf<DrawCommandBufferInfo>(), Allocator.Persistent);
    //
    //          UnsafeUtility.MemCpy(newBufferPool, _bufferPool, sizeof(DrawCommandBufferInfo) * _capacity);
    //          UnsafeUtility.Free(_bufferPool, Allocator.Persistent);
    //
    //          _bufferPool = newBufferPool;
    //          _capacity = newCapacity;
    //          InitializePool(_length);
    //      }

         internal bool IsCreated => _bufferPool != null;

         public void Dispose()
         {
             // if (!IsCreated)
             //     return;
             
             for (var i = 0; i < _capacity; i++)
             {
                 // if (!_bufferPool[i].IsCreated) 
                 //     continue;
                 _bufferPool[i].Buffer->Dispose();
                 UnsafeUtility.Free(_bufferPool[i].Buffer, Allocator.Persistent);
                 _bufferPool[i].Buffer = null;
             }

             // UnsafeUtility.Free(_bufferPool, Allocator.Persistent);
             _bufferPool = null;
         }
     }
}