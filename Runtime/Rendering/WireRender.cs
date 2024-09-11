using System;
using UnityEngine;

namespace Drawbug
{
    internal class WireRender : IDisposable
    {
        private readonly Material _material = new(Resources.Load<Shader>("WireShader"));

        private int _positionsCount;

        // private RenderParams _renderParams;
        //
        private static readonly int PositionsProperty = Shader.PropertyToID("_Positions");
        private static readonly int LineDataProperty = Shader.PropertyToID("_LineData");
        // private static readonly int ObjectToWorldProperty = Shader.PropertyToID("_ObjectToWorld");
        private GraphicsBuffer _positions;
        private GraphicsBuffer _lineData;
        // private Mesh _mesh = new();

        internal unsafe void UpdateBuffer(WireBuffer* positions, int count)
        {
            if (_positions == null || _positions.count < count)
            {
                _positions?.Dispose();
                _positions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.LockBufferForWrite, 
                    count, sizeof(WireBuffer.PositionData));
            }
            
            _positionsCount = count;
            
            positions->FillBuffer(_positions);
            // _mesh.Clear();
            // var vertices = new NativeArray<float3>(4, Allocator.Temp);
            // vertices[0] = new float3(1, 1, 0);
            // vertices[1] = new float3(5, 1, 0);
            // vertices[2] = new float3(5, 2, 0);
            // vertices[3] = new float3(5, 10, 0);
            
            // _mesh.SetVertices(positions->VertexArray());
            //
            // var indices = new NativeArray<int>(4, Allocator.Temp);
            // indices[0] = 0;
            // indices[1] = 1;
            // indices[2] = 2;
            // indices[3] = 3;
            // for (int i = 0; i < indices.Length; i++)
            // {
            //     indices[i] = i;
            // }
            
            // _mesh.SetIndices(indices, MeshTopology.Lines, 0);
            
            // UpdateParams();
        }
        
        // private void UpdateParams()
        // {
        //     if (!_renderParams.material)
        //     {
        //         _renderParams = new RenderParams(_material)
        //         {
        //             worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one),  // use tighter bounds
        //             matProps = new MaterialPropertyBlock()
        //         };
        //     }
        //     _renderParams.matProps.SetMatrix(ObjectToWorldProperty, Matrix4x4.identity);
        //     _renderParams.matProps.SetBuffer(PositionsProperty, _positions);
        //     _renderParams.matProps.SetBuffer(LineDataProperty, _lineData);
        // }

        internal void Render(UnityEngine.Rendering.CommandBuffer cmd)
        {
            //cmd.Buffer
            //cmd.DrawMesh(_mesh, Matrix4x4.identity, _material, 0, 0);
            cmd.SetGlobalBuffer(PositionsProperty, _positions);
            cmd.DrawProcedural(Matrix4x4.identity, _material, 0, MeshTopology.Lines, _positionsCount);
            // Debug.Log("Render: (Vertex: " + _positionsCount + ")");
            //Graphics.RenderPrimitives(_renderParams, MeshTopology.Lines, _positionsCount);
        }

        public void Dispose()
        {
            _positions?.Dispose();
            _lineData?.Dispose();
        }
    }
}