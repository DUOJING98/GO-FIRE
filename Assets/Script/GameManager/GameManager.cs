using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.Audio;

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

    [SerializeField] private int p1Hp = 100, p2Hp = 100;

    private bool roundEnded = false;
    public static bool currentIsRealSignal = false;
    private Coroutine signalLoopCoroutine;
    private string firstPlayerPressed = null; // 最初にボタンを押したプレイヤー
    public string FirstPlayerPressed => firstPlayerPressed;
    private float goSignalTime = -1f;
    private Coroutine timeoutCoroutine;     //タイムアウトの検査

    [Header("Ready")]
    //開始前の準備
    public bool isWaitingForReady = true;
    private bool P1Ready = false;
    private bool P2Ready = false;
    [SerializeField] Text p1ready;
    [SerializeField] Text p2ready;

    [Header("GameOver")]
    [SerializeField] Text gameOverText;

    [Header("TEST")]
    [SerializeField] float perfectTime = 3.0f;

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
        //StartNewRound();
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
        CDM.timerText.text = "0.0";
        CDM.UIText.text = null;
        firstPlayerPressed = null;
        p1.ResetRound();
        p2.ResetRound();
        CDM.canInput = false;
        CDM.hasGoAppeared = false;
        currentIsRealSignal = false; // 初始化为false，避免意外  
        CDM.StartCountdown();
        CDM.reactionText.gameObject.SetActive(false);
        p1ready.gameObject.SetActive(false);
        p2ready.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        //p1.ClearReady();
        //p2.ClearReady();
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

    public void PlayerPressed(string playerName, bool isCorrect,int attackNum)
    {
        CDM.canInput = false;
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
        if (roundEnded || firstPlayerPressed != null)
        {
            Debug.Log("round end,can not input");
            return;
        } 

        firstPlayerPressed = playerName;
        CDM.StopLoop(); 
        CDM.StopUpdateTimer(); //按下时停止计时器
        CDM.StopCountdown(); //stop time count
        //if (timeoutCoroutine != null)
        //{
        //    StopCoroutine(timeoutCoroutine);
        //    timeoutCoroutine = null;
        //}

        // ダメージ処理
        bool isP1 = playerName == "P1";
        //Perfectチェック
        bool isPerfect = false;
        if (currentIsRealSignal && isCorrect && goSignalTime > 0)
        {
            float timeSinceGo = Time.time - goSignalTime;
            if (timeSinceGo <= perfectTime)
            {
                isPerfect = true;
            }
        }

        //反応時間表示
        float reaction = CDM.GetCurrentReactionTime();
        reaction = MathF.Round(reaction * 1000f) / 1000f;
        if (isPerfect)
        {
            //  Perfect 命中
            if (isP1) p2Hp -= 100;
            else p1Hp -= 100;
            CDM.reactionText.gameObject.SetActive(true);
            Perfect.text = "PERFECT!!";
            CDM.reactionText.text = $"{reaction:0.000}s";
        }
        else if (currentIsRealSignal)
        {
            if (isP1) p2Hp -= 50;
            else p1Hp -= 50;
            CDM.reactionText.gameObject.SetActive(true);
            CDM.UIText.text = $"{playerName} HIT!";
            CDM.reactionText.text = $"{reaction:0.000}s";
            audioSource.Play();
        }
        else if (!currentIsRealSignal)
        {
            if (isP1) p1Hp -= 50;
            else p2Hp -= 50;
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

        //Debug.Log($"P1 HP: {p1Hp}, P2 HP: {p2Hp}");


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
}
