using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int player1Hp, player2Hp, player1PerfectTimes, player2PerfectTimes, player1RPSWinTimes, player2RPSWinTimes;
    private float player1AverageReaction, player2AverageReaction;
    public Text player1HpText, player2HpText, player1PerfectTimesText, player2PerfectTimesText
        , player1RPSWinTimesText, player2RPSWinTimesText
        , player1AverageReactionText, player2AverageReactionText;
    void Start()
    {
        player1Hp = PlayerPrefs.GetInt("player1Hp", 0);
        player2Hp = PlayerPrefs.GetInt("player2Hp", 0);
        player1PerfectTimes = PlayerPrefs.GetInt("player1PerfectTimes", 0);
        player2PerfectTimes = PlayerPrefs.GetInt("player2PerfectTimes", 0);
        player1RPSWinTimes = PlayerPrefs.GetInt("player1RPSWinTimes", 0);
        player2RPSWinTimes = PlayerPrefs.GetInt("player2RPSWinTimes", 0);
        player1AverageReaction = PlayerPrefs.GetFloat("player1AverageReaction");
        player2AverageReaction = PlayerPrefs.GetFloat("player2AverageReaction");


        player1HpText.text = "HP: " + player1Hp;
        player2HpText.text = "HP: " + player2Hp;
        player1PerfectTimesText.text = "PERFECT: " + player1PerfectTimes;
        player2PerfectTimesText.text = "PERFECT: " + player2PerfectTimes;
        player1RPSWinTimesText.text = "RPSWIN: " + player1RPSWinTimes;
        player2RPSWinTimesText.text = "RPSWIN: " + player2RPSWinTimes;
        player1AverageReactionText.text = "AVERAGE: " + player1AverageReaction.ToString("F3") + " S";
        player2AverageReactionText.text = "AVERAGE: " + player2AverageReaction.ToString("F3") + " S";
    }


    void Update()
    {

    }
}
