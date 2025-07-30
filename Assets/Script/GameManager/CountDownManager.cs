using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountDownManager : MonoBehaviour
{
    public float minDelay = 2f;
    public float maxDelay = 6f;
    public float signalInterval = 1f;

    public Text signalText;     //信号
    public Text UIText;         //UI文字
    public Text timerText;      //タイマー
    public Text reactionText;   //タイマー

    public UnityEvent onReadyStart;
    public UnityEvent onGoSignal;
    public UnityEvent onFakeSignal;

    //SE信号
    [Header("SE")]
    [SerializeField] AudioClip goClip;
    [SerializeField] AudioClip fakeClip;
    private AudioSource audioSource;
    [Header("チェック")]
    public bool hasGoAppeared = false;
    public bool isRealSignal = false;
    public bool canInput = true;
    //タイマー要
    private Coroutine timerCoroutine;
    public float timerValue = 0f;
    private float TimeStartTime=0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        UIText.text = " ";
    }

    //计时器协程
    public void StartTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        TimeStartTime = Time.time;
        //timerValue = 0.10f;
        timerText.text = "0.00s"; //  归零
        timerCoroutine = StartCoroutine(UpdateTimer());
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private IEnumerator UpdateTimer()
    {
        while (true)
        {
            float elapsed = Time.time - TimeStartTime;
            timerValue = elapsed;
            timerText.text = elapsed.ToString("0.00");
            yield return null;
            //timerValue += 0.10f;
        }
    }


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
        yield return new WaitForSeconds(1f);
        ClearText();

        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        StartCoroutine(nameof(SignalLoop));
        
    }

    public IEnumerator SignalLoop()
    {
        while (!hasGoAppeared)
        {
            float delay = Random.Range(2, 3);
            yield return new WaitForSeconds(delay);
            int rand = Random.Range(0, 3);//0:fake,1:Go!
            if (rand == 0)
            {
                signalText.text = "GO!";
                onGoSignal?.Invoke();
                isRealSignal = true;
                hasGoAppeared = true;
                if (audioSource != null && goClip != null)
                {
                    audioSource.PlayOneShot(goClip);
                }
            }
            else
            {
                string[] fakeSignals = { "WAIT!", "DOG!", "START!"," "," "," " };
                string fake = fakeSignals[Random.Range(0, fakeSignals.Length)];
                signalText.text = fake;
                onFakeSignal?.Invoke();
                isRealSignal = false;
                if (audioSource != null && fakeClip != null&&!string.IsNullOrWhiteSpace(fake))
                {
                    audioSource.PlayOneShot(fakeClip);
                }
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
        UIText.text = " ";
    }

    public float GetCurrentReactionTime()
    {
        return timerValue;
    }

}
