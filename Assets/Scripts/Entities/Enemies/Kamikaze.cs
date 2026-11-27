using UnityEngine;

public class Kamikaze : Enemy
{
    [Header("Kamikaze Settings")]
    [SerializeField] private float _explosionScaleMultiplier = 1.5f;

    private bool _isDiving = true;

    public override void OnSpawnedFromPool()
    {
        base.OnSpawnedFromPool();
        _isDiving = true;
    }

    protected override void BehaviorLogic()
    {
        if (_isDiving)
        {
            transform.Translate(Vector3.down * _moveSpeed * Time.deltaTime);
        }
    }

    protected override void SpawnExplosion()
    {
        if (_explosionPrefab != null)
        {
            Vector3 explosionPos = transform.position + new Vector3(0f, 0f, Random.Range(-0.1f, -0.5f));

            GameObject explosion = ExplosionPool.Instance != null
                ? ExplosionPool.Instance.SpawnExplosion(explosionPos, Quaternion.identity)
                : Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);

            if (explosion != null)
            {
                explosion.transform.localScale *= _explosionScaleMultiplier;

                if (explosion.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
                {
                    sr.sortingOrder = 10;
                }
            }
        }

        DropLoot();
    }

    protected override bool ShouldDestroyOnPlayerHit
    {
        get
        {
            Spaceship spaceship = FindFirstObjectByType<Spaceship>();
            return spaceship != null && spaceship.ShieldActivated;
        }
    }

    protected override void DropLoot()
    {
        float roll = Random.Range(0f, 100f);

        if (roll < 50f)
        {
            if (LootPool.Instance != null)
                LootPool.Instance.SpawnMissileAmmo(transform.position);
            else if (_missileAmmoPrefab != null)
                Instantiate(_missileAmmoPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            if (LootPool.Instance != null)
                LootPool.Instance.SpawnRandomGeneralPowerUp(transform.position);
            else
                SpawnRandomGeneralPowerUp();
        }
    }
}