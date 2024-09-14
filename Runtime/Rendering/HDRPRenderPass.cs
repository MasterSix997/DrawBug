#if PACKAGE_HIGH_DEFINITION_RP
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

namespace Drawbug {
	class DrawbugHDRPCustomPass : CustomPass 
	{
		protected override void Setup (ScriptableRenderContext renderContext, CommandBuffer cmd) { }

#if PACKAGE_HIGH_DEFINITION_RP_9_0_OR_NEWER
		protected override void Execute (CustomPassContext context) 
		{
			DrawbugManager.ExecuteCustomPass(context.cmd, context.hdCamera.camera);
		}
#else
		protected override void Execute (ScriptableRenderContext context, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult) 
		{
			DrawbugManager.ExecuteCustomPass(cmd, camera.camera);
		}
#endif

		protected override void Cleanup () { }
	}
}
#endif