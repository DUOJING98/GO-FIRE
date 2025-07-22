using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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

    private float goSignalTime = -1f;
    private Coroutine timeoutCoroutine;     //タイムアウトの検査

    private void Start()
    {
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
        StartNewRound();

        
    }
    private void StartNewRound()
    {
        CDM.timerText.text = "0.0";
        CDM.UIText.text=" ";
        firstPlayerPressed = null;
        p1.ResetRound();
        p2.ResetRound();
        CDM.canInput = false;
        currentIsRealSignal = false; // 初始化为false，避免意外  
        CDM.StartCountdown();
        roundEnded = false;
        
        
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
            if (timeSinceGo <= 0.2f)
            {
                isPerfect = true;
            }
        }

        bool damageOpponent = currentIsRealSignal && isCorrect;
        bool damageSelf = !currentIsRealSignal && isCorrect;

        if (isPerfect)
        {
            //  Perfect 命中
            if (isP1) p2Hp -= 100;
            else p1Hp -= 100;
            CDM.UIText.text = "PERFECT!!";
        }

        else if (damageOpponent)
        {
            if (isP1) p2Hp -= 50;
            else p1Hp -= 50;
            CDM.UIText.text = $"{playerName} 命中!";
           
        }
        else if (damageSelf)
        {
            if (isP1) p1Hp -= 50;
            else p2Hp -= 50;
            CDM.UIText.text = $"{playerName} ミス!";
            
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
        }
        else
        {
            winner = "P1";
        }
        CDM.UIText.text = $"{winner} WIN!";
    }
}
