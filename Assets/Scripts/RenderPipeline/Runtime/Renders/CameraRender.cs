using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRender
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    private CullingResults cullingResults;

    private const string bufferName = "Camera Buffer";
    private CommandBuffer _cameraBuffer = new CommandBuffer();
    private static ShaderTagId unlitShaderTagId = new ShaderTagId("ARPUnlit");
        
    private DrawingSettings _drawingSettings;
    private SortingSettings _sortingSettings;
    private FilteringSettings _filteringSettings;

    private bool _enableDynamicBatch;

    private Lighting _light = new Lighting();

    public void Render(ref ScriptableRenderContext context, Camera camera, bool enableDynamicBatch)
    {
        _context = context;
        _camera = camera;
        _cameraBuffer.name = bufferName + " " + _camera.name;
        _enableDynamicBatch = enableDynamicBatch;
        PrepareForSceneWindow();
        if (!Cull())
        {
            return;
        }
        
        Setup();
        _light.Setup(ref _context, ref cullingResults);
        DrawVisibleGeo();
        DrawUnSupportShaders();
        DrawGizmos();
        Submit();
    }

    private void Setup()
    {
        _context.SetupCameraProperties(_camera);
        CameraClearFlags flags = _camera.clearFlags;
        _cameraBuffer.ClearRenderTarget(flags<= CameraClearFlags.Depth, flags == CameraClearFlags.Color, Color.clear);
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
        _drawingSettings.enableDynamicBatching = _enableDynamicBatch;
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
    
    
    private void Submit()
    {
        RPUtil.EndSample(ref _context, _cameraBuffer);
        _context.Submit();
    }

}
