using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Drawbug.PhysicsExtension;
using Drawbug.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if PACKAGE_HIGH_DEFINITION_RP
using UnityEngine.Rendering.HighDefinition;
#endif
#if PACKAGE_UNIVERSAL_RP
using UnityEngine.Rendering.Universal;
#endif

namespace Drawbug
{
    [ExecuteInEditMode]
    internal class DrawbugManager : MonoBehaviour
    {
        private enum RenderPipelineOption
        {
            BuiltIn,
            Custom,
            URP,
            HDRP
        }

        private enum ManagerMode
        {
            PlayMode,
            EditMode
        }
        
        private static DrawbugManager _instance;

        private Draw _draw;
        private CommandBuffer _cmd;
        private bool _isEnabled;
        private DrawbugSettings _settings;
        private bool _hasPendingData;

        private ManagerMode _mode = ManagerMode.PlayMode;
        
        private RenderPipelineOption _currentRenderPipeline = RenderPipelineOption.BuiltIn;
#if PACKAGE_UNIVERSAL_RP
        private DrawbugRenderPassFeature _renderPassFeature;
#endif

        internal static DrawbugSettings Settings => _instance._settings;
        
        public static void Initialize()
        {
            if(_instance)
                return;

            var gameObj = new GameObject(string.Concat("DrawbugManager (", Random.Range(0, 10000).ToString("0000"), ")"))
            {
                hideFlags = HideFlags.NotEditable | HideFlags.DontSave// | HideFlags.HideInHierarchy | HideFlags.HideInInspector
            };
            Debug.Log(gameObj.name + " Initilized");
            _instance = gameObj.AddComponent<DrawbugManager>();

            // if (Application.isPlaying)
            //     DontDestroyOnLoad(gameObj);
        }

        private void UpdateCurrentRenderPipeline()
        {
            var pipelineType = RenderPipelineManager.currentPipeline != null ? RenderPipelineManager.currentPipeline.GetType() : null;
            
#if PACKAGE_HIGH_DEFINITION_RP
            if (pipelineType == typeof(HDRenderPipeline)) {
                if (_currentRenderPipeline != RenderPipelineOption.HDRP) {
                    _currentRenderPipeline = RenderPipelineOption.HDRP;
                    if (!_instance.gameObject.TryGetComponent(out CustomPassVolume volume)) {
                        volume = _instance.gameObject.AddComponent<CustomPassVolume>();
                        volume.isGlobal = true;
                        volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
                        volume.customPasses.Add(new DrawbugHDRPCustomPass());
                    }

                    var asset = GraphicsSettings.defaultRenderPipeline as HDRenderPipelineAsset;
                    if (asset != null) {
                        if (!asset.currentPlatformRenderPipelineSettings.supportCustomPass) {
                            Debug.LogWarning("Drawbug: Custom pass support is disabled in the current render pipeline. Please enable it in the HDRenderPipelineAsset.", asset);
                        }
                    }
                }
                return;
            }
#endif
#if PACKAGE_UNIVERSAL_RP
            if (pipelineType == typeof(UniversalRenderPipeline)) {
                _currentRenderPipeline = RenderPipelineOption.URP;
                return;
            }
#endif
            _currentRenderPipeline = pipelineType != null ? RenderPipelineOption.Custom : RenderPipelineOption.BuiltIn;
        }
        
        void DelayedDestroy () 
        {
            EditorApplication.update -= DelayedDestroy;
            // Check if the object still exists (it might have been destroyed in some other way already).
            if (gameObject) DestroyImmediate(gameObject);
        }
        
        private void OnEnable()
        {
            if (!_instance)
                _instance = this;
            
            if (_instance != this)
            {
#if UNITY_EDITOR
                // Unity don't allow to destroy object when is executing OnEnable
                EditorApplication.update += DelayedDestroy;
#endif
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
                _mode = ManagerMode.EditMode;
#endif
            _isEnabled = true;
            _settings = DrawbugSettings.LoadSettings();
            _cmd = new CommandBuffer { name = "Drawbug" };
            _draw = new Draw();

            DrawPhysics.Style = _settings;
            DrawPhysics2D.Style = _settings;
            
            InsertToPlayerLoop();
            
            Camera.onPostRender += PostRender;
#if UNITY_2023_3_OR_NEWER
            RenderPipelineManager.beginContextRendering += BeginContextRendering;
#else
			RenderPipelineManager.beginFrameRendering += BeginFrameRendering;
#endif
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            RenderPipelineManager.endCameraRendering += EndCameraRendering;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        private void OnDisable()
        {
            if (!_isEnabled)
                return;
            Debug.Log(gameObject.name + " Disabled");
            
            _isEnabled = false;
            _instance = null;
            RemoveFromPlayerLoop();
            _draw.Dispose();
            _cmd.Dispose();
            _settings = null;
            
            Camera.onPostRender -= PostRender;
#if UNITY_2023_3_OR_NEWER
            RenderPipelineManager.beginContextRendering -= BeginContextRendering;
#else
			RenderPipelineManager.beginFrameRendering -= BeginFrameRendering;
#endif
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= EndCameraRendering;
            
#if PACKAGE_UNIVERSAL_RP
			if (_renderPassFeature) 
            {
				DestroyImmediate(_renderPassFeature);
				_renderPassFeature = null;
			}
#endif

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state is PlayModeStateChange.ExitingEditMode or PlayModeStateChange.ExitingPlayMode or PlayModeStateChange.EnteredEditMode)
            {
                if (!_instance) return;
                DestroyImmediate(_instance.gameObject);
                // EditorApplication.update += DelayedDestroy;
                _instance = null;
            }
        }

        void BeginContextRendering (ScriptableRenderContext context, List<Camera> cameras) 
        {
            UpdateCurrentRenderPipeline();
        }

        void BeginFrameRendering (ScriptableRenderContext context, Camera[] cameras) 
        {
            UpdateCurrentRenderPipeline();
        }

        void BeginCameraRendering (ScriptableRenderContext context, Camera camera) 
        {
#if PACKAGE_UNIVERSAL_RP
			if (_currentRenderPipeline == RenderPipelineOption.URP) 
            {
				var data = camera.GetUniversalAdditionalCameraData();
				if (data != null) {
					var renderer = data.scriptableRenderer;
					if (_renderPassFeature == null) {
						_renderPassFeature = ScriptableObject.CreateInstance<DrawbugRenderPassFeature>();
					}
					_renderPassFeature.AddRenderPasses(renderer);
				}
			}
#endif
        }
        
        private void EndCameraRendering (ScriptableRenderContext context, Camera camera) 
        {
            if (_currentRenderPipeline == RenderPipelineOption.Custom) 
            {
                ExecuteCustomRenderPass(context, camera);
            }
        }
        
        void PostRender (Camera camera) 
        {
            RenderData(_cmd, true);
            Graphics.ExecuteCommandBuffer(_cmd);
        }
        
        private struct BeginFixedUpdate { }
        private struct ClearDrawbug { }
        private struct BuildDrawbugCommands { }

        private void InsertToPlayerLoop()
        {
            PlayerLoopInserter.InsertSystem(typeof(BeginFixedUpdate), typeof(UnityEngine.PlayerLoop.FixedUpdate), InsertType.First, ClearFixedCommands);
            PlayerLoopInserter.InsertSystem(typeof(ClearDrawbug), typeof(UnityEngine.PlayerLoop.EarlyUpdate), InsertType.Before, ClearFrameData);
            PlayerLoopInserter.InsertSystem(typeof(BuildDrawbugCommands), typeof(UnityEngine.PlayerLoop.PostLateUpdate), InsertType.Before, BuildCommandsUpdate);
        }


        private void RemoveFromPlayerLoop()
        {
            PlayerLoopInserter.RemoveRunner(typeof(BeginFixedUpdate));
            PlayerLoopInserter.RemoveRunner(typeof(ClearDrawbug));
            PlayerLoopInserter.RemoveRunner(typeof(BuildDrawbugCommands));
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (_mode != ManagerMode.EditMode) return;
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
#endif
        
        private void ClearFixedCommands()
        {
            _draw.ClearFixed();
        }

        private void ClearFrameData()
        {
            _draw.Clear();
            _draw.UpdateTimedBuffers(Time.deltaTime);
        }

        private void BuildCommandsUpdate()
        {
            if (_hasPendingData)
                return;
         
            _draw.BuildData();
            _hasPendingData = true;
        }
        
        private void RenderData(CommandBuffer cmd, bool clearBeforeRender = false) => RenderData(new CommandBufferWrapper(cmd), clearBeforeRender);
        
        private void RenderData(RasterCommandBuffer cmd, bool clearBeforeRender = false) => RenderData(new CommandBufferWrapper(cmd), clearBeforeRender);

        private void RenderData(CommandBufferWrapper cmd, bool clearBeforeRender = false)
        {
#if UNITY_EDITOR
            if (_mode == ManagerMode.PlayMode && !Application.isPlaying)
                return;
#endif
            if (_hasPendingData)
            {
                _hasPendingData = false;
                _draw.GetDataResults();
            }
            
            if (clearBeforeRender)
                cmd.Clear();
            
            _draw.Render(cmd);
        }
        
        internal static void ExecuteCustomRenderPass(ScriptableRenderContext context, Camera camera)
        {
            if(!_instance || !_instance._isEnabled)
                return;
            
            _instance.RenderData(_instance._cmd, true);
            context.ExecuteCommandBuffer(_instance._cmd);
        }
        
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER

        internal static void ExecuteCustomRenderGraphPass(RasterCommandBuffer cmd, Camera camera)
        {
            if(!_instance || !_instance._isEnabled)
                return;
            
            _instance.RenderData(cmd);
        }
#endif

#if PACKAGE_HIGH_DEFINITION_RP
        internal static void ExecuteCustomPass(CommandBuffer cmd, Camera camera)
        {
            if(!_instance || !_instance._isEnabled)
                return;
            
            _instance.RenderData(cmd);
        }
#endif
    }
}
