using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashDuration = 0.1f;
    public AudioSource damageSE;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void TakeDamage()
    {

        StartCoroutine(Flash());
        damageSE.Play();
    }

    private System.Collections.IEnumerator Flash()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}
