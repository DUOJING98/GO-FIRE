using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CountDownManager CDM;

    public Push p1;
    public Push p2;

    [SerializeField]private int p1Hp =100,p2Hp =100;
    
    private bool roundEnded = false;
    private bool currentIsRealSignal = false;

    private void Start()
    {
        CDM.onGoSignal.AddListener(() => {
            currentIsRealSignal = true;
            StartRound(true);
        });

        CDM.onFakeSignal.AddListener(() => {
            currentIsRealSignal = false;
            StartRound(true);
        });
        StartNewRound();

    }

    private void StartNewRound()
    {
        p1.RestRound();
        p2.RestRound();
        CDM.StartCountdown();
        roundEnded = false;
    }

    void StartRound(bool isRealGo)
    {
        roundEnded = false;
        p1.BeginRound(isRealGo);
        p2.BeginRound(isRealGo);
    }

    public void PlayerPressed(string playerName,bool isCorrect)
    {
        Debug.Log($"PlayerPressed called with playerName={playerName}, isCorrect={isCorrect}");
        if (roundEnded) return;
        CDM.StopCoroutine("SignalLoop");
        if (currentIsRealSignal)
        {
            if (playerName == "P1"&& isCorrect)
                p2Hp -= 50;
            else if(playerName =="P2"&& isCorrect)
                p1Hp -= 50;
        }
        else
        {
            if(playerName == "P1" && isCorrect)
                p1Hp -= 50;
            else if (playerName == "P2" && isCorrect)
                p2Hp -= 50;
        }


        if (p1Hp<=0||p2Hp<=0)
        {
            EndGame();
        }
        else
        {
            Invoke(nameof(StartNewRound), 1f);
        }
        Debug.Log($"P1 HP: {p1Hp}, P2 HP: {p2Hp}");
        roundEnded = true;
    }

    private void EndGame()
    {
        string winner;
        if (p1Hp <= 0 && p2Hp <= 0)
        {
            winner = "平手";  // 両者のHPが同時にゼロになったときは引き分け
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
