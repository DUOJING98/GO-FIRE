using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountDownManager : MonoBehaviour
{
    public float minDelay = 1f;
    public float maxDelay = 3f;
    public float signalInterval = 1f;

    public Text signalText;

    public UnityEvent onReadyStart;
    public UnityEvent onGoSignal;
    public UnityEvent onFakeSignal;

    private bool hasGoAppeared = false;
    public bool isRealSignal = false;
    public bool canInput = true;




    public void StartCountdown()
    {
        StartCoroutine(nameof(CountdownRoutine));
        hasGoAppeared = false;
    }

    IEnumerator CountdownRoutine()
    {
        yield return ShowNumber("3");
        yield return ShowNumber("2");
        yield return ShowNumber("1");
        signalText.text = "READY...";
        onReadyStart?.Invoke();

        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

        StartCoroutine(nameof(SignalLoop));

    }

    public IEnumerator SignalLoop()
    {
        while (!hasGoAppeared)
        {
            yield return new WaitForSeconds(signalInterval);
            int rand = Random.Range(0, 3);//0:fake,1:Go!
            if (rand == 0)
            {
                signalText.text = "GO!";
                onGoSignal?.Invoke();
                isRealSignal = true;
                hasGoAppeared = true;

            }
            else
            {
                string[] fakeSignals = { "WAIT!", "DOG!", "START!", "HOLD!" };
                string fake = fakeSignals[Random.Range(0, fakeSignals.Length)];
                signalText.text = fake;
                onFakeSignal?.Invoke();
                isRealSignal = false;

            }

        }
    }

    public void StopLoop()
    {
        StopCoroutine(nameof(SignalLoop));
    }

    IEnumerator ShowNumber(string number)
    {
        signalText.text = number;
        yield return new WaitForSeconds(1f);
    }

    public void ClearText()
    {
        signalText.text = " ";
    }
}
