using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Drawbug
{
    internal class DrawbugManager : MonoBehaviour
    {
        private static DrawbugManager _instance;

        private Draw _draw;
        
        private bool _isEnabled;

        private UnityEngine.Rendering.CommandBuffer _cmd;
        
        public static void Initialize()
        {
            if(_instance)
                return;

            var gameObj = new GameObject(string.Concat("DrawbugManager (", Random.Range(0, 10000).ToString("0000"), ")"))
            {
                //hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInHierarchy | HideFlags.HideInInspector
            };
            Debug.Log(gameObj.name + " Initilized");
            _instance = gameObj.AddComponent<DrawbugManager>();
            // if (Application.isPlaying)
            // {
            //     print("Dont Destroy");
            //     DontDestroyOnLoad(gameObj);
            // }
        }
        

        private void OnEnable()
        {
            if (_instance == null)
                _instance = this;

            if (_instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }
            
            _isEnabled = true;
            _draw = new Draw();
            InsertToPlayerLoop();
            // RenderPipelineManager.endContextRendering += ContextRendering;
            _cmd = new UnityEngine.Rendering.CommandBuffer()
            {
                name = "Drawbug"
            };
        }

        private void OnDisable()
        {
            if (!_isEnabled)
                return;

            _isEnabled = false;
            _instance = null;
            _draw.Dispose();
            RemoveFromPlayerLoop();
            // RenderPipelineManager.endContextRendering -= ContextRendering;
            _cmd.Dispose();
        }
        
        private struct BuildDrawbugCommands
        {
            
        }
        private struct RenderDrawbugCommands
        {
            
        }

        private void OnRenderObject()
        {
            // if (Camera.current != Camera.main)
            // {
            //     return;
            // }
            RenderData();
            Graphics.ExecuteCommandBuffer(_cmd);
        }

        private void InsertToPlayerLoop()
        {
            PlayerLoopInserter.InsertSystem(typeof(BuildDrawbugCommands), typeof(UnityEngine.PlayerLoop.PostLateUpdate), InsertType.Before, BuildCommandsUpdate);
            // PlayerLoopInserter.InsertSystem(typeof(RenderDrawbugCommands), typeof(UnityEngine.PlayerLoop.PostLateUpdate), InsertType.After, RenderUpdate);
        }

        private void RemoveFromPlayerLoop()
        {
            PlayerLoopInserter.RemoveRunner(typeof(BuildDrawbugCommands));
            // PlayerLoopInserter.RemoveRunner(typeof(RenderDrawbugCommands));
        }

        private bool _hasPendingData;
        
        private void BuildCommandsUpdate()
        {
            // Debug.Log("========================");
            // Debug.Log("Build Commands");
            // Debug.Log("==============================");
            if (_hasPendingData)
                return;
         
            _draw.BuildData();
            _hasPendingData = true;
            // Debug.Log("Update");
            // RenderUpdate();
        }
        
        private void ContextRendering(ScriptableRenderContext arg1, List<Camera> arg2)
        {
            RenderData();
            arg1.ExecuteCommandBuffer(_cmd);
            arg1.Submit();
        }

        private void RenderData()
        {
            if (_hasPendingData)
            {
                _hasPendingData = false;
                
                _cmd.Clear();
                _draw.GetDataResults();
                _draw.Render(_cmd);
                _draw.Clear();
            }
        }

        // private void RenderUpdate()
        // {
        //     if (!_hasPendingData)
        //     {
        //         Debug.Log("Not pending data");
        //         _draw.Clear();
        //         Debug.Log("----------------------");
        //         return;
        //     }
        //     
        //     _draw.Render();
        //     _draw.Clear();
        //     _hasPendingData = false;
        //     Debug.Log("Render");
        //     Debug.Log("----------------------");
        // }

        // private void OnPostRender()
        // {
        //     RenderUpdate();
        // }

        // private void OnRenderObject()
        // {
        //     Debug.Log("Render Object");
        // }
        // private void Update()
        // {
        //     _firstUpdate = true;
        //     _draw.BuildData();
        // }
        //
        // private void LateUpdate()
        // {
        //     if (!_firstUpdate)
        //     {
        //         _draw.Clear();
        //         return;
        //     }
        //     _draw.Render();
        //     _draw.Clear();
        // }
    }
}
