using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.Audio;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    public CountDownManager CDM;

    [SerializeField] Push p1;
    [SerializeField] Push p2;
    [SerializeField] HealthBar p1HPBar;
    [SerializeField] HealthBar p2HPBar;
    [SerializeField] Text Perfect;
    [SerializeField] AudioClip audioClip;
    private AudioSource audioSource;

    [SerializeField] private int p1Hp = 100, p2Hp = 100, BaseDamage = 20, damage, firstAttackNum;

    private bool roundEnded = false;
    public static bool currentIsRealSignal = false;
    private Coroutine signalLoopCoroutine;
    private string firstPlayerPressed = null; // 最初にボタンを押したプレイヤー
    public string FirstPlayerPressed => firstPlayerPressed;

    private float goSignalTime = -1f, firstPlayerPushSignalTime = -1f;
    private Coroutine timeoutCoroutine;     //タイムアウトの検査
    private Coroutine WaitForSecondPlayerTimeoutCoroutine;


    private float goSignalTime = -1f;               // GOが出た時刻（Perfect判定用）
    private Coroutine timeoutCoroutine;             // タイムアウト監視用（使用例はWaitForTimeoutに集約）
    public bool isMiss;

    [Header("Ready")]
    //開始前の準備
    public bool isWaitingForReady = true;
    private bool P1Ready = false;
    private bool P2Ready = false;
    private bool P1Inputed = false;
    private bool P2Inputed = false;
    private bool isPerfect = false;
    [SerializeField] Text p1ready;
    [SerializeField] Text p2ready;

    [Header("GameOver")]
    [SerializeField] Text gameOverText;

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
        p1.SetStandPose(false); p2.SetStandPose(false);
    }

    private void Start()
    {
        //SE
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;

        //文字表示演出
        CDM.signalText.gameObject.SetActive(true);
        CDM.UIText.gameObject.SetActive(true);
        Perfect.gameObject.SetActive(true);


        CDM.onGoSignal.AddListener(() =>
        {
            currentIsRealSignal = true;
            goSignalTime = Time.time; //  记录 GO! 出现的时刻

            CDM.StartUpdateTimer(); //  开始计时器

            StartRound(true);
            StartCoroutine(nameof(WaitForTimeout));
        });

        CDM.onFakeSignal.AddListener(() =>
        {
            currentIsRealSignal = false;
            StartRound(false);
        });

        CDM.onFirstPlayerPushSignal.AddListener(() =>
        {
            firstPlayerPushSignalTime = Time.time;
        });

        StartPreparationPhase();

    }
    void StartPreparationPhase()
    {
        CDM.UIText.text = "PRESS TO READY";
        CDM.signalText.text = null;
        Perfect.text = null;
        isWaitingForReady = true;
        P1Ready = false;
        P2Ready = false;
    }
    private void StartNewRound()
    {

        Debug.Log("start new round");
        roundEnded = false;
        Perfect.text = null;
        isPerfect = false;
        CDM.timerText.text = "0.0";
        CDM.UIText.text = null;
        firstPlayerPressed = null;

        p1.ResetRound();
        p2.ResetRound();


        // プレイヤー姿勢をスタンドにリセット
        p1.SetStandPose(false);
        p2.SetStandPose(false);

        // 入力とシグナル状態の初期化

        CDM.canInput = false;
        CDM.hasGoAppeared = false;
        currentIsRealSignal = false; // 初始化为false，避免意外  
        CDM.StartCountdown();
        CDM.reactionText.gameObject.SetActive(false);
        p1ready.gameObject.SetActive(false);
        p2ready.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        P1Inputed = false;
        P2Inputed = false;
        firstAttackNum = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void StartRound(bool isRealGo)
    {
        CDM.canInput = true;
        roundEnded = false;
        p1.BeginRound(isRealGo);
        p2.BeginRound(isRealGo);
    }

    public void PlayerPressed(string playerName, int attackNum)
    {
        //CDM.canInput = false;
        //ボタンを押すと準備完了
        if (isWaitingForReady)
        {
            if (playerName == "P1") P1Ready = true;
            if (playerName == "P2") P2Ready = true;
            if (P1Ready) p1ready.gameObject.SetActive(true);
            if (P2Ready) p2ready.gameObject.SetActive(true);
            //重複押す防止
            if (P1Ready && P2Ready)
            {
                isWaitingForReady = false;
                CDM.UIText.text = null;
                Invoke(nameof(StartNewRound), 0.5f);//ゲーム開始
            }
            return;
        }

        // ラウンド終了、入力不可、または既に入力済みの場合は無視
        if (roundEnded || firstPlayerPressed == playerName)
        {
            Debug.Log("can not input !!");
            return;
        }
        // ダメージ処理
        bool isP1 = playerName == "P1";
        float timeSinceGo = Time.time - goSignalTime;
        //Perfectチェック

        if (currentIsRealSignal)
        {
            if (timeSinceGo <= perfectTime)
            {
                isPerfect = true;
            }
        }
        if (firstPlayerPressed == null)
        {
            firstPlayerPressed = playerName;
            firstAttackNum = attackNum;
            CDM.StopUpdateTimer(); //按下时停止计时器
            WaitForSecondPlayerTimeoutCoroutine = StartCoroutine(WaitForSecondPlayerTimeout(isPerfect));
        }
        if (playerName == "P1")
        {

            P1Inputed = true;
        }
        else
        {
            P2Inputed = true;

            isMiss = false;
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
            isMiss = true;
            // フェイク中に押した：自傷50ダメージ（ミス）
            if (isP1)
            {
                p1.SetStandPose(true);
                p2.SetFirePose();
                p1Hp -= 50;
                p1.GetComponent<DamageFlash>().TakeDamage();
            }
            else
            {
                p2.SetStandPose(true);
                p1.SetFirePose();
                p2Hp -= 50;
                p2.GetComponent<DamageFlash>().TakeDamage();
            }
            CDM.UIText.text = $"{playerName} MISS!";
            audioSource.Play();

        }

        CDM.StopLoop();
        CDM.StopCountdown(); //stop time count

        // ダメージ処理
        //bool isP1 = playerName == "P1";
        //float timeSinceGo = Time.time - goSignalTime;
        ////Perfectチェック
        //bool isPerfect = false;


        //次の人が撃てるのかを判断する
        if (P1Inputed && P2Inputed)
        {
            StopCoroutine(WaitForSecondPlayerTimeoutCoroutine);
            WaitForSecondPlayerTimeoutCoroutine = null;
            // StopCoroutine(nameof(WaitForSecondPlayerTimeout));
            CDM.canInput = false;
            playerAttack(isPerfect, firstPlayerPressed, attackNum);
        }
    }
    //何秒経過後、ボタン押されていない場合、引分
    private IEnumerator WaitForTimeout()
    {
        yield return new WaitForSeconds(1f);
        if (!roundEnded && string.IsNullOrEmpty(firstPlayerPressed))
        {
            //CDM.StopTimer();
            CDM.canInput = false;
            CDM.StopLoop();
            CDM.signalText.text = "";
            yield return new WaitForSeconds(0.5f);
            CDM.UIText.text = "DRAW";
            Invoke(nameof(StartNewRound), 2f);
        }
    }

    //
    private IEnumerator WaitForSecondPlayerTimeout(bool isPerfect)
    {
        yield return new WaitForSeconds(0.5f);
        playerAttack(isPerfect, firstPlayerPressed, firstAttackNum);
    }



    private void EndGame()
    {
        CDM.signalText.text = null;
        //string winner;

        if (p1Hp <= 0)
        {
            //winner = "P2";
            CDM.UIText.rectTransform.anchoredPosition = new Vector2(688, -448);
        }
        else
        {
            //winner = "P1";
            CDM.UIText.rectTransform.anchoredPosition = new Vector2(-688, -448);
        }

        //CDM.UIText.text = $"{winner} WIN!";
        CDM.UIText.text = "WIN!";
        gameOverText.gameObject.SetActive(true);
        Invoke(nameof(ToGameover), 1f);
    }

    IEnumerator AnyKeyDown()
    {
        yield return new WaitUntil(() => Input.anyKeyDown);
        SceneManager.LoadScene("EndingScene");
    }


    void ToGameover()
    {
        StartCoroutine(nameof(AnyKeyDown));
    }

    void playerAttack(bool isPerfect, string playerName, int attackNum)
    {
        //damage calc
        damage = BaseDamage;
        if (firstAttackNum > attackNum || (firstAttackNum == 1 && attackNum == 3) || !(P1Inputed && P2Inputed))
        {
            damage += 20;
        }
        if (isPerfect)
        {
            damage += 20;
        }
        //反応時間表示
        float reaction = CDM.GetCurrentReactionTime();
        reaction = MathF.Round(reaction * 1000f) / 1000f;
        if (currentIsRealSignal)
        {
            if (playerName == "P1") p2Hp -= damage;
            else p1Hp -= damage;
            CDM.reactionText.gameObject.SetActive(true);
            CDM.UIText.text = isPerfect ? "PERFECT!!" : $"{playerName} HIT!";
            CDM.reactionText.text = $"{reaction:0.000}s";
            audioSource.Play();
        }
        else
        {
            if (playerName == "P1") p1Hp -= BaseDamage;
            else p2Hp -= damage;
            CDM.UIText.text = $"{playerName} MISS!";
            audioSource.Play();
        }

        p1HPBar.setHP(p1Hp);
        p2HPBar.setHP(p2Hp);

        roundEnded = true;

        if (p1Hp <= 0 || p2Hp <= 0)
        {
            EndGame();
        }
        else
        {
            Invoke(nameof(StartNewRound), 2.5f);
        }
    }
}
