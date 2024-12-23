using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

partial class CameraRender
{
    partial void DrawGizmos();
    partial void DrawUnSupportShaders();

    partial void PrepareForSceneWindow();
    
    private static ShaderTagId[] unsupportShaderTagId =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("vertexLM")
    };

    private static Material errMaterial;
    private static string errShaderName = "Hidden/InternalErrorShader";
    
    #if UNITY_EDITOR
        partial void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos())
            {
                _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
                _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
            }
        }

        partial void PrepareForSceneWindow()
        {
            if (_camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
            }
        }
        
        partial void DrawUnSupportShaders()
        {
            if (errMaterial == null)
            {
                errMaterial = new Material(Shader.Find(errShaderName));
            }
        
            var drawSettings = new DrawingSettings(unsupportShaderTagId[0], new SortingSettings(_camera));
            drawSettings.enableDynamicBatching = _enableDynamicBatch;
            for (int n = 1; n < unsupportShaderTagId.Length; n++)
            {
                drawSettings.SetShaderPassName(n, unsupportShaderTagId[n]);
            }

            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
            drawSettings.overrideMaterial = errMaterial;
        
            _context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings);
        }
    #endif
    
}
