using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountDownManager : MonoBehaviour
{
    public float minDelay = 1f;
    public float maxDelay = 3f;

    public Text signalText;

    public UnityEvent onReadyStart;
    public UnityEvent onGoSignal;
    public UnityEvent onFakeSignal;


    private void Start()
    {
        StartCoroutine(nameof(CountdownRoutine));
    }

    IEnumerator CountdownRoutine()
    {
        yield return ShowNumber("3");
        yield return ShowNumber("2");
        yield return ShowNumber("1");
        signalText.text = "èÄîı...";
        onReadyStart.Invoke();
        
        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

        int rand = Random.Range(0, 2);//0:fake,1:Go!
        if(rand == 0)
        {
            string[] fakeSignals = { "WAIT!", "DOG!", "START!", "READY!", "HOLD!" };
            string fake = fakeSignals[Random.Range(0, fakeSignals.Length)];
            signalText.text = fake;
            onFakeSignal.Invoke();
        }
        else
        {
            signalText.text = "GO!";
            onGoSignal.Invoke();
        }
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
