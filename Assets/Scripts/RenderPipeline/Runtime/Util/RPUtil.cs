using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RPUtil
{
    public static void BeginSample(ref ScriptableRenderContext context, CommandBuffer cmd)
    {
        cmd.BeginSample(cmd.name);
        ExecuteBuffer(ref context, cmd);
    }

    public static void ExecuteBuffer(ref ScriptableRenderContext context, CommandBuffer cmd)
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public static void EndSample(ref ScriptableRenderContext context, CommandBuffer cmd)
    {
        cmd.EndSample(cmd.name);
        ExecuteBuffer(ref context, cmd);
    }
}
