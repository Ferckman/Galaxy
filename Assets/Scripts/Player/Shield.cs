using System.Collections;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private float _timeLife;

    private SpriteRenderer _spriteRenderer;
    private Spaceship spaceship;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        spaceship = Object.FindFirstObjectByType<Spaceship>();

        if (spaceship == null) Debug.LogError("Spaceship component not found in the scene!", this);

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
        if (spaceship != null) spaceship.ShieldActivated = false;
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