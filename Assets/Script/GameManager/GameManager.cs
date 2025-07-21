using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public CountDownManager CDM;

    [SerializeField] Push p1;
    [SerializeField] Push p2;
    [SerializeField] HealthBar p1HPBar;
    [SerializeField] HealthBar p2HPBar;


    [SerializeField] private int p1Hp = 100, p2Hp = 100;

    private bool roundEnded = false;
    private bool currentIsRealSignal = false;
    private Coroutine signalLoopCoroutine;
    private string firstPlayerPressed = null; // 最初にボタンを押したプレイヤー
    public string FirstPlayerPressed => firstPlayerPressed;

    private Coroutine timeoutCoroutine;     //タイムアウトの検査

    private void Start()
    {
        CDM.onGoSignal.AddListener(() =>
        {
            currentIsRealSignal = true;
            StartRound(true);

            timeoutCoroutine = StartCoroutine(WaitForTimeout());
        });

        CDM.onFakeSignal.AddListener(() =>
        {
            currentIsRealSignal = false;
            StartRound(false);
        });
        StartNewRound();

    }

    private void StartNewRound()
    {
        CDM.UIText.text=" ";
        firstPlayerPressed = null;
        p1.ResetRound();
        p2.ResetRound();
        CDM.canInput = false;
        CDM.StartCountdown();
        roundEnded = false;
        //StartRound(false );
        
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
        if (roundEnded ||
            !CDM.canInput ||
            firstPlayerPressed != null) return; // ラウンド終了、入力不可、または既に入力済みの場合は無視
        firstPlayerPressed = playerName;
        CDM.canInput = false;
        CDM.StopLoop();

        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        // ダメージ処理
        bool isP1 = playerName == "P1";
        bool damageOpponent = currentIsRealSignal && isCorrect;
        bool damageSelf = !currentIsRealSignal && isCorrect;

        if (damageOpponent)
        {
            if (isP1) p2Hp -= 50;
            else p1Hp -= 50;
            p1HPBar.setHP(p1Hp);
            p2HPBar.setHP(p2Hp);
        }
        else if (damageSelf)
        {
            if (isP1) p1Hp -= 50;
            else p2Hp -= 50;
            p1HPBar.setHP(p1Hp);
            p2HPBar.setHP(p2Hp);
        }

        roundEnded = true;

        if (p1Hp <= 0 || p2Hp <= 0)
        {
            EndGame();
        }
        else
        {
            Invoke(nameof(StartNewRound), 2f);
        }

        Debug.Log($"P1 HP: {p1Hp}, P2 HP: {p2Hp}");


    }
    //何秒経過後、ボタン押されていない場合、引分
    private IEnumerator WaitForTimeout()
    {
        yield return new WaitForSeconds(1f);
        if (!roundEnded && string.IsNullOrEmpty(firstPlayerPressed))
        {
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
        string winner;
        //if (p1Hp <= 0 && p2Hp <= 0)
        //{
        //    winner = "引分";  // 両者のHPが同時にゼロになったときは引き分け
        //}
        if (p1Hp <= 0)
        {
            winner = "P2";
        }
        else
        {
            winner = "P1";
        }
        CDM.UIText.text = $"{winner} WIN!";
    }
}
