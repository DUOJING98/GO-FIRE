﻿using System;
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
    private bool currentIsRealSignal = false;
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

            CDM.StartTimer(); //  开始计时器

            StartRound(true);
            timeoutCoroutine = StartCoroutine(WaitForTimeout());
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
        Perfect.text = null;
        CDM.timerText.text = "0.0";
        CDM.UIText.text = null;
        firstPlayerPressed = null;
        p1.ResetRound();
        p2.ResetRound();
        CDM.canInput = false;
        currentIsRealSignal = false; // 初始化为false，避免意外  
        CDM.StartCountdown();
        roundEnded = false;
        CDM.reactionText.gameObject.SetActive(false);
        //p1.ClearReady();
        //p2.ClearReady();
    }

    void StartRound(bool isRealGo)
    {
        CDM.canInput = true;
        roundEnded = false;
        p1.BeginRound(isRealGo);
        p2.BeginRound(isRealGo);
    }

    public void PlayerPressed(string playerName, bool isCorrect)
    {
        //ボタンを押すと準備完了
        if (isWaitingForReady)
        {
            if (playerName == "P1") P1Ready = true;
            if (playerName == "P2") P2Ready = true;

            //重複押す防止
            if(P1Ready && P2Ready)
            {
                isWaitingForReady= false;
                CDM.UIText.text= null;
                StartNewRound();//ゲーム開始
            }
            return;
        }


        if (roundEnded ||
           !CDM.canInput ||
            firstPlayerPressed != null) return; // ラウンド終了、入力不可、または既に入力済みの場合は無視

        firstPlayerPressed = playerName;
        CDM.canInput = false;
        CDM.StopLoop();


        CDM.StopTimer(); //  按下时停止计时器
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        // ダメージ処理
        bool isP1 = playerName == "P1";
        //Perfectチェック
        bool isPerfect = false;
        if (currentIsRealSignal && isCorrect && goSignalTime > 0)
        {
            float timeSinceGo = Time.time - goSignalTime;
            if (timeSinceGo <= 0.3f)
            {
                isPerfect = true;
            }
        }

        bool damageOpponent = currentIsRealSignal && isCorrect;
        bool damageSelf = !currentIsRealSignal && isCorrect;

        //反応時間表示
        float reaction = CDM.GetCurrentReactionTime();
        reaction = MathF.Round(reaction * 100f) / 100f;
        if (isPerfect)
        {
            //  Perfect 命中
            if (isP1) p2Hp -= 100;
            else p1Hp -= 100;
            CDM.reactionText.gameObject.SetActive(true);
            Perfect.text = "PERFECT!!";
            CDM.reactionText.text = $"{reaction:0.00}s";
        }

        else if (damageOpponent)
        {
            if (isP1) p2Hp -= 50;
            else p1Hp -= 50;
            CDM.reactionText.gameObject.SetActive(true);
            CDM.UIText.text = $"{playerName} HIT!";
            CDM.reactionText.text = $"{reaction:0.00}s";
            audioSource.Play();
        }
        else if (damageSelf)
        {
            if (isP1) p1Hp -= 50;
            else p2Hp -= 50;
            CDM.UIText.text = $"{playerName} MISS!";
            
            audioSource .Play();
            
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
            Invoke(nameof(StartNewRound), 2f);
        }

        //Debug.Log($"P1 HP: {p1Hp}, P2 HP: {p2Hp}");


    }
    //何秒経過後、ボタン押されていない場合、引分
    private IEnumerator WaitForTimeout()
    {
        yield return new WaitForSeconds(1f);
        if (!roundEnded && string.IsNullOrEmpty(firstPlayerPressed))
        {
            CDM.StopTimer();
            CDM.canInput = false;
            CDM.StopLoop();
            CDM.signalText.text = "";
            yield return new WaitForSeconds(0.5f);
            CDM.UIText.text = "DRAW";
            Invoke(nameof(StartNewRound), 2f);
            roundEnded = true;
        }
    }



    private void EndGame()
    {
        CDM.signalText.text = null;
        string winner;

        if (p1Hp <= 0)
        {
            winner = "P2";
            CDM.UIText.rectTransform.anchoredPosition = new Vector2(688, -348);
        }
        else
        {
            winner = "P1";
            CDM.UIText.rectTransform.anchoredPosition = new Vector2(-688, -348);
        }

        CDM.UIText.text = $"{winner} WIN!";

        Invoke(nameof(ToGameover), 2f);
    }
    void ToGameover()
    {
        SceneManager.LoadScene("EndingScene");
    }
}
