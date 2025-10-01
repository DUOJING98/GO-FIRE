using UnityEngine;

public class Kusa : MonoBehaviour
{
    [Header("風・移動")]
    public int direction = 1; // 1 右向き, -1 左向き
    public float leftX = -10f;
    public float rightX = 10f;

    [Tooltip("基礎移動速度の範囲（風速の変動）")]
    public float minSpeed = 1.2f;
    public float maxSpeed = 3.0f;

    [Header("速度の揺らぎ / スタッター")]
    [Tooltip("速度変化の更新間隔の範囲（秒）")]
    public Vector2 speedChangeInterval = new Vector2(2f, 5f);
    [Tooltip("スタッターが発生する確率（0~1）")]
    [Range(0f, 1f)] public float stutterChance = 0f; // 抽動を避けるためデフォルト0
    [Tooltip("スタッター時の速度割合（0~1）")]
    [Range(0.2f, 1f)] public float stutterSpeedScale = 0.55f;
    [Tooltip("スタッターの持続時間（秒）")]
    public float stutterDuration = 0.15f;

    [Header("バウンス / 細かい上下運動")]
    [Tooltip("バウンスの振幅（単位：メートル）")]
    public float bobAmplitude = 0.06f;
    [Tooltip("バウンスの周波数（Hz）")]
    public float bobFrequency = 1.8f;
    [Tooltip("バウンスのランダム位相/ノイズ強度（不規則さを抑えるなら0推奨）")]
    public float bobNoise = 0f; // 不規則な揺れを減らすためデフォルト0

    [Header("ホップ / 小さなジャンプ")]
    [Tooltip("ジャンプ間隔の範囲（秒）")]
    public Vector2 hopInterval = new Vector2(1.6f, 3.2f);
    [Tooltip("ジャンプ初速度（m/s）")]
    public float hopInitialVelocity = 1.1f;
    [Tooltip("重力（m/s²）, 正の値）")]
    public float gravity = 8.0f;
    [Tooltip("着地時のつぶれ具合（1=つぶれなし, 小さいほどつぶれる）")]
    public float squashMin = 0.88f;
    [Tooltip("つぶれからの回復速度")]
    public float squashRecover = 12f;

    [Header("回転（無滑りロール）")]
    [Tooltip("見た目半径（m）。大きいほど同じ距離で回転が少ない")]
    public float rollRadius = 0.5f;
    [Tooltip("1フレームあたりの最大回転角度（度）")]
    public float maxDegPerFrame = 720f;

    private float baseY; // 地面の基準高さ
    private float curSpeed;
    private float targetSpeed;
    private float speedTimer = 0f;
    private float speedNextChange = 0f;

    // スタッター
    private bool isStuttering = false;
    private float stutterTimer = 0f;

    // バウンス
    private float bobPhaseOffset;
    private float bobY = 0f;

    // ホップ
    private float hopVy = 0f;
    private float hopTimer = 0f;
    private float hopNextTime = 0f;
    private bool isAirborne = false;

    // スクワッシュ＆ストレッチ
    private Vector3 baseScale;
    private float squash = 1f; // 1 通常；<1 垂直方向につぶれ

    // 回転（無滑り）
    private float lastX;
    private float accumDeg; // 積算角度（度）

    void Start()
    {
        // 初期方向が未設定なら位置に応じて決定
        if (direction == 0) direction = transform.position.x > 0 ? -1 : 1;

        baseScale = transform.localScale;
        baseY = transform.position.y;

        curSpeed = Random.Range(minSpeed, maxSpeed);
        targetSpeed = curSpeed;

        speedNextChange = Random.Range(speedChangeInterval.x, speedChangeInterval.y);
        bobPhaseOffset = Random.value * 100f;

        hopNextTime = Random.Range(hopInterval.x, hopInterval.y);

        lastX = transform.position.x;
        accumDeg = transform.eulerAngles.z;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // —— 速度目標値のランダム変化（風速の揺らぎ） ——
        speedTimer += dt;
        if (speedTimer >= speedNextChange)
        {
            targetSpeed = Random.Range(minSpeed, maxSpeed);
            speedNextChange = Random.Range(speedChangeInterval.x, speedChangeInterval.y);
            speedTimer = 0f;

            // スタッター発生判定（デフォルト無効）
            if (!isStuttering && Random.value < stutterChance)
            {
                isStuttering = true;
                stutterTimer = 0f;
            }
        }

        // —— スタッター（短時間の減速） ——
        float speedLerp = isStuttering ? 10f : 2.5f;
        curSpeed = Mathf.Lerp(curSpeed, targetSpeed, dt * speedLerp);
        if (isStuttering)
        {
            stutterTimer += dt;
            curSpeed *= stutterSpeedScale;
            if (stutterTimer >= stutterDuration)
            {
                isStuttering = false;
                targetSpeed = Mathf.Clamp(targetSpeed * Random.Range(1.0f, 1.12f), minSpeed, maxSpeed);
            }
        }

        // —— バウンス（細かい上下動） ——
        float t = Time.time;
        float sinPart = Mathf.Sin((t + bobPhaseOffset) * Mathf.PI * 2f * bobFrequency);
        float noise = (Mathf.PerlinNoise((t + bobPhaseOffset) * 0.8f, 0.37f) - 0.5f) * 2f;
        bobY = (sinPart + noise * bobNoise) * bobAmplitude;

        // —— ホップ（不定期の小ジャンプ：放物線で離陸/着地） ——
        hopTimer += dt;
        if (!isAirborne && hopTimer >= hopNextTime)
        {
            // ジャンプ開始
            hopVy = hopInitialVelocity * Random.Range(0.9f, 1.1f);
            isAirborne = true;
            hopTimer = 0f;
            hopNextTime = Random.Range(hopInterval.x, hopInterval.y);
        }

        float hopOffsetY = 0f;
        if (isAirborne)
        {
            hopVy -= gravity * dt;
            hopOffsetY += hopVy * dt;

            // 着地判定
            if (transform.position.y + bobY + hopOffsetY <= baseY)
            {
                float overshoot = baseY - (transform.position.y + bobY + hopOffsetY);
                hopOffsetY += overshoot;
                isAirborne = false;
                hopVy = 0f;

                // 着地時のスクワッシュ（※ 回転へのランダム衝撃は付与しない）
                squash = Mathf.Min(squash, squashMin);
            }
        }

        // —— 水平方向の移動（ワールド座標） ——
        transform.Translate(Vector3.right * direction * curSpeed * dt, Space.World);

        // —— 垂直位置 = 基準地面 + バウンス + ホップ ——
        Vector3 pos = transform.position;
        pos.y = baseY + bobY + hopOffsetY;
        transform.position = pos;

        // —— 無滑りロール：水平移動距離から回転角を算出（抽動なし） ——
        float dx = transform.position.x - lastX;
        lastX = transform.position.x;

        if (rollRadius > 1e-4f)
        {
            float dDeg = -direction * (dx / rollRadius) * Mathf.Rad2Deg;
            dDeg = Mathf.Clamp(dDeg, -maxDegPerFrame, maxDegPerFrame); // 瞬間移動対策
            accumDeg += dDeg;
            transform.rotation = Quaternion.Euler(0f, 0f, accumDeg);
        }

        // —— ループ境界（無限草原効果） ——
        if (direction == 1 && transform.position.x > rightX)
        {
            transform.position = new Vector3(leftX, transform.position.y, transform.position.z);
            lastX = transform.position.x; // dx暴発防止
        }
        else if (direction == -1 && transform.position.x < leftX)
        {
            transform.position = new Vector3(rightX, transform.position.y, transform.position.z);
            lastX = transform.position.x;
        }

        // —— スクワッシュ回復（時間経過で元に戻る） ——
        squash = Mathf.Lerp(squash, 1f, dt * squashRecover);
        transform.localScale = new Vector3(
        baseScale.x / Mathf.Sqrt(Mathf.Max(0.0001f, squash)),
        baseScale.y * squash,
        baseScale.z
        );
    }
}

