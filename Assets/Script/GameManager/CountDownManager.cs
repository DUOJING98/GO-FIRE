using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountDownManager : MonoBehaviour
{
    public float minDelay = 1f;
    public float maxDelay = 3f;

    public UnityEvent onReadyStart;
    public UnityEvent onGoSignal;
    public UnityEvent<string> onFakeSignal;


    private void Start()
    {
        StartCoroutine(nameof(CountdownRoutine));
    }

    IEnumerator CountdownRoutine()
    {
        yield return ShowNumber("3");
    }


    IEnumerator ShowNumber(string number)
    {
        yield return new WaitForSeconds(1f);
    }
}
