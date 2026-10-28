using System.Collections;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private float _timeLife = 10f;

    private SpriteRenderer _spriteRenderer;
    private Player _player;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _player = Object.FindFirstObjectByType<Player>();

        if (_player == null) Debug.LogError("Player component not found in the scene!", this);

        StartCoroutine(BlinkRoutine());
    }

    private void Update()
    {
        CheckTimeLife();
    }

    public void SetTimeLife(float timeLife)
    {
        _timeLife = timeLife;
    }

    private void CheckTimeLife()
    {
        _timeLife -= Time.deltaTime;
        if (_timeLife <= 0f)
        {
            DisableShield();
            Destroy(gameObject);
        }
    }

    private void DisableShield()
    {
        if (_player != null) _player.ShieldActivated = false;
    }

    public void WeakenShield()
    {
        _timeLife -= 3.0f;
    }

    private IEnumerator BlinkRoutine()
    {
        WaitForSeconds waitTime = new WaitForSeconds(0.2f);

        while (true)
        {
            if (_timeLife <= 10.0f && _timeLife > 0f && _spriteRenderer != null) _spriteRenderer.enabled = !_spriteRenderer.enabled;

            yield return waitTime;
        }
    }
}