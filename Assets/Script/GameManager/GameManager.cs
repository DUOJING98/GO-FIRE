using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 対戦ゲーム全体を管理するゲームマネージャ。
/// ラウンド開始/終了、入力受付フロー、HP更新、勝敗判定、UI表示、SE再生を統括する。
/// </summary>
/// <remarks>
/// CountDownManager が発する「GO/FAKE」シグナル（イベント）をトリガに、
/// 反応時間・Perfect判定・ダメージ計算を行う。準備フェーズ→ラウンド→遷移の流れを一本化。
/// </remarks>
public class GameManager : MonoBehaviour
{
    /// <summary>カウントダウンやGO/Fake発火を担当する外部コンポーネント。</summary>
    public CountDownManager CDM;

    [Header("Player / UI")]
    [SerializeField] Push p1;                       // P1の入力/姿勢制御
    [SerializeField] Push p2;                       // P2の入力/姿勢制御
    [SerializeField] HealthBar p1HPBar;             // P1のHPバーUI
    [SerializeField] HealthBar p2HPBar;             // P2のHPバーUI
    [SerializeField] Text Perfect;                  // PERFECT演出用テキスト

    [Header("Audio")]
    [SerializeField] AudioClip audioClip;           // ヒット/ミス時などのSE
    private AudioSource audioSource;                // 再生に使用するAudioSource

    [Header("HP")]
    [SerializeField] private int p1Hp = 100;        // P1の初期HP
    [SerializeField] private int p2Hp = 100;        // P2の初期HP

    // ラウンド進行状態
    private bool roundEnded = false;
    public static bool currentIsRealSignal = false; // 現在の合図が本物（GO）かどうか
    private Coroutine signalLoopCoroutine;          //（未使用のため将来の拡張用）
    private string firstPlayerPressed = null;       // 最初にボタンを押したプレイヤーID（"P1"/"P2"）
    public string FirstPlayerPressed => firstPlayerPressed;
    private float goSignalTime = -1f;               // GOが出た時刻（Perfect判定用）
    private Coroutine timeoutCoroutine;             // タイムアウト監視用（使用例はWaitForTimeoutに集約）

    [Header("Ready")]
    /// <summary>試合開始前の「準備OK?」フェーズのフラグ。</summary>
    public bool isWaitingForReady = true;
    public AudioSource readySE;                     // 準備完了時のSE
    private bool P1Ready = false;                   // P1が準備OKか
    private bool P2Ready = false;                   // P2が準備OKか
    [SerializeField] Text PushToReady;              // 「押して準備」の案内テキスト
    [SerializeField] Text p1ready;                  // P1 Ready表示
    [SerializeField] Text p2ready;                  // P2 Ready表示

    [Header("GameOver")]
    [SerializeField] Text gameOverText;             // ゲームオーバー演出用テキスト

    [Header("Perfect 判定")]
    [SerializeField] float perfectTime = 3.0f;      // GO後、何秒以内ならPerfect扱いにするか

    [Header("Perfect 演出（未使用サンプル）")]
    [SerializeField] GameObject perfactBG;          // Perfect背景（使用例はコメント化）
    [SerializeField] GameObject BlackBG;            // 演出用黒背景（使用例はコメント化）

    [Header("FireFlash")]
    public Flash1 Flash1;
    [SerializeField] Transform muzzlePoint1;
    [SerializeField] Transform muzzlePoint2;
    [SerializeField] Flash muzzleFlash1;
    [SerializeField] Flash muzzleFlash2;

    private void Awake()
    {
        if (!muzzlePoint1 && muzzlePoint1 != null)
        {
            muzzleFlash1 = muzzlePoint1.GetComponent<Flash>();
        }
        if (!muzzlePoint2 && muzzlePoint2 != null)
        {
            muzzleFlash2 = muzzlePoint2.GetComponent<Flash>();
        }
    }

    private void Start()
    {
        // --- SE初期化 ---
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;

        // --- 初期UI表示（各種テキストを可視に）---
        CDM.signalText.gameObject.SetActive(true);
        CDM.UIText.gameObject.SetActive(true);
        Perfect.gameObject.SetActive(true);

        // --- CountDownManagerのイベント購読 ---
        // GO（本物の合図）が来たときの処理
        CDM.onGoSignal.AddListener(() =>
        {
            currentIsRealSignal = true;
            goSignalTime = Time.time;        // GO表示の正確な時刻を記録（Perfect/反応時間用）
            CDM.StartUpdateTimer();          // 反応計測用のタイマー開始
            StartRound(true);                // ラウンド入力可へ
            StartCoroutine(nameof(WaitForTimeout)); // 一定時間無入力なら引き分け判定
        });

        // ダミー（フェイク）合図が来たときの処理
        CDM.onFakeSignal.AddListener(() =>
        {
            currentIsRealSignal = false;
            StartRound(false);
        });

        // 最初は準備フェーズから開始
        StartPreparationPhase();
    }

    /// <summary>
    /// 準備フェーズを開始。プレイヤーに「押して準備」を促し、各種フラグを初期化。
    /// </summary>
    void StartPreparationPhase()
    {
        CDM.UIText.text = " ";
        PushToReady.gameObject.SetActive(true);
        CDM.signalText.text = null;
        Perfect.text = null;
        isWaitingForReady = true;
        P1Ready = false;
        P2Ready = false;
    }

    /// <summary>
    /// 新しいラウンドを開始。UI/フラグ/姿勢の初期化、カウントダウン開始を行う。
    /// </summary>
    private void StartNewRound()
    {
        Debug.Log("start new round");

        // 案内非表示＆状態初期化
        PushToReady.gameObject.SetActive(false);
        roundEnded = false;
        Perfect.text = null;
        CDM.timerText.text = "0.0";
        CDM.UIText.text = null;
        firstPlayerPressed = null;

        // プレイヤー姿勢をスタンドにリセット
        p1.SetStandPose();
        p2.SetStandPose();

        // 入力とシグナル状態の初期化
        CDM.canInput = false;
        CDM.hasGoAppeared = false;
        currentIsRealSignal = false;     // フェーズ遷移時の誤検知防止

        // カウントダウン（CDM側でGO/FAKE発火）
        CDM.StartCountdown();

        // 表示類の初期化
        CDM.reactionText.gameObject.SetActive(false);
        p1ready.gameObject.SetActive(false);
        p2ready.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // ESCでアプリ終了（PC用の簡易Quit）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    /// <summary>
    /// ラウンドの入力可否を切り替える。
    /// </summary>
    /// <param name="isRealGo">今回の合図が本物（GO）かどうか。</param>
    void StartRound(bool isRealGo)
    {
        CDM.canInput = true;
        roundEnded = false;
    }

    /// <summary>
    /// 任意プレイヤーのボタン押下イベントの入口。
    /// 準備フェーズ・ラウンド中を統合処理し、Perfect/ヒット/ミスに応じてHPやUIを更新。
    /// </summary>
    /// <param name="playerName">"P1" または "P2"</param>
    /// <param name="isCorrect">GOの最中で正しく押下したかどうか（CDM側の判定結果）</param>
    public void PlayerPressed(string playerName, bool isCorrect)
    {
        // 入力受理と同時に一旦、他の入力受付を止める（誤多重入力防止）
        CDM.canInput = false;

        // --- 準備フェーズ中の押下は「準備完了」として処理 ---
        if (isWaitingForReady)
        {
            if (playerName == "P1")
            {
                P1Ready = true;
                if (readySE != null) readySE.Play();
            }
            if (playerName == "P2")
            {
                P2Ready = true;
                if (readySE != null) readySE.Play();
            }

            if (P1Ready) p1ready.gameObject.SetActive(true);
            if (P2Ready) p2ready.gameObject.SetActive(true);

            // 両者の準備完了を確認してゲーム開始
            if (P1Ready && P2Ready)
            {
                isWaitingForReady = false;
                CDM.UIText.text = null;
                Invoke(nameof(StartNewRound), 0.5f); // 少し間を置いて開始演出
            }
            return; // 準備フェーズのため早期リターン
        }

        // --- ラウンド終了済み、またはすでに誰かが押しているなら無視 ---
        if (roundEnded || firstPlayerPressed != null)
        {
            Debug.Log("round end,can not input");
            return;
        }

        // 最初に押したプレイヤーを記録
        firstPlayerPressed = playerName;

        // 双方発砲ポーズに切替（演出）
        //p1.SetFirePose();
        //p2.SetFirePose();

        // カウント/ループ停止（反応時間計測の締め）
        CDM.StopLoop();
        CDM.StopUpdateTimer();  // 反応時間のカウント停止
        CDM.StopCountdown();    // カウントダウン停止

        bool isP1 = playerName == "P1";

        // --- Perfect判定 ---
        bool isPerfect = false;
        if (currentIsRealSignal && isCorrect && goSignalTime > 0)
        {
            float timeSinceGo = Time.time - goSignalTime;
            if (timeSinceGo <= perfectTime)
            {
                isPerfect = true;
            }
        }

        // --- 反応時間の表示（ミリ秒3桁まで、0.000s形式）---
        float reaction = CDM.GetCurrentReactionTime();
        reaction = MathF.Round(reaction * 1000f) / 1000f;

        // --- ダメージ＆UI分岐 ---
        if (isPerfect)
        {
            // Perfect命中：即撃破（100ダメージ）
            if (isP1) p2Hp -= 100;
            else p1Hp -= 100;

            CDM.reactionText.gameObject.SetActive(true);
            Perfect.text = "PERFECT!!";
            CDM.reactionText.text = $"{reaction:0.000}s";
        }
        else if (currentIsRealSignal)
        {
            // 本物のGO中に正しく押せた：相手へ50ダメージ
            if (isP1)
            {
                if (muzzleFlash1) muzzleFlash1.TriggerFlash();
                if (Flash1) Flash1.TriggerFlash();
                p2Hp -= 50;
                p2.GetComponent<DamageFlash>().TakeDamage(); // 被弾演出
            }
            else
            {
                if (muzzleFlash2) muzzleFlash2.TriggerFlash();

                if (Flash1) Flash1.TriggerFlash();
                p1Hp -= 50;
                p1.GetComponent<DamageFlash>().TakeDamage();
            }
            CDM.reactionText.gameObject.SetActive(true);
            CDM.UIText.text = $"{playerName} HIT!";
            CDM.reactionText.text = $"{reaction:0.000}s";

            audioSource.Play();
        }
        else
        {
            // フェイク中に押した：自傷50ダメージ（ミス）
            if (isP1)
            {
                p1.GetComponent<SpriteRenderer>().transform.localScale = new Vector3(0, 180, 0);
                p2.SetFirePose();
                p1Hp -= 50;
                p1.GetComponent<DamageFlash>().TakeDamage();
            }
            else
            {
                p2.GetComponent<SpriteRenderer>().transform.localScale = new Vector3(0, 180, 0);
                p1.SetFirePose();
                p2Hp -= 50;
                p2.GetComponent<DamageFlash>().TakeDamage();
            }
            CDM.UIText.text = $"{playerName} MISS!";
            audioSource.Play();
        }

        // --- HPバー同期 ---
        p1HPBar.setHP(p1Hp);
        p2HPBar.setHP(p2Hp);

        roundEnded = true;

        // --- 勝敗チェック ---
        if (p1Hp <= 0 || p2Hp <= 0)
        {
            EndGame();
        }
        else
        {
            // 次ラウンドへ移行（少し間を空けて演出）
            Invoke(nameof(StartNewRound), 2.5f);
        }
    }

    /// <summary>
    /// GO後に一定時間内に誰も押さない場合は引き分けにする。
    /// </summary>
    private IEnumerator WaitForTimeout()
    {
        yield return new WaitForSeconds(1f);
        if (!roundEnded && string.IsNullOrEmpty(firstPlayerPressed))
        {
            CDM.canInput = false;  // 入力締め
            CDM.StopLoop();
            CDM.signalText.text = "";
            yield return new WaitForSeconds(0.5f);
            CDM.UIText.text = "DRAW";
            Invoke(nameof(StartNewRound), 2f);
        }
    }

    /// <summary>
    /// 試合終了演出と次シーンへの遷移準備。
    /// </summary>
    private void EndGame()
    {
        CDM.signalText.text = null;

        // 勝者サイドに合わせてWIN表示位置をずらす（左右演出）
        if (p1Hp <= 0)
        {
            CDM.UIText.rectTransform.anchoredPosition = new Vector2(688, -448);
        }
        else
        {
            CDM.UIText.rectTransform.anchoredPosition = new Vector2(-688, -448);
        }

        CDM.UIText.text = "WIN!";
        gameOverText.gameObject.SetActive(true);
        Invoke(nameof(ToGameover), 1f);
    }

    /// <summary>
    /// いずれかのキーが押されるまで待ってエンディングシーンへ。
    /// </summary>
    IEnumerator AnyKeyDown()
    {
        yield return new WaitUntil(() => Input.anyKeyDown);
        SceneManager.LoadScene("EndingScene");
    }

    /// <summary>
    /// エンディングへの遷移をコルーチンで開始。
    /// </summary>
    void ToGameover()
    {
        StartCoroutine(nameof(AnyKeyDown));
    }
}
