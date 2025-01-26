using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Drawbug.PhysicsExtension;
using UnityEditor;
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
        internal enum RenderPipelineOption
        {
            BuiltIn,
            Custom,
            URP,
            HDRP
        }
        
        private static DrawbugManager _instance;

        private Draw _draw;
        private CommandBuffer _cmd;
        private bool _isEnabled;
        [SerializeField] private DrawbugSettings _settings;
        
        private RenderPipelineOption _currentRenderPipeline = RenderPipelineOption.BuiltIn;
#if PACKAGE_UNIVERSAL_RP
        private DrawbugRenderPassFeature _renderPassFeature;
#endif

        internal static DrawbugSettings Settings => _instance._settings;
        
        public static void Initialize()
        {
            if(_instance)
                return;

            var gameObj = new GameObject(string.Concat("DrawbugManager (", UnityEngine.Random.Range(0, 10000).ToString("0000"), ")"))
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
                // We cannot destroy the object while it is being enabled, so we need to delay it a bit
#if UNITY_EDITOR
                
                EditorApplication.update += DelayedDestroy;
#endif
                return;
            }
            
            _isEnabled = true;
            _settings ??= DrawbugSettings.CreateDefaultSettings();
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
            if (state is not (PlayModeStateChange.ExitingEditMode or PlayModeStateChange.EnteredEditMode)) return;
            if (!_instance) return;
            // DestroyImmediate(_instance.gameObject);
            EditorApplication.update += DelayedDestroy;
            _instance = null;
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
        private struct AfterDrawGizmos { }

        private void InsertToPlayerLoop()
        {
            PlayerLoopInserter.InsertSystem(typeof(BeginFixedUpdate), typeof(UnityEngine.PlayerLoop.FixedUpdate), InsertType.First, ClearFixedCommands);
            PlayerLoopInserter.InsertSystem(typeof(ClearDrawbug), typeof(UnityEngine.PlayerLoop.EarlyUpdate), InsertType.Before, ClearFrameData);
            PlayerLoopInserter.InsertSystem(typeof(BuildDrawbugCommands), typeof(UnityEngine.PlayerLoop.PostLateUpdate), InsertType.Before, BuildCommandsUpdate);
            if (Application.isPlaying)
            {
            }
            else
            {
                EditorApplication.update += EditorUpdate;
            }
        }

        private void ClearFixedCommands()
        {
            _draw.ClearFixed();
        }

        private void ClearFrameData()
        {
            _draw.Clear();
            _draw.UpdateTimedBuffers(Time.deltaTime);
        }
        
        void OnDrawGizmos()
        {
            // Your gizmo drawing thing goes here if required...

#if UNITY_EDITOR
            // Ensure continuous Update calls.
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
#endif
        }

        private static void EditorUpdate()
        {
            // _instance.BuildCommandsUpdate();
        }

        private void RemoveFromPlayerLoop()
        {
            PlayerLoopInserter.RemoveRunner(typeof(BeginFixedUpdate));
            PlayerLoopInserter.RemoveRunner(typeof(ClearDrawbug));
            PlayerLoopInserter.RemoveRunner(typeof(BuildDrawbugCommands));
            if (Application.isPlaying)
            {
            }
            else
            {
                EditorApplication.update -= EditorUpdate;
            }
        }

        private bool _hasPendingData;

        private void BuildCommandsUpdate()
        {
            if (_hasPendingData)
                return;
         
            _draw.BuildData();
            _hasPendingData = true;
        }

        private void RenderData(CommandBuffer cmd, bool clearBeforeRender = false)
        {
            if (_hasPendingData)
            {
                _hasPendingData = false;
                _draw.GetDataResults();
            }
            
            if (clearBeforeRender)
                _draw.Clear();
            _draw.Render(cmd);
        }
        
        private void RenderData(RasterCommandBuffer cmd, bool clearBeforeRender = false)
        {
            if (_hasPendingData)
            {
                _hasPendingData = false;
                _draw.GetDataResults();
            }
            
            if (clearBeforeRender)
                _draw.Clear();
            _draw.Render(cmd);
        }

        // private void RenderCustomPass(CommandBuffer cmd, Camera camera)
        // {
        //     if (_hasPendingData)
        //     {
        //         _hasPendingData = false;
        //         _draw.GetDataResults();
        //     }
        //     
        //     _draw.Render(cmd);
        // }
        
        internal static void ExecuteCustomRenderPass(ScriptableRenderContext context, Camera camera)
        {
            if(!_instance || !_instance._isEnabled)
                return;
            
            _instance.RenderData(_instance._cmd, true);
            context.ExecuteCommandBuffer(_instance._cmd);
        }
        
#if PACKAGE_UNIVERSAL_RP_17_0_0_OR_NEWER
        // private void RenderGraphPass(RasterCommandBuffer cmd, Camera camera)
        // {
        //     RenderData(cmd);
        // }

        internal static void ExecuteCustomRenderGraphPass(RasterCommandBuffer cmd, Camera camera)
        {
            if(!_instance || !_instance._isEnabled)
                return;
            
            // _instance.RenderGraphPass(cmd, camera);
            _instance.RenderData(cmd);
        }
#endif

#if PACKAGE_HIGH_DEFINITION_RP
        internal static void ExecuteCustomPass(CommandBuffer cmd, Camera camera)
        {
            if(!_instance || !_instance._isEnabled)
                return;
            
            // _instance.RenderCustomPass(cmd, camera);
            _instance.RenderData(cmd);
        }
#endif
    }
}
