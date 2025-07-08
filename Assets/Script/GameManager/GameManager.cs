using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CountDownManager CDM;

    public Push p1;
    public Push p2;

    [SerializeField]private int p1Hp =100,p2Hp =100;
    [SerializeField]private int p1Wins=0,p2Wins=0;
    private bool roundEnded = false;

    private void Start()
    {
        CDM.onGoSignal.AddListener(() => StartRound(true));
        CDM.onGoSignal.AddListener(()=>StartRound(false));
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
        if(roundEnded) return;
        roundEnded = true;

        if (playerName == "P1"&&isCorrect)
        {
            p2Hp -= 50;
        }
        else if(playerName =="P2"&&isCorrect)
        {
            p1Hp -= 50;
        }

        if(p1Wins==2||p2Wins==2)
        {
            EndGame();
        }
        else
        {
            Invoke(nameof(StartNewRound), 1f);
        }
    }

    private void EndGame()
    {
        string winner = p1Wins == 2 ? "P1" : "P2";
        CDM.signalText.text = $"{winner} èüóòÅI";
    }
}
