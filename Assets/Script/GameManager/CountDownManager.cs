using System.Collections;
using Unity.VisualScripting;
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
    public bool canInput = false;
    //タイマー要
    public float timerValue = 0f;
    private float TimeStartTime = 0f;

    public Coroutine UpdateTimerCoroutine, CountdownRoutineCoroutine, SignalLoopCoroutine, timerCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        UIText.text = " ";
    }

    private void Update()
    {

    }

    //计时器协程
    public void StartUpdateTimer()
    {
        StopCoroutine(nameof(UpdateTimer));
        TimeStartTime = Time.time;
        //timerValue = 0.10f;
        timerText.text = "0.000s"; //  归零
        UpdateTimerCoroutine = StartCoroutine(nameof(UpdateTimer));
    }

    public void StopUpdateTimer()
    {
        StopCoroutine(nameof(UpdateTimer));
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
    }

    public void StopCountdown()
    {
        StopCoroutine(nameof(CountdownRoutine));
    }

    IEnumerator CountdownRoutine()
    {
        signalText.text = "3";
        yield return new WaitForSeconds(1f);
        signalText.text = "2";
        yield return new WaitForSeconds(1f);
        signalText.text = "1";
        yield return new WaitForSeconds(1f);
        signalText.text = "READY...";
        onReadyStart?.Invoke();
        yield return new WaitForSeconds(1f);
        ClearText();
        //yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        StartCoroutine(nameof(SignalLoop));
    }

    public IEnumerator SignalLoop()
    {
        Debug.Log("start loop");
        canInput = true;
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
                //GameManager.currentIsRealSignal = true;
                if (audioSource != null && goClip != null)
                {
                    audioSource.PlayOneShot(goClip);
                }
            }
            else
            {
                string[] fakeSignals = { "WAIT!", "DOG!", "START!", " ", " ", " " };
                string fake = fakeSignals[Random.Range(0, fakeSignals.Length)];
                signalText.text = fake;
                onFakeSignal?.Invoke();
                isRealSignal = false;
                if (audioSource != null && fakeClip != null && !string.IsNullOrWhiteSpace(fake))
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
