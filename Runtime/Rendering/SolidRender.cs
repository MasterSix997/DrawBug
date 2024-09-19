using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace Drawbug
{
    internal class SolidRender : IDisposable
    {
        private readonly Material _material = new(Resources.Load<Shader>("SolidShader"));

        private int _trianglesCount;

        private static readonly int PositionsProperty = Shader.PropertyToID("_Positions");
        private static readonly int StyleDataProperty = Shader.PropertyToID("_StyleData");
        
        private GraphicsBuffer _positions;
        private GraphicsBuffer _triangles;
        private GraphicsBuffer _styleData;
        
        internal bool CanRender { get; private set; }

        internal unsafe void UpdateBuffer(SolidBuffer dataBuffer, NativeArray<DrawCommandBuffer.StyleData> styleData, int trianglesCount)
        {
            if (trianglesCount < 3)
            {
                CanRender = false;
                return;
            }
            
            Profiler.BeginSample("Update Solid Buffer");
            
            if (_positions == null || _positions.count < trianglesCount)
            {
                _positions?.Dispose();
                _positions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.LockBufferForWrite, 
                    trianglesCount, sizeof(PositionData));
                
                _triangles?.Dispose();
                _triangles = new GraphicsBuffer(GraphicsBuffer.Target.Index, GraphicsBuffer.UsageFlags.LockBufferForWrite, 
                    trianglesCount, sizeof(int));
                
                _styleData?.Dispose();
                _styleData = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 
                    trianglesCount, sizeof(DrawCommandBuffer.StyleData));
            }
            
            _trianglesCount = trianglesCount;
            
            dataBuffer.FillVerticesBuffer(_positions);
            dataBuffer.FillTrianglesBuffer(_triangles);
            
            _styleData.SetData(styleData);
            
            _material.SetBuffer(PositionsProperty, _positions);
            _material.SetBuffer(StyleDataProperty, _styleData);

            CanRender = _positions != null && _triangles != null;
            
            Profiler.EndSample();
        }

        internal void Render(UnityEngine.Rendering.CommandBuffer cmd)
        {
            if (!CanRender)
                return;
            
            cmd.DrawProcedural(_triangles, Matrix4x4.identity, _material, -1, MeshTopology.Triangles, _trianglesCount);
        }
        
        internal void Render(UnityEngine.Rendering.RasterCommandBuffer cmd)
        {
            if (!CanRender)
                return;
            
            cmd.DrawProcedural(_triangles, Matrix4x4.identity, _material, -1, MeshTopology.Triangles, _trianglesCount);
        }

        public void Dispose()
        {
            _positions?.Dispose();
            _triangles?.Dispose();
            _styleData?.Dispose();
        }
    }
}