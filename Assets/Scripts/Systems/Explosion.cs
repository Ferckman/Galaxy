using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 5.0f;

    private float _timer;

    private void OnEnable()
    {
        _timer = _lifeTime;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            if (ExplosionPool.Instance != null)
            {
                ExplosionPool.Instance.ReturnToPool(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}