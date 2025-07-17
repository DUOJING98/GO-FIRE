using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CountDownManager CDM;

    public Push p1;
    public Push p2;

    [SerializeField] private int p1Hp = 100, p2Hp = 100;

    private bool roundEnded = false;
    private bool currentIsRealSignal = false;
    private Coroutine signalLoopCoroutine;
    private string firstPlayerPressed = null; // 最初にボタンを押したプレイヤー
    public string FirstPlayerPressed => firstPlayerPressed;

    private void Start()
    {
        CDM.onGoSignal.AddListener(() =>
        {
            currentIsRealSignal = true;
            StartRound(true);
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
        firstPlayerPressed = null;
        p1.ResetRound();
        p2.ResetRound();
        CDM.canInput = true;
        CDM.StartCountdown();
        roundEnded = false;
    }

    void StartRound(bool isRealGo)
    {
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

        // ダメージ処理
        bool isP1 = playerName == "P1";
        bool damageOpponent = currentIsRealSignal && isCorrect;
        bool damageSelf = !currentIsRealSignal && isCorrect;

        if (damageOpponent)
        {
            if (isP1) p2Hp -= 50;
            else p1Hp -= 50;
        }
        else if (damageSelf)
        {
            if (isP1) p1Hp -= 50;
            else p2Hp -= 50;
        }

        roundEnded = true;

        if (p1Hp <= 0 || p2Hp <= 0)
        {
            EndGame();
        }
        else
        {
            Invoke(nameof(StartNewRound), 1f);
        }

        Debug.Log($"P1 HP: {p1Hp}, P2 HP: {p2Hp}");


    }

    private void EndGame()
    {
        string winner;
        if (p1Hp <= 0 && p2Hp <= 0)
        {
            winner = "引分";  // 両者のHPが同時にゼロになったときは引き分け
        }
        else if (p1Hp <= 0)
        {
            winner = "P2";
        }
        else
        {
            winner = "P1";
        }
        CDM.signalText.text = $"{winner} WIN!";
    }
}
