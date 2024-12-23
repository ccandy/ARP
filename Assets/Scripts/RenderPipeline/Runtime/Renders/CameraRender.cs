using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TerrainTools;

public class CameraRender
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    private CullingResults cullingResults;

    private const string bufferName = "Camera Buffer";
    private CommandBuffer _cameraBuffer = new CommandBuffer
    {
        name = bufferName
    };

    private static ShaderTagId unlitShaderTagId = new ShaderTagId("ARPUnlit");
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
        
    private DrawingSettings _drawingSettings;
    private SortingSettings _sortingSettings;
    private FilteringSettings _filteringSettings;

public void Render(ref ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;
        
        if (!Cull())
        {
            return;
        }
        
        Setup();
        DrawVisibleGeo();
        DrawUnSupportShaders();
        Submit();
    }

    private void Setup()
    {
        _context.SetupCameraProperties(_camera);
        _cameraBuffer.ClearRenderTarget(true, true, Color.clear);
        RPUtil.BeginSample(ref _context, _cameraBuffer);
        RPUtil.ExecuteBuffer(ref _context, _cameraBuffer);
    }

    private bool Cull()
    {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters scriptableCullingParameters))
        {
            cullingResults = _context.Cull(ref scriptableCullingParameters);
            return true;
        }

        return false;
    }

    private void DrawVisibleGeo()
    {
        DrawOpaqueGeo();
        _context.DrawSkybox(_camera);
        DrawTransGeo();
    }

    private void DrawOpaqueGeo()
    {
        _sortingSettings = new SortingSettings(_camera);
        _sortingSettings.criteria = SortingCriteria.CommonOpaque;
        _drawingSettings = new DrawingSettings(unlitShaderTagId,_sortingSettings);
        _filteringSettings = new FilteringSettings(RenderQueueRange.opaque);


        var sortSettings = new SortingSettings(_camera);
        sortSettings.criteria = SortingCriteria.CommonOpaque;
        _context.DrawRenderers(cullingResults, ref _drawingSettings, ref _filteringSettings);
    }

    private void DrawTransGeo()
    {
        _sortingSettings.criteria = SortingCriteria.CommonTransparent;
        _filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        _drawingSettings.sortingSettings = _sortingSettings;
        
        _context.DrawRenderers(cullingResults, ref _drawingSettings, ref _filteringSettings);
    }

    private void DrawUnSupportShaders()
    {
        if (errMaterial == null)
        {
            errMaterial = new Material(Shader.Find(errShaderName));
        }
        
        var drawSettings = new DrawingSettings(unsupportShaderTagId[0], new SortingSettings(_camera));
        for (int n = 1; n < unsupportShaderTagId.Length; n++)
        {
            drawSettings.SetShaderPassName(n, unsupportShaderTagId[n]);
        }

        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
        drawSettings.overrideMaterial = errMaterial;
        
        _context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings);
    }
    

    private void Submit()
    {
        RPUtil.EndSample(ref _context, _cameraBuffer);
        _context.Submit();
    }

}
