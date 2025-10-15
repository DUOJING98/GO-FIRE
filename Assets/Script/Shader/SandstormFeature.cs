using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SandstormFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class SandstormSettings
    {
        public Material material;                         
        public RenderPassEvent injectionPoint = RenderPassEvent.AfterRendering;
        [Range(0, 2f)] public float intensity = 0.8f;
        [ColorUsage(false, true)] public Color tint = new Color(0.95f, 0.85f, 0.6f, 1f);
        [Range(0.1f, 4f)] public float noiseScale = 1.2f;
        [Range(0f, 2f)] public float distort = 0.35f;
        [Range(0f, 1f)] public float grain = 0.25f;
        [Range(0f, 2f)] public float streak = 0.7f;
        [Range(0f, 1f)] public float vignette = 0.35f;
        public Vector2 windDir = new Vector2(1f, 0.15f);
        public float speed = 0.6f;
    }

    class SandstormPass : ScriptableRenderPass
    {
        const string kTag = "Sandstorm (FullScreen)";
        readonly SandstormSettings settings;
        readonly new ProfilingSampler profilingSampler;
        Material mat;
        RTHandle tempRT;

       
        readonly int _Intensity = Shader.PropertyToID("_Intensity");
        readonly int _Tint = Shader.PropertyToID("_Tint");
        readonly int _NoiseScale = Shader.PropertyToID("_NoiseScale");
        readonly int _Distort = Shader.PropertyToID("_Distort");
        readonly int _Grain = Shader.PropertyToID("_Grain");
        readonly int _Streak = Shader.PropertyToID("_Streak");
        readonly int _Vignette = Shader.PropertyToID("_Vignette");
        readonly int _WindDir = Shader.PropertyToID("_WindDir");
        readonly int _Speed = Shader.PropertyToID("_Speed");

        public SandstormPass(SandstormSettings s)
        {
            settings = s;
            profilingSampler = new ProfilingSampler(kTag);
        }

        public void Setup(Material m) => mat = m;

        [System.Obsolete]
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        [System.Obsolete]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            RenderingUtils.ReAllocateIfNeeded(ref tempRT, desc, name: "_SandstormTemp");
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (mat == null) return;

            var src = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if (src == null || src.rt == null)

            {
                
                // Debug.Log("[Sandstorm] Skip: invalid color target (scene switching)");
                return;
            }

            var cmd = CommandBufferPool.Get(kTag);

            float t = Time.time * 0.7f; // 控制风变化速度
            float windNoise = Mathf.PerlinNoise(t, 1.23f); // 得到 0~1 平滑随机数

            // 强度：在 0.8~1.3 倍之间波动
            float intensity = settings.intensity * Mathf.Lerp(0.8f, 1.3f, windNoise);

            // 扭曲：与风同步起伏
            float distort = settings.distort * Mathf.Lerp(0.9f, 1.2f, windNoise);

            // 颗粒：轻微颤动，频率可以略快一点
            float grain = settings.grain * Mathf.Lerp(0.9f, 1.1f, Mathf.PerlinNoise(t * 1.8f, 2.56f));

            // 下发到 shader
            mat.SetFloat(_Intensity, intensity);
            mat.SetFloat(_Distort, distort);
            mat.SetFloat(_Grain, grain);
            //mat.SetFloat(_Intensity, settings.intensity);
            mat.SetColor(_Tint, settings.tint);
            mat.SetFloat(_NoiseScale, settings.noiseScale);
            //mat.SetFloat(_Distort, settings.distort);
            //mat.SetFloat(_Grain, settings.grain);
            mat.SetFloat(_Streak, settings.streak);
            mat.SetFloat(_Vignette, settings.vignette);
            mat.SetVector(_WindDir, settings.windDir);
            mat.SetFloat(_Speed, settings.speed);

            using (new ProfilingScope(cmd, profilingSampler))
            {
                Blitter.BlitCameraTexture(cmd, src, tempRT, mat, 0);
                Blitter.BlitCameraTexture(cmd, tempRT, src);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }
    }

    public SandstormSettings settings = new SandstormSettings();
    SandstormPass pass;
    int skipFrames = 0;

    public override void Create()
    {
        pass = new SandstormPass(settings);
        pass.renderPassEvent = settings.injectionPoint;

       
        SceneManager.activeSceneChanged += (_, __) => skipFrames = 2;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;

        if (skipFrames > 0)
        {
            skipFrames--;
            return;
        }

        var camData = renderingData.cameraData;
        if (camData.cameraType != CameraType.Game || camData.renderType != CameraRenderType.Base)
            return;

        pass.Setup(settings.material);
        renderer.EnqueuePass(pass);
    }
}
