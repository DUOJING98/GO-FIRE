using UnityEngine;
using UnityEngine.UI;

public class CountDownManager : MonoBehaviour
{
    public float countdownTime = 3f;
    public Text countdownText;
    public static bool canAct=false;


    private void Start()
    {
        canAct = false;
        StartCoroutine(nameof(CountdownRoutine);
    }

    private System.Collections.IEnumerator CountdownRoutine()
    {

    }
}
