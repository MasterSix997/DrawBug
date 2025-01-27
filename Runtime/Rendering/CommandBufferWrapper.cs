using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Drawbug.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CommandBufferWrapper
    {
        private readonly CommandBuffer _cmd;
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
        private readonly RasterCommandBuffer _rasterCmd;
#endif
        private readonly bool _isRaster;

        public CommandBufferWrapper(CommandBuffer cmd)
        {
            _cmd = cmd;
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
            _rasterCmd = null;
#endif
            _isRaster = false;
        }
        
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
        public CommandBufferWrapper(RasterCommandBuffer rasterCmd)
        {
            _cmd = null;
            _rasterCmd = rasterCmd;
            _isRaster = true;
        }
#endif

        public void DrawProcedural(Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int vertexCount)
        {
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
            if (_isRaster)
            {
                _rasterCmd.DrawProcedural(matrix, material, shaderPass, topology, vertexCount);
                return;
            }
#endif
            _cmd.DrawProcedural(matrix, material, shaderPass, topology, vertexCount);
        }

        public void DrawProcedural(GraphicsBuffer indexBuffer, Matrix4x4 matrix, Material material, int shaderPass, MeshTopology topology, int indexCount)
        {
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
            if (_isRaster)
            {
                _rasterCmd.DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount);
                return;
            }
#endif
            _cmd.DrawProcedural(indexBuffer, matrix, material, shaderPass, topology, indexCount);
        }

        public void Clear()
        {
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
            if (_isRaster)
            {
                Debug.LogError("Clear() is not supported for RasterCommandBuffer.");
                return;
            }
#endif
            _cmd.Clear();
        }
    }
}