using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        static readonly string kTag = "Sandstorm (FullScreen)";
        SandstormSettings settings;
        Material mat;
        RTHandle tempRT;
        int _Intensity = Shader.PropertyToID("_Intensity");
        int _Tint = Shader.PropertyToID("_Tint");
        int _NoiseScale = Shader.PropertyToID("_NoiseScale");
        int _Distort = Shader.PropertyToID("_Distort");
        int _Grain = Shader.PropertyToID("_Grain");
        int _Streak = Shader.PropertyToID("_Streak");
        int _Vignette = Shader.PropertyToID("_Vignette");
        int _WindDir = Shader.PropertyToID("_WindDir");
        int _Speed = Shader.PropertyToID("_Speed");

        public SandstormPass(SandstormSettings s)
        {
            settings = s;
            profilingSampler = new ProfilingSampler(kTag);
        }

        public void Setup(Material m) => mat = m;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempRT, desc, name: "_SandstormTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (mat == null) return;

            var cmd = CommandBufferPool.Get(kTag);

            
            mat.SetFloat(_Intensity, settings.intensity);
            mat.SetColor(_Tint, settings.tint);
            mat.SetFloat(_NoiseScale, settings.noiseScale);
            mat.SetFloat(_Distort, settings.distort);
            mat.SetFloat(_Grain, settings.grain);
            mat.SetFloat(_Streak, settings.streak);
            mat.SetFloat(_Vignette, settings.vignette);
            mat.SetVector(_WindDir, settings.windDir);
            mat.SetFloat(_Speed, settings.speed);

            var src = renderingData.cameraData.renderer.cameraColorTargetHandle;
            using (new ProfilingScope(cmd, profilingSampler))
            {
                Blitter.BlitCameraTexture(cmd, src, tempRT, mat, 0);
                Blitter.BlitCameraTexture(cmd, tempRT, src);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

 
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }
        public override void OnFinishCameraStackRendering(CommandBuffer cmd) { }
    }

    public SandstormSettings settings = new SandstormSettings();
    SandstormPass pass;

    public override void Create()
    {
        pass = new SandstormPass(settings);
        pass.renderPassEvent = settings.injectionPoint;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;
        pass.Setup(settings.material);
        renderer.EnqueuePass(pass);
    }
}