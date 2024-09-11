using System;
using Unity.Mathematics;
using UnityEngine;

namespace Drawbug
{
    public class Draw : IDisposable
    {
        private static Draw _instance;

        private CommandBuffer _commandBuffer;
        private RenderData _renderData;
        private WireRender _wireRender;

        internal Draw()
        {
            if(_instance != null)
                return;
            
            _instance = this;
            _commandBuffer = new CommandBuffer(2048); // 2kb
            _renderData = new RenderData()
            {
                WireBuffer = new WireBuffer(1024) // 1kb
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
                fixed (WireBuffer* wireBufferPtr = &_renderData.WireBuffer)
                {
                    _wireRender.UpdateBuffer(wireBufferPtr, wireBufferPtr->Length);
                }
            }
        }

        internal void Render(UnityEngine.Rendering.CommandBuffer cmd)
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

        public static void Line(float3 point1, float3 point2)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Line(point1, point2);
        }
        
        public static void Line(Vector3 point1, Vector3 point2)
        {
            DrawbugManager.Initialize();
            
            _instance._commandBuffer.Line(point1, point2);
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