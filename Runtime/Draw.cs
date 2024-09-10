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
        }

        internal unsafe void BuildData()
        {
            _renderData.ProcessCommands(_commandBuffer.GetBuffer());
        }

        internal void Render()
        {
            _renderData.GetCommandResults();
        }

        public void Dispose()
        {
            _instance = null;
            _commandBuffer.Dispose();
            _renderData.Dispose();
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

    }
}