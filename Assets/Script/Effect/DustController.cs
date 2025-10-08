using UnityEngine;

public class DustController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem ps;

    [Header("Emission (发射率波动)")]
    public float minRate = 20f;
    public float maxRate = 160f;

    [Header("Base Wind (基础风)")]
    // 基础持续风：影响已生成粒子的受力（更真实）
    public float baseForceX = 0.8f;     // 平时风力
    public float maxForceX = 3.5f;      // 阵风顶峰

    [Header("Noise 时序（越小越慢）")]
    public float rateNoiseSpeed = 0.15f; // 发射率起伏速度
    public float forceNoiseSpeed = 0.20f; // 风力起伏速度

    [Header("Gust 阵风（可选）")]
    public bool enableGust = true;
    public float gustChance = 0.08f;    // 每秒触发概率
    public float gustDuration = 1.2f;   // 阵风持续秒数
    [Range(0f, 1f)] public float gustBlend = 0.6f; // 阵风强度混合

    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.ForceOverLifetimeModule force;
    private float seedRate, seedForce;
    private float gustTimer = 0f;
    private float gustTarget = 0f;

    void Awake()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();
        emission = ps.emission;
        force = ps.forceOverLifetime;
        seedRate = Random.value * 1000f;
        seedForce = Random.value * 1000f;
    }

    void Update()
    {
        float t = Time.time;

        // --- 平滑随机（Perlin Noise） ---
        // 0..1
        float r01 = Mathf.PerlinNoise(seedRate, t * rateNoiseSpeed);
        float f01 = Mathf.PerlinNoise(seedForce, t * forceNoiseSpeed);

        // 发射率平滑起伏
        float targetRate = Mathf.Lerp(minRate, maxRate, r01);
        var rate = emission.rateOverTime;
        float currentRate = rate.constant;
        float newRate = Mathf.Lerp(currentRate, targetRate, Time.deltaTime * 2f);
        emission.rateOverTime = newRate;

        // 基础风力平滑起伏（作用于已生成粒子）
        float targetForce = Mathf.Lerp(baseForceX, maxForceX * 0.6f, f01);

        // --- 阵风（短时提升） ---
        if (enableGust)
        {
            if (gustTimer <= 0f && Random.value < gustChance * Time.deltaTime)
            {
                gustTimer = gustDuration;
                // 阵风目标更高一点
                gustTarget = Random.Range(maxForceX * 0.7f, maxForceX);
            }

            if (gustTimer > 0f)
            {
                gustTimer -= Time.deltaTime;
                targetForce = Mathf.Lerp(targetForce, gustTarget, gustBlend);
            }
        }

        // 平滑到目标风力
        float currentForceX = force.x.constant;
        float newForceX = Mathf.Lerp(currentForceX, targetForce, Time.deltaTime * 3f);

        // 写回 Force over Lifetime（仅 X）
        force.enabled = true;
        force.space = ParticleSystemSimulationSpace.World;
        force.x = new ParticleSystem.MinMaxCurve(newForceX);
        // Y 不动：force.y = 0
    }
}
