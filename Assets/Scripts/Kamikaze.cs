using UnityEngine;

public class Kamikaze : Enemy
{
    [Header("Kamikaze Explosion Settings")]
    [SerializeField] private float _explosionScaleMultiplier;

    protected override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    protected override void SpawnExplosion()
    {
        if (_explosionPrefab != null) 
        {
            GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale *= _explosionScaleMultiplier;
        }
    }

    protected override bool ShouldDestroyOnPlayerHit
    {
        get
        {
            Player player = Object.FindAnyObjectByType<Player>();
            return player != null && player.ShieldActivated;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Obstacles>(out var obstacle))
        {
            if (obstacle.gameObject != this.gameObject)
            {
                Debug.Log("El Kamikaze ha arrollado a: " + other.gameObject.name);
                obstacle.DestroyMyself();
                return;
            }
        }

        base.OnTriggerEnter2D(other);
    }
}