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
            CDM.signalText.text = $"{playerName} �������I���i�I";
            return;
        }

        if(isRealGO)
        {
            gameEnded = true;
            CDM.signalText.text = $"{playerName} �����I";
        }
        else
        {
            gameEnded = true;
            CDM.signalText.text = $"{playerName} �ԈႢ�I���i�I";

        }

    }

}
