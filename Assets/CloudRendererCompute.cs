using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudRendererCompute : ScriptableRendererFeature
{
    class CloudPass : ScriptableRenderPass
    {
        static readonly int _Mx_InvVP = Shader.PropertyToID("_Mx_InvVP");
        static readonly int _Mx_V = Shader.PropertyToID("_Mx_V");
        static readonly int _Origin = Shader.PropertyToID("_Origin");
        static readonly int _Clip = Shader.PropertyToID("_Clip");
        static readonly int _ScreenSize = Shader.PropertyToID("_ScreenSize");
        static readonly int _MainLightDir = Shader.PropertyToID("_MainLightDir");
        static readonly int _Volume = Shader.PropertyToID("_Volume");
        static readonly int _Result = Shader.PropertyToID("Result");
        static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        static readonly int _ZBufferParams = Shader.PropertyToID("_ZBufferParams");
        static readonly int _Beer = Shader.PropertyToID("_Beer");
        static readonly int _HG = Shader.PropertyToID("_HG");
        static readonly int _BlueNoise = Shader.PropertyToID("_BlueNoise");
        static readonly int _CloudLayers = Shader.PropertyToID("_CloudLayers");
        static readonly int RT = Shader.PropertyToID("_CloudComputeRT");

        public CloudRendererSettings settings;
        public RenderTargetIdentifier colorTarget;
        public RenderTargetIdentifier depthTarget;
        int width;
        int height;
        Material blendMat;

        Matrix4x4 GetInverseVP(Camera camera)
        {
            Matrix4x4 P = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            Matrix4x4 V = camera.worldToCameraMatrix;
            return (P*V).inverse;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            width = cameraTextureDescriptor.width / settings.downsample;
            height = cameraTextureDescriptor.height / settings.downsample;
            blendMat = new Material(settings.blendShader);
            cmd.GetTemporaryRT(RT, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear, 1, true);
            ConfigureTarget(RT);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            int k = settings.kernel;
            Camera camera = renderingData.cameraData.camera;
            CommandBuffer cmd = CommandBufferPool.Get();
            cmd.SetComputeMatrixParam(settings.shader, _Mx_InvVP, GetInverseVP(camera));
            cmd.SetComputeMatrixParam(settings.shader, _Mx_V, camera.worldToCameraMatrix);
            cmd.SetComputeVectorParam(settings.shader, _Origin, camera.transform.position);
            cmd.SetComputeFloatParams(settings.shader, _Clip, camera.nearClipPlane, 128f);
            cmd.SetComputeFloatParams(settings.shader, _ScreenSize, width, height);
            VisibleLight mainLight = renderingData.lightData.visibleLights[renderingData.lightData.mainLightIndex];
            cmd.SetComputeVectorParam(settings.shader, _MainLightDir, mainLight.light.transform.forward);
            cmd.SetComputeTextureParam(settings.shader, k, _Volume, settings.cloudVolume);
            cmd.SetComputeTextureParam(settings.shader, k, _Result, RT);
            cmd.SetComputeTextureParam(settings.shader, k, _CameraDepthTexture, _CameraDepthTexture);
            cmd.SetComputeVectorParam(settings.shader, _ZBufferParams, Shader.GetGlobalVector(_ZBufferParams));
            cmd.SetComputeFloatParam(settings.shader, _Beer, settings.alpha);
            cmd.SetComputeFloatParams(settings.shader, _HG, settings.g0, settings.g1, settings.hgLerp);
            cmd.SetComputeFloatParams(settings.shader, _CloudLayers, settings.bottomY, settings.thinY, settings.topY);
            int noiseId = Time.frameCount % settings.blueNoise.Length;
            cmd.SetComputeTextureParam(settings.shader, k, _BlueNoise, settings.blueNoise[noiseId]);
            int groupsX = Mathf.CeilToInt(width / 8f);
            int groupsY = Mathf.CeilToInt(height / 8f);
            cmd.DispatchCompute(settings.shader, k, groupsX, groupsY, 1);

            cmd.Blit(RT, colorTarget, blendMat);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(RT);
        }
    }

    CloudPass cloudPass;
    public CloudRendererSettings settings;

    public override void Create()
    {
        cloudPass = new CloudPass();
        cloudPass.settings = settings;
        cloudPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        cloudPass.colorTarget = renderer.cameraColorTarget;
        cloudPass.depthTarget = renderer.cameraDepth;
        renderer.EnqueuePass(cloudPass);
    }
}


