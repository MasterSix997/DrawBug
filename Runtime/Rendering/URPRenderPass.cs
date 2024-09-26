#if PACKAGE_UNIVERSAL_RP
using UnityEngine;
using UnityEngine.Rendering;
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif
using UnityEngine.Rendering.Universal;

namespace Drawbug 
{
	public class DrawbugRenderPassFeature : ScriptableRendererFeature 
	{
		public class DrawbugURPRenderPass : ScriptableRenderPass 
		{
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
			[System.Obsolete]
#endif
			public override void Configure (CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) { }

#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
			[System.Obsolete]
#endif
			public override void Execute (ScriptableRenderContext context, ref RenderingData renderingData) 
			{
				DrawbugManager.ExecuteCustomRenderPass(context, renderingData.cameraData.camera);
			}
			
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
			private class PassData 
			{
				public Camera camera;
			}

			public override void RecordRenderGraph (RenderGraph renderGraph, ContextContainer frameData) 
			{
				var cameraData = frameData.Get<UniversalCameraData>();
				var resourceData = frameData.Get<UniversalResourceData>();

				using var builder = renderGraph.AddRasterRenderPass("Drawbug", out PassData passData, profilingSampler);
				
				builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
				builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
				passData.camera = cameraData.camera;
				
				builder.SetRenderFunc((PassData data, RasterGraphContext context) => {
						DrawbugManager.ExecuteCustomRenderGraphPass(context.cmd, data.camera);
					}
				);
			}
#endif

			public override void FrameCleanup (CommandBuffer cmd) { }
		}

		private DrawbugURPRenderPass _scriptablePass;

		public override void Create () 
		{
			_scriptablePass = new DrawbugURPRenderPass
			{
				renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
			};
		}

		public override void AddRenderPasses (ScriptableRenderer renderer, ref RenderingData renderingData) 
		{
			AddRenderPasses(renderer);
		}
		
		public void AddRenderPasses (ScriptableRenderer renderer) {
			renderer.EnqueuePass(_scriptablePass);
		}
	}
}
#endif