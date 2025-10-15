using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Flash1 : MonoBehaviour
{
    public Image flashImage;

    [Header("Timing")]
    public float riseTime = 0.03f;
    public float fallTime = 0.12f;
    [Range(0f, 1f)] public float PeakAlpha = 1f;





    private void Awake()
    {
        if (!flashImage) { enabled = false; return; }
        var c = flashImage.color; c.a = 0f; flashImage.color = c;
        flashImage.raycastTarget = false;
    }


    public void TriggerFlash()
    {
        StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        float t = 0f;
        while (t < riseTime)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, PeakAlpha, t / riseTime));
            yield return null;
        }
        SetAlpha(PeakAlpha);

        t = 0f;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(PeakAlpha, 0f, t / fallTime));
            yield return null;
        }
        SetAlpha(0f);

    }

    private void SetAlpha(float v)
    {
        var c = flashImage.color;
        c.a = v;
        flashImage.color = c;
    }
}
