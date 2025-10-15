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
    private int player1PerfectTimes, player2PerfectTimes, player1RPSWinTimes, player2RPSWinTimes;

    private bool roundEnded = false;
    public static bool currentIsRealSignal = false;
    private Coroutine signalLoopCoroutine;
    private string firstPlayerPressed = null; // 最初にボタンを押したプレイヤー
    public string FirstPlayerPressed => firstPlayerPressed;
    private float goSignalTime = -1f, firstPlayerPushSignalTime = -1f;
    private Coroutine timeoutCoroutine;     //タイムアウトの検査
    private Coroutine WaitForSecondPlayerTimeoutCoroutine;

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

    [Header("TEST")]
    [SerializeField] float perfectTime = 1.0f;

    private void Start()
    {
        //clear gamedatas
        PlayerPrefs.DeleteAll();
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
        roundEnded = false;
        Perfect.text = null;
        isPerfect = false;
        CDM.timerText.text = "0.0";
        CDM.UIText.text = null;
        CDM.PerfectText.text = null;
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
            return;
        }
        // ダメージ処理
        bool isP1 = playerName == "P1";
        float timeSinceGo = Time.time - goSignalTime;
        //Perfectチェック

        if (currentIsRealSignal)
        {
            if (timeSinceGo <= perfectTime && firstPlayerPressed == null)
            {
                isPerfect = true;
                if (isP1)
                {
                    player1PerfectTimes++;
                }
                else
                {
                    player2PerfectTimes++;
                }
                Debug.Log("player1Perfect=" + player1PerfectTimes + "  player2Perfect=" + player2PerfectTimes);
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
        PlayerPrefs.SetInt("player1Hp", p1Hp);
        PlayerPrefs.SetInt("player2Hp", p2Hp);
        PlayerPrefs.SetInt("player1PerfectTimes", player1PerfectTimes);
        PlayerPrefs.SetInt("player2PerfectTimes", player2PerfectTimes);
        PlayerPrefs.SetInt("player1RPSWinTimes", player1RPSWinTimes);
        PlayerPrefs.SetInt("player2RPSWinTimes", player2RPSWinTimes);
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
        if (firstAttackNum - attackNum == 1 || firstAttackNum - attackNum == -2 || !(P1Inputed && P2Inputed))
        {
            damage += 10;
            if (currentIsRealSignal)
            {
                if (firstPlayerPressed == "P1")
                {
                    player1RPSWinTimes++;
                }
                else
                {
                    player2RPSWinTimes++;
                }
            }
        }

        else if (firstAttackNum != attackNum)
        {
            damage -= 10;
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
            CDM.UIText.text = $"{playerName}" + (damage > BaseDamage ? " HEAVY HIT!" : damage == BaseDamage ? " HIT!" : " LIGHT HIT!");
            CDM.PerfectText.text = isPerfect ? "Perfect!" : null;
            CDM.reactionText.text = $"{reaction:0.000}s";
            audioSource.Play();
        }
        else
        {
            if (playerName == "P1") p1Hp -= BaseDamage;
            else p2Hp -= BaseDamage;
            CDM.UIText.text = $"{playerName} MISS!";
            //audioSource.Play();
        }

        p1HPBar.setHP(p1Hp);
        p2HPBar.setHP(p2Hp);

        roundEnded = true;

        if (p1Hp <= 0 || p2Hp <= 0)
        {
            p1Hp = p1Hp < 0 ? 0 : p1Hp;
            p2Hp = p2Hp < 0 ? 0 : p2Hp;
            EndGame();
        }
        else
        {
            Invoke(nameof(StartNewRound), 2.5f);
        }
    }
}
