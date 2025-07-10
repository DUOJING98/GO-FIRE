using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountDownManager : MonoBehaviour
{
    float timeCnt, signalTimeCnt, timeInterval;
    public GameObject signalTextObj, gameOverTextObj, player1, player2;
    Text signalText;
    bool startShowSignal, initTimeInterval;
    [HideInInspector]
    public bool canPress, gameOver;
    string[] signals = new string[] { "DOG", "START", "WAIT", "HOLD" };
    [HideInInspector]
    public string signal;

    private void Start()
    {
        signalText = signalTextObj.GetComponent<Text>();
        signal = signals[Random.Range(0, 4)];
        timeInterval = Random.Range(1f, 5.0f);
    }
    private void Update()
    {
        if (gameOver)
        {
            signalText.text = "";
            gameOverTextObj.SetActive(true);
            gameOverTextObj.GetComponent<Text>().text = player1.GetComponent<Player>().hp <= 0 ? "PLAYER2WIN!" : "PLAYER1WIN";
            Time.timeScale = 0;
            return;
        }
        timeCnt += Time.deltaTime;
        //dao ji shi
        if (timeCnt >= 0 && timeCnt < 1)
        {
            signalText.text = "3";
        }
        else if (timeCnt < 2)
        {
            signalText.text = "2";
        }
        else if (timeCnt < 3)
        {
            signalText.text = "1";
        }
        else
        {
            showSignal();
        }
    }
    void showSignal()
    {
        signalTimeCnt += Time.deltaTime;
        if (signalTimeCnt > timeInterval + 1f)
        {
            initTimeInterval = false;
        }
        if (signalTimeCnt >= timeInterval && signalTimeCnt <= timeInterval + 1f)
        {
            signalText.text = signal;
            canPress = true;
        }
        else
        {
            signalText.text = "";
            canPress = false;
            //xin hao xiao shi shun jian ,chong xin sui ji xin hao
            if (!initTimeInterval)
            {
                signal = signals[Random.Range(0, 4)];
                timeInterval = Random.Range(1f, 5.0f);
                initTimeInterval = true;
                signalTimeCnt = 0;
            }
        }
    }
}
