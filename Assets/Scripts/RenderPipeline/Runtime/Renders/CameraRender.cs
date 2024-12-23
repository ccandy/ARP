using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRender
{
    private ScriptableRenderContext _context;
    private Camera _camera;


    private const string bufferName = "Camera Buffer";
    private CommandBuffer _cameraBuffer = new CommandBuffer
    {
        name = bufferName
    };

public void Render(ref ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;
        Setup();
        DrawVisibleGeo();
        Submit();
    }

    private void Setup()
    {
        RPUtil.BeginSample(ref _context, _cameraBuffer);
        _context.SetupCameraProperties(_camera);
    }

    private void Cull()
    {
        
    }

    private void DrawVisibleGeo()
    {
        _context.DrawSkybox(_camera);
    }

    private void DrawOpaqueGeo()
    {

    }

    private void DrawTransGeo()
    {
        
    }


    private void Submit()
    {
        RPUtil.EndSample(ref _context, _cameraBuffer);
        _context.Submit();
    }

}
