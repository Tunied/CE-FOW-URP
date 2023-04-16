using Plugins.CopyEngine.Fow_URP;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CE_FOW_RenderFeature : ScriptableRendererFeature
{
    public const string CE_FOW_LAYER_NAME = "CE_FOW";

    [Range(0, 1)]
    public float FowRT_ScaleRate = 0.5f;

    public Material MT_DrawMesh;

    private CE_FOW_RenderFeature_Pass mPass;

    public override void Create() { mPass = new CE_FOW_RenderFeature_Pass(this); }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) { renderer.EnqueuePass(mPass); }
}