using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Plugins.CopyEngine.Fow_URP
{
    public class CE_FOW_RenderFeature_Pass : ScriptableRenderPass
    {
        private readonly ProfilingSampler mProfilingSampler = new("CE_FOW");

        private readonly CE_FOW_RenderFeature mFeatureRoot;

        /// <summary>
        /// FOW 用的 RenderTexutre
        /// </summary>
        private readonly int mRT_ID = Shader.PropertyToID("_CE_FOW_RT");

        private readonly List<ShaderTagId> _shaderTagIdList = new();

        public CE_FOW_RenderFeature_Pass(CE_FOW_RenderFeature featureRoot)
        {
            mFeatureRoot = featureRoot;

            _shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            _shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            _shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(mRT_ID,
                Mathf.CeilToInt(Screen.width * mFeatureRoot.FowRT_ScaleRate),
                Mathf.CeilToInt(Screen.height * mFeatureRoot.FowRT_ScaleRate), (int) DepthBits.Depth24);

            ConfigureTarget(new RenderTargetIdentifier(mRT_ID));
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, mProfilingSampler))
            {
                //清空RT,并提交运行
                cmd.ClearRenderTarget(true, mFeatureRoot, Color.black);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                //渲染所有 CE_FOW 层级的Mesh到RT
                var FOW_DrawSetting = CreateDrawingSettings(_shaderTagIdList, ref renderingData, SortingCriteria.CommonOpaque);
                FOW_DrawSetting.overrideMaterial = mFeatureRoot.MT_DrawMesh;
                FOW_DrawSetting.overrideMaterialPassIndex = 0;

                var FOW_FilterSetting = new FilteringSettings(RenderQueueRange.opaque) {layerMask = LayerMask.GetMask(CE_FOW_RenderFeature.CE_FOW_LAYER_NAME)};
                var FOW_RenderState = new RenderStateBlock(RenderStateMask.Nothing);
                context.DrawRenderers(renderingData.cullResults, ref FOW_DrawSetting, ref FOW_FilterSetting, ref FOW_RenderState);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd) { cmd.ReleaseTemporaryRT(mRT_ID); }
    }
}