using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// カウントダウン表示・合図（GO/FAKE）の生成・反応時間タイマーを管理するコンポーネント。
/// ゲーム進行の「準備→ランダムなシグナル出力→入力受付」の中核を担う。
/// </summary>
public class CountDownManager : MonoBehaviour
{
    [Header("Delay / Interval（将来の調整用）")]
    [Tooltip("GO/FAKE を出すまでの遅延の最小値（未使用パラメータ、拡張フック）")]
    public float minDelay = 2f;

    [Tooltip("GO/FAKE を出すまでの遅延の最大値（未使用パラメータ、拡張フック）")]
    public float maxDelay = 6f;

    [Tooltip("シグナル間隔（未使用パラメータ、将来のテンポ調整に使用予定）")]
    public float signalInterval = 1f;

    [Header("UI")]
    public Text signalText;    // シグナル表示（GO!/WAIT! など）
    public Text UIText;        // 汎用UIテキスト（メッセージ表示）
    public Text timerText;     // 反応時間の表示
    public Text reactionText;  // リアクション（GameManager側で有効化・文言設定）

    [Header("Events")]
    public UnityEvent onReadyStart; // 「READY...」表示のタイミングで発火
    public UnityEvent onGoSignal;   // 本物の合図（GO!）発火
    public UnityEvent onFakeSignal; // フェイク合図発火

    // --- SE ---
    [Header("SE")]
    [SerializeField] AudioClip goClip;   // GO時のSE
    [SerializeField] AudioClip fakeClip; // フェイク時のSE
    private AudioSource audioSource;

    // --- フラグ類 ---
    [Header("チェック")]
    [Tooltip("今回のサイクルで GO が既に出現したかどうか")]
    public bool hasGoAppeared = false;

    [Tooltip("直近に出した合図が本物（GO）かどうか")]
    public bool isRealSignal = false;

    [Tooltip("外部が入力を受け付けて良い状態かどうか（GameManager からも制御）")]
    public bool canInput = false;

    // --- タイマー管理（反応時間） ---
    [Tooltip("現在の反応時間（秒）")]
    public float timerValue = 0f;

    private float TimeStartTime = 0f; // 計測開始のタイムスタンプ（Time.time）

    // --- コルーチンハンドル（安全に停止するため保持） ---
    public Coroutine UpdateTimerCoroutine;
    public Coroutine CountdownRoutineCoroutine;
    public Coroutine SignalLoopCoroutine;
    public Coroutine timerCoroutine; // 予備（未使用）

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (UIText != null) UIText.text = " ";
    }

    private void Update()
    {
        // 毎フレームの処理は現状なし（拡張用）
    }

    // ===== 反応時間タイマー =====

    /// <summary>
    /// 反応時間の表示を開始。以後はフレーム毎に経過秒を更新する。
    /// </summary>
    public void StartUpdateTimer()
    {
        // 二重起動を避けるため、既存のコルーチンがあれば止める
        if (UpdateTimerCoroutine != null) StopCoroutine(UpdateTimerCoroutine);

        TimeStartTime = Time.time;
        timerValue = 0f;

        if (timerText != null)
            timerText.text = "0.000s"; // 表示を即リセット（小数3桁+単位）

        UpdateTimerCoroutine = StartCoroutine(UpdateTimer());
    }

    /// <summary>
    /// 反応時間の更新を停止。
    /// </summary>
    public void StopUpdateTimer()
    {
        if (UpdateTimerCoroutine != null)
        {
            StopCoroutine(UpdateTimerCoroutine);
            UpdateTimerCoroutine = null;
        }
    }

    /// <summary>
    /// フレームごとに経過時間を計測し、UIへ反映する。
    /// </summary>
    private IEnumerator UpdateTimer()
    {
        while (true)
        {
            float elapsed = Time.time - TimeStartTime;
            timerValue = elapsed;

            // 表示は 0.000s 形式に統一（GameManager 側の表示方針に合わせる）
            if (timerText != null)
                timerText.text = $"{elapsed:0.000}s";

            yield return null;
        }
    }

    // ===== カウントダウン（3→2→1→READY...） =====

    /// <summary>
    /// 新しいラウンドのカウントダウンを開始（3→2→1→READY...→SignalLoop）。
    /// </summary>
    public void StartCountdown()
    {
        if (CountdownRoutineCoroutine != null) StopCoroutine(CountdownRoutineCoroutine);
        CountdownRoutineCoroutine = StartCoroutine(CountdownRoutine());
    }

    /// <summary>
    /// カウントダウンを停止。
    /// </summary>
    public void StopCountdown()
    {
        if (CountdownRoutineCoroutine != null)
        {
            StopCoroutine(CountdownRoutineCoroutine);
            CountdownRoutineCoroutine = null;
        }
    }

    /// <summary>
    /// 3→2→1→READY... の順で表示し、準備イベントを発火後にシグナルループへ遷移。
    /// </summary>
    private IEnumerator CountdownRoutine()
    {
        if (signalText != null) signalText.text = "3";
        yield return new WaitForSeconds(1f);

        if (signalText != null) signalText.text = "2";
        yield return new WaitForSeconds(1f);

        if (signalText != null) signalText.text = "1";
        yield return new WaitForSeconds(1f);

        if (signalText != null) signalText.text = "READY...";
        onReadyStart?.Invoke();
        yield return new WaitForSeconds(1f);

        ClearText();

        // 次段階：ランダムな GO/FAKE を出すループへ
        if (SignalLoopCoroutine != null) StopCoroutine(SignalLoopCoroutine);
        SignalLoopCoroutine = StartCoroutine(SignalLoop());
    }

    // ===== GO/FAKE のランダム出力ループ =====

    /// <summary>
    /// ランダムなタイミングで GO/FAKE を出力するループ。
    /// 一度 GO を出したらループは終了する（hasGoAppeared = true）。
    /// </summary>
    public IEnumerator SignalLoop()
    {
        Debug.Log("start loop");
        canInput = true;
        hasGoAppeared = false;

        while (!hasGoAppeared)
        {
            // ランダム遅延（今は 2〜3 秒で固定、将来 min/max を用いた揺らぎ対応も可能）
            float delay = Random.Range(2f, 3f);
            yield return new WaitForSeconds(delay);

            // 0 → GO!, それ以外 → フェイク（確率は 1/3 でGO）
            int rand = Random.Range(0, 3); // 0: GO, 1/2: FAKE

            if (rand == 0)
            {
                // --- 本物の合図（GO!） ---
                if (signalText != null) signalText.text = "GO!";
                onGoSignal?.Invoke();
                isRealSignal = true;
                hasGoAppeared = true;

                // SE 再生
                if (audioSource != null && goClip != null)
                    audioSource.PlayOneShot(goClip);
            }
            else
            {
                // --- フェイク（WAIT!/DOG!/START!/空白 など） ---
                string[] fakeSignals = { "WAIT!", "DOG!", "START!", " ", " ", " " };
                string fake = fakeSignals[Random.Range(0, fakeSignals.Length)];

                if (signalText != null) signalText.text = fake;
                onFakeSignal?.Invoke();
                isRealSignal = false;

                // 空白の場合はSEを鳴らさない
                if (audioSource != null && fakeClip != null && !string.IsNullOrWhiteSpace(fake))
                    audioSource.PlayOneShot(fakeClip);
            }
        }
    }

    /// <summary>
    /// シグナルループを停止（GO確定・ラウンド終了時などに外部から呼び出し）。
    /// </summary>
    public void StopLoop()
    {
        if (SignalLoopCoroutine != null)
        {
            StopCoroutine(SignalLoopCoroutine);
            SignalLoopCoroutine = null;
        }
    }

    /// <summary>
    /// 数字を1秒だけ表示する簡易演出（拡張用途）。
    /// </summary>
    private IEnumerator ShowNumber(string number)
    {
        if (signalText != null) signalText.text = number;
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// シグナル領域と汎用UIテキストをクリア。
    /// </summary>
    public void ClearText()
    {
        if (signalText != null) signalText.text = " ";
        if (UIText != null) UIText.text = " ";
    }

    /// <summary>
    /// 現在の反応時間（秒）を取得。
    /// </summary>
    public float GetCurrentReactionTime()
    {
        return timerValue;
    }
}
