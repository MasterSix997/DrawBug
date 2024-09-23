using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug
{
    public class Draw : IDisposable
    {
        private static Draw _instance;

        private DrawCommandBuffer _commandBuffer;
        private DrawCommandBuffer _fixedCommandBuffer;
        private RenderData _renderData;
        private readonly WireRender _wireRender;
        private readonly SolidRender _solidRender;

        private DrawCommandBufferTimed _timedCommandBuffer;
        private float _currentDuration = 0;
        
        internal Draw()
        {
            if(_instance != null)
                return;
            
            _instance = this;
            
            _commandBuffer = new DrawCommandBuffer(2048);
            _fixedCommandBuffer = new DrawCommandBuffer(1024);
            _timedCommandBuffer = new DrawCommandBufferTimed(200);
            _currentDuration = 0;
            
            _renderData = new RenderData
            {
                WireBuffer = new WireBuffer(2048),
                SolidBuffer = new SolidBuffer(2048, 6144),
                StyleData = new NativeList<DrawCommandBuffer.StyleData>(1024, Allocator.Persistent),
                //TODO)) Within the job, when you exceed the position or triangle buffer capacity, Submit the data and subtract the amount of data sent for return
                TempFloat3Buffer = new NativeArray<float3>(2048, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
                TempTriangleBuffer = new NativeArray<int>(10000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
            };
            _renderData.CreatePtr();
            _wireRender = new WireRender();
            _solidRender = new SolidRender();
        }

        internal unsafe void BuildData()
        {
            if (_fixedCommandBuffer.HasData)
                _commandBuffer.AnotherBuffer(_fixedCommandBuffer);

            _timedCommandBuffer.SendCommandsToBuffer(ref _commandBuffer);
                
            if (_commandBuffer.HasData)
            {
                _renderData.ProcessCommands(_commandBuffer.GetBuffer());
            }
        }

        internal void GetDataResults()
        {
            if (_commandBuffer.HasData)
            {
                _renderData.GetCommandResults();
                var styleData = _renderData.StyleData.ToArray(Allocator.Temp);
                _wireRender.UpdateBuffer(_renderData.WireBuffer, styleData, _renderData.WireBuffer.Length);
                _solidRender.UpdateBuffer(_renderData.SolidBuffer, styleData, _renderData.SolidBuffer.TrianglesLength);
            }
        }

        internal void Render(UnityEngine.Rendering.CommandBuffer cmd)
        {
            if (_commandBuffer.HasData)
            {
                _wireRender.Render(cmd);
                _solidRender.Render(cmd);
            }
        }
        
        internal void Render(UnityEngine.Rendering.RasterCommandBuffer cmd)
        {
            if (_commandBuffer.HasData)
            {
                _wireRender.Render(cmd);
                _solidRender.Render(cmd);
            }
        }

        public void Dispose()
        {
            _commandBuffer.Dispose();
            _fixedCommandBuffer.Dispose();
            _timedCommandBuffer.Dispose();
            _renderData.Dispose();
            _wireRender.Dispose();
            _solidRender.Dispose();
            _instance = null;
        }
        
        internal void Clear()
        {
            _commandBuffer.Clear();
            _currentDuration = 0;
        }
        
        internal void ClearFixed()
        {
            _fixedCommandBuffer.Clear();
            _currentDuration = 0;
        }
        
        public void UpdateTimedBuffers(float deltaTime)
        {
            _timedCommandBuffer.UpdateTimes(deltaTime);
            Debug.Log(_timedCommandBuffer.ActiveBuffersCount);
        }

        private static unsafe DrawCommandBuffer* CurrentCommandBuffer
        {
            get
            {
                DrawbugManager.Initialize();
                var currentDuration = _instance._currentDuration;

                if (currentDuration > 0)
                    return _instance._timedCommandBuffer.GetBuffer(currentDuration);

                fixed (DrawCommandBuffer* fixedBuffer = &_instance._fixedCommandBuffer)
                fixed (DrawCommandBuffer* commandBuffer = &_instance._commandBuffer)
                {
                    return Time.inFixedTimeStep ? fixedBuffer : commandBuffer;
                }
            }
        }

        // private static ref DrawCommandBuffer CurrentCommandBuffer
        // {
        //     get
        //     {
        //         DrawbugManager.Initialize();
        //         var currentDuration = _instance._currentDuration;
        //         if (currentDuration > 0)
        //         {
        //             var lastTimedBuffer = ref _instance._timedCommandBuffer[^1];
        //             
        //             if (lastTimedBuffer.Item2 == currentDuration)
        //             {
        //                 return ref lastTimedBuffer;
        //             }
        //
        //             _instance._timedCommandBuffer.Add((new DrawCommandBuffer(512), currentDuration));
        //             return ref _instance._timedCommandBuffer[^1];
        //         }
        //         
        //         return ref Time.inFixedTimeStep ? 
        //             ref _instance._fixedCommandBuffer : 
        //             ref _instance._commandBuffer;
        //     }
        // }

        public static unsafe void Reset()
        {
            CurrentCommandBuffer->ResetStyle();
            CurrentCommandBuffer->DrawMode(DrawMode.Wire);
            CurrentCommandBuffer->Matrix(float4x4.identity);
            _instance._currentDuration = 0;
        }

        public static unsafe Color Color
        {
            get => CurrentCommandBuffer->PendingStyle.color;
            set => CurrentCommandBuffer->StyleColor(value);
        }
        
        public static unsafe bool Forward
        {
            get => CurrentCommandBuffer->PendingStyle.forward;
            set => CurrentCommandBuffer->StyleForward(value);
        }
        
        public static unsafe DrawMode DrawMode
        {
            get => CurrentCommandBuffer->CurrentDrawMode;
            set => CurrentCommandBuffer->DrawMode(value);
        }
        
        public static unsafe float Duration
        {
            get
            {
                DrawbugManager.Initialize();
                return _instance._currentDuration;
            }
            set
            {
                DrawbugManager.Initialize();
                _instance._currentDuration = value;
            }
        }
        
        public static unsafe float4x4 Matrix
        {
            get => CurrentCommandBuffer->CurrentMatrix;
            set => CurrentCommandBuffer->Matrix(value);
        }
        
        public readonly struct DrawScope<T> : IDisposable
        {
            private readonly T _before;
            private readonly Action<T> _setter;
            public DrawScope(T current, T newValue, Action<T> setter)
            {
                _before = current;
                _setter = setter;
                _setter.Invoke(newValue);
            }
            
            public void Dispose()
            {
                _setter.Invoke(_before);
            }
        }
        
        public static unsafe DrawScope<Color> WithColor(Color newValue)
        {
            return new DrawScope<Color>(Color, newValue, previousValue => Color = previousValue);
        }
        
        public static unsafe DrawScope<bool> WithForward(bool newValue = true)
        {
            return new DrawScope<bool>(Forward, newValue, previousValue => Forward = previousValue);
        }
        
        public static unsafe DrawScope<DrawMode> WithDrawMode(DrawMode newValue)
        {
            return new DrawScope<DrawMode>(DrawMode, newValue, previousValue => DrawMode = previousValue);
        }
        
        public static unsafe DrawScope<float> WithDuration(float newValue)
        {
            return new DrawScope<float>(Duration, newValue, previousValue => Duration = previousValue);
        }
        
        public static unsafe DrawScope<float4x4> WithMatrix(float4x4 newValue)
        {
            return new DrawScope<float4x4>(Matrix, newValue, previousValue => Matrix = previousValue);
        }
        
        public static unsafe DrawScope<float4x4> InLocalSpace(Transform transform)
        {
            return new DrawScope<float4x4>(Matrix, transform.localToWorldMatrix, previousValue => Matrix = previousValue);
        }
        
        public static unsafe DrawScope<float4x4> InPosition(float3 position)
        {
            return new DrawScope<float4x4>(Matrix, float4x4.TRS(position, quaternion.identity, new float3(1)), previousValue => Matrix = previousValue);
        }

        public static unsafe void Line(float3 point1, float3 point2)
        {
            CurrentCommandBuffer->Line(point1, point2);
        }
        
        public static unsafe void Line(Vector3 point1, Vector3 point2)
        {
            CurrentCommandBuffer->Line(point1, point2);
        }

        public static unsafe void Lines(float3[] lines)
        {
            CurrentCommandBuffer->Lines(lines);
        }
        
        public static unsafe void Lines(NativeArray<float3> lines)
        {
            CurrentCommandBuffer->Lines(lines);
        }
        
        public static unsafe void Rectangle(float3 position, float3 scale, quaternion rotation)
        {
            CurrentCommandBuffer->Rectangle(position, scale, rotation);
        }
        
        public static unsafe void Circle(float3 position, float radius, quaternion rotation)
        {
            CurrentCommandBuffer->Circle(position, radius, rotation);
        }
        
        public static unsafe void HollowCircle(float3 position, float innerRadius, float outerRadius, quaternion rotation)
        {
            CurrentCommandBuffer->HollowCircle(position, innerRadius, outerRadius, rotation);
        }
        
        public static unsafe void Capsule(float3 position, float2 size, quaternion rotation, bool isVertical)
        {
            CurrentCommandBuffer->Capsule(position, size, rotation, isVertical);
        }
        
        public static unsafe void Box(float3 position, float scale)
        {
            CurrentCommandBuffer->Box(position, scale, quaternion.identity);
            // DrawbugManager.Initialize();
            // if (_hasDuration)
            // {
            //     _instance._durationBuffers[_currentDurationBuffer];
            // }
            // else if (Time.inFixedTimeStep)
            // {
            // _instance._fixedCommandBuffer.Box(position, scale, quaternion.identity);
            // }
            // else
            // {
            // _instance._commandBuffer.Box(position, scale, quaternion.identity);
            // }
            
        }
        
        public static unsafe void Box(float3 position, float3 scale)
        {
            CurrentCommandBuffer->Box(position, scale, quaternion.identity);
        }

        public static unsafe void Box(float3 position, float3 scale, quaternion rotation)
        {
            CurrentCommandBuffer->Box(position, scale, rotation);
        }

        public static unsafe void Sphere(float3 position, float radius)
        {
            CurrentCommandBuffer->Sphere(position, radius);
        }
        
        public static unsafe void Cylinder(float3 position, float radius, float height, quaternion rotation)
        {
            CurrentCommandBuffer->Cylinder(position, radius, height, rotation);
        }
        
        public static unsafe void Capsule3D(float3 position, float radius, float height, quaternion rotation)
        {
            CurrentCommandBuffer->Capsule3D(position, radius, height, rotation);
        }
    }
}