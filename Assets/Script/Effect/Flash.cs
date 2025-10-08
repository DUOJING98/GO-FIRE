using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flash : MonoBehaviour
{
    [Header("Light")]
    public Light2D flashLight;
    public float maxIntensity = 1.5f;
    public float radiusAtPeak = 3.5f;
    public Color flashColor = Color.white;

    [Header("Timing")]
    public float riseTime = 0.03f;
    public float fallTime = 0.07f;


    float baseRadius;


    private void Awake()
    {
        if (!flashLight) flashLight = GetComponent<Light2D>();
        if (!flashLight) { enabled = false; return; }
        baseRadius = flashLight.pointLightOuterRadius;
        flashLight.intensity = 0f;
        enabled = false;
    }


    public void TriggerFlash()
    {
        if (!flashLight) return;
        StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        Debug.Log("!!!");
        flashLight.enabled = true;
        flashLight.color = flashColor;

        float t = 0f;
        while (t < riseTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / riseTime);
            flashLight.intensity = Mathf.Lerp(0f, maxIntensity, k);
            flashLight.pointLightOuterRadius = Mathf.Lerp(baseRadius, radiusAtPeak, k);
            yield return null;
        }

        t = 0f;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fallTime);
            flashLight.intensity = Mathf.Lerp(maxIntensity, 0f, k);
            flashLight.pointLightOuterRadius = Mathf.Lerp(radiusAtPeak, baseRadius, k);
            yield return null;
        }
        flashLight.intensity = 0;
        flashLight.enabled = false;
    }
}
