using System;
using System.Collections;
using UnityEngine;

public class WarningIndicator : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Coroutine _flashRoutine;
    private Action _onFinished;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(float duration, Action onFinished = null)
    {
        _onFinished = onFinished;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }

        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
        }

        _flashRoutine = StartCoroutine(FlashAndFinish(duration));
    }

    private IEnumerator FlashAndFinish(float duration)
    {
        float timer = 0f;
        float blinkInterval = 0.15f;

        while (timer < duration)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }

        _flashRoutine = null;
        _onFinished?.Invoke();

        if (WarningPool.Instance != null)
        {
            WarningPool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
            _flashRoutine = null;
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }
    }
}