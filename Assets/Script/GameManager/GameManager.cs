using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CountDownManager CDM;

    private bool canPress = false;
    private bool isRealGO = false;
    private bool gameEnded = false;


    private void Start()
    {
        CDM.onGoSignal.AddListener(HandleGO);
        CDM.onGoSignal.AddListener(HandleFake);
    }

    void HandleGO()
    {
        canPress = true;
        isRealGO = true;
    }

    void HandleFake()
    {
        canPress = true;
        isRealGO = false;
    }

    public void PlayerPressed(string playerName)
    {
        if (gameEnded) return;
        if (!canPress)
        {
            gameEnded = true;
            CDM.signalText.text = $"{playerName} 早押し！失格！";
            return;
        }

        if(isRealGO)
        {
            gameEnded = true;
            CDM.signalText.text = $"{playerName} 勝利！";
        }
        else
        {
            gameEnded = true;
            CDM.signalText.text = $"{playerName} 間違い！失格！";

        }

    }

}
