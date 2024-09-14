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

        internal Draw()
        {
            if(_instance != null)
                return;
            
            _instance = this;
            _commandBuffer = new DrawCommandBuffer(2048);
            _renderData = new RenderData()
            {
                WireBuffer = new WireBuffer(2048),
                StyleData = new NativeList<DrawCommandBuffer.StyleData>(1024, Allocator.Persistent)
            };
            _wireRender = new WireRender();
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
            _renderData.GetCommandResults();
            if (_commandBuffer.HasData)
            {
                _wireRender.UpdateBuffer(_renderData.WireBuffer, _renderData.StyleData.ToArray(Allocator.Temp), _renderData.WireBuffer.Length);
            }
        }

        internal void Render(UnityEngine.Rendering.CommandBuffer cmd)
        {
            if (_commandBuffer.HasData)
            {
                _wireRender.Render(cmd);
            }
        }
        
        internal void Render(UnityEngine.Rendering.RasterCommandBuffer cmd)
        {
            if (_commandBuffer.HasData)
            {
                _wireRender.Render(cmd);
            }
        }

        public void Dispose()
        {
            _instance = null;
            _commandBuffer.Dispose();
            _renderData.Dispose();
            _wireRender.Dispose();
        }
        
        internal void Clear()
        {
            _commandBuffer.Clear();
        }

        public static void Reset()
        {
            DrawbugManager.Initialize();
            _instance._commandBuffer.ResetStyle();
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
                return _instance._commandBuffer.PendingStyle.foward;
            }
            set
            {
                DrawbugManager.Initialize();
                _instance._commandBuffer.StyleForward(value);
            }
        }
        
        public readonly struct ColorScope : IDisposable
        {
            private readonly Color _beforeColor;
            
            public ColorScope(Color newColor)
            {
                _beforeColor = Color;
                Color = newColor;
            }
            
            public void Dispose()
            {
                Color = _beforeColor;
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

        public static ColorScope WithColorScope(Color color)
        {
            return new ColorScope(color);
        }
        
        public static DrawScope<Color> WithColor(Color newColor)
        {
            return new DrawScope<Color>(Color, newColor, beforeColor => Color = beforeColor);
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
        
        public static void Cube(float3 position, float scale)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Cube(position, scale, quaternion.identity);
        }
        
        public static void Cube(float3 position, float3 scale)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Cube(position, scale, quaternion.identity);
        }

        public static void Cube(float3 position, float3 scale, quaternion rotation)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Cube(position, scale, rotation);
        }

    }
}