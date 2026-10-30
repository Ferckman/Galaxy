using System.Collections;
using UnityEngine;

public class WarningIndicator : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(float duration)
    {
        StartCoroutine(FlashAndRemove(duration));
    }

    private IEnumerator FlashAndRemove(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (_spriteRenderer != null) 
            {
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(0.15f);
            timer += 0.15f;
        }
        Destroy(gameObject);
    }
}