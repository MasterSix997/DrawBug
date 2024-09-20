using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Drawbug
{
    public class Draw : IDisposable
    {
        private static Draw _instance;

        private DrawCommandBuffer _commandBuffer;
        private RenderData _renderData;
        private WireRender _wireRender;
        private SolidRender _solidRender;

        internal Draw()
        {
            if(_instance != null)
                return;
            
            _instance = this;
            _commandBuffer = new DrawCommandBuffer(2048);
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
            if (_commandBuffer.HasData)
            {
                _renderData.ProcessCommands(_commandBuffer.GetBuffer());
            }
        }

        internal unsafe void GetDataResults()
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
            _instance = null;
            _commandBuffer.Dispose();
            _renderData.Dispose();
            _wireRender.Dispose();
            _solidRender.Dispose();
        }
        
        internal void Clear()
        {
            _commandBuffer.Clear();
        }

        public static void Reset()
        {
            DrawbugManager.Initialize();
            _instance._commandBuffer.ResetStyle();
            _instance._commandBuffer.DrawMode(DrawMode.Wire);
            _instance._commandBuffer.Matrix(float4x4.identity);
        }

        public static Color Color
        {
            get
            {
                DrawbugManager.Initialize();
                return _instance._commandBuffer.PendingStyle.color;
            }
            set
            {
                DrawbugManager.Initialize();
                _instance._commandBuffer.StyleColor(value);
            }
        }
        
        public static bool Forward
        {
            get
            {
                DrawbugManager.Initialize();
                return _instance._commandBuffer.PendingStyle.forward;
            }
            set
            {
                DrawbugManager.Initialize();
                _instance._commandBuffer.StyleForward(value);
            }
        }
        
        public static DrawMode DrawMode
        {
            get
            {
                DrawbugManager.Initialize();
                return _instance._commandBuffer.CurrentDrawMode;
            }
            set
            {
                DrawbugManager.Initialize();
                _instance._commandBuffer.DrawMode(value);
            }
        }
        
        public static float4x4 Matrix
        {
            get
            {
                DrawbugManager.Initialize();
                return _instance._commandBuffer.CurrentMatrix;
            }
            set
            {
                DrawbugManager.Initialize();
                _instance._commandBuffer.Matrix(value);
            }
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
        
        public static DrawScope<Color> WithColor(Color newValue)
        {
            return new DrawScope<Color>(Color, newValue, previousValue => Color = previousValue);
        }
        
        public static DrawScope<bool> WithForward(bool newValue = true)
        {
            return new DrawScope<bool>(Forward, newValue, previousValue => Forward = previousValue);
        }
        
        public static DrawScope<DrawMode> WithDrawMode(DrawMode newValue)
        {
            return new DrawScope<DrawMode>(DrawMode, newValue, previousValue => DrawMode = previousValue);
        }
        
        public static DrawScope<float4x4> WithMatrix(float4x4 newValue)
        {
            return new DrawScope<float4x4>(Matrix, newValue, previousValue => Matrix = previousValue);
        }
        
        public static DrawScope<float4x4> InLocalSpace(Transform transform)
        {
            return new DrawScope<float4x4>(Matrix, transform.localToWorldMatrix, previousValue => Matrix = previousValue);
        }
        
        public static DrawScope<float4x4> InPosition(float3 position)
        {
            return new DrawScope<float4x4>(Matrix, float4x4.TRS(position, quaternion.identity, new float3(1)), previousValue => Matrix = previousValue);
        }

        public static void Line(float3 point1, float3 point2)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Line(point1, point2);
        }
        
        public static void Line(Vector3 point1, Vector3 point2)
        {
            Profiler.BeginSample("Line");
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Line(point1, point2);
            Profiler.EndSample();
        }

        public static void Lines(float3[] lines)
        {
            Profiler.BeginSample("Lines");
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Lines(lines);
            Profiler.EndSample();
        }
        
        public static void Lines(NativeArray<float3> lines)
        {
            Profiler.BeginSample("Lines");
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Lines(lines);
            Profiler.EndSample();
        }
        
        public static void Rectangle(float3 position, float3 scale, quaternion rotation)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Rectangle(position, scale, rotation);
        }
        
        public static void Circle(float3 position, float radius, quaternion rotation)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Circle(position, radius, rotation);
        }
        
        public static void HollowCircle(float3 position, float innerRadius, float outerRadius, quaternion rotation)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.HollowCircle(position, innerRadius, outerRadius, rotation);
        }
        
        public static void Capsule(float3 position, float2 size, quaternion rotation, bool isVertical)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Capsule(position, size, rotation, isVertical);
        }
        
        public static void Box(float3 position, float scale)
        {
            if (Time.inFixedTimeStep)
            {
                
            }
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Box(position, scale, quaternion.identity);
        }
        
        public static void Box(float3 position, float3 scale)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Box(position, scale, quaternion.identity);
        }

        public static void Box(float3 position, float3 scale, quaternion rotation)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Box(position, scale, rotation);
        }

        public static void Sphere(float3 position, float radius)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Sphere(position, radius);
        }
        
        public static void Cylinder(float3 position, float radius, float height, quaternion rotation)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Cylinder(position, radius, height, rotation);
        }
        
        public static void Capsule3D(float3 position, float radius, float height, quaternion rotation)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Capsule3D(position, radius, height, rotation);
        }
    }
}