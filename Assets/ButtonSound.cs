using UnityEngine;
using UnityEngine.EventSystems;


public class ButtonSelectSound : MonoBehaviour, ISelectHandler
{
    public AudioSource audioSource;

    public void OnSelect(BaseEventData eventData)
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}

