using System;
using Unity.Collections;
using UnityEngine;

namespace Drawbug
{
    internal class WireRender : IDisposable
    {
        private readonly Material _material = new(Resources.Load<Shader>("WireShader"));

        private int _positionsCount;

        private static readonly int PositionsProperty = Shader.PropertyToID("_Positions");
        private static readonly int StyleDataProperty = Shader.PropertyToID("_StyleData");
        
        private GraphicsBuffer _positions;
        private GraphicsBuffer _styleData;

        internal unsafe void UpdateBuffer(WireBuffer positions, NativeArray<CommandBuffer.StyleData> styleData, int count)
        {
            if (_positions == null || _positions.count < count)
            {
                _positions?.Dispose();
                _positions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.LockBufferForWrite, 
                    count, sizeof(PositionData));
                
                _styleData?.Dispose();
                _styleData = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 
                    count, sizeof(CommandBuffer.StyleData));
            }
            
            _positionsCount = count;
            
            positions.FillBuffer(_positions);
            _styleData.SetData(styleData);
        }

        internal void Render(UnityEngine.Rendering.CommandBuffer cmd)
        {
            cmd.SetGlobalBuffer(PositionsProperty, _positions);
            cmd.SetGlobalBuffer(StyleDataProperty, _styleData);
            cmd.DrawProcedural(Matrix4x4.identity, _material, 0, MeshTopology.Lines, _positionsCount);
        }

        public void Dispose()
        {
            _positions?.Dispose();
            _styleData?.Dispose();
        }
    }
}