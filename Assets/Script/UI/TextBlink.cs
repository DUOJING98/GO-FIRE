using UnityEngine;
using UnityEngine.UI;

public class TextBlink : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] float blinkTime = 0.5f;

    private float timer;

    private void Update()
    {
           if(text == null) return;


        timer += Time.deltaTime;
        if(timer >= blinkTime)
        {
            text.enabled=!text.enabled;
            timer = 0;
        }

    }
}
