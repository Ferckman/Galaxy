/*using System.Collections;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Combat & Shooting")]
    [SerializeField] private GameObject _enemyLaserPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 2f;

    [Header("Movement Base")]
    [SerializeField] protected float _moveSpeed = 6f;

    [Header("Loot Drop Settings")]
    [SerializeField] private protected GameObject _missileAmmoPrefab; // Prefab del power-up de munición
    [SerializeField] private GameObject[] _generalPowerUpPrefabs;    // Prefabs de power-ups (Triple, Speed, Shield)

    [Header("Loot Chances (0 - 100)")]
    [SerializeField] private float _noDropChance = 50f;      // 50%: Nada
    [SerializeField] private float _missileAmmoChance = 35f; // 35%: Munición de misil (total acumulado: 85f)
    // El resto (100 - (_noDropChance + _missileAmmoChance) = 15%) será power-up general

    [SerializeField] private Animator _animator;

    private Coroutine _shootRoutine;

    protected override void Movement()
    {
        BehaviorLogic();
    }

    protected override void Update()
    {
        Movement();
        CheckBounds();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopShooting();
    }

    /// <summary>
    /// Sobrescribible en cada tipo de enemigo para definir su movimiento principal o puntual.
    /// </summary>
    protected virtual void BehaviorLogic()
    {
    }

    public virtual void StartShooting()
    {
        if (ProjectilePool.Instance == null && _enemyLaserPrefab == null) return;
        if (_shootRoutine != null) StopCoroutine(_shootRoutine);
        _shootRoutine = StartCoroutine(ShootRoutine());
    }

    public virtual void StopShooting()
    {
        if (_shootRoutine != null)
        {
            StopCoroutine(_shootRoutine);
            _shootRoutine = null;
        }
    }

    private IEnumerator ShootRoutine()
    {
        // Pequeño retardo aleatorio inicial para que no disparen todos a la vez
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        while (enabled)
        {
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
            GameObject bullet = null;

            if (ProjectilePool.Instance != null)
            {
                bullet = ProjectilePool.Instance.SpawnEnemyLaser(spawnPos, Quaternion.identity);
            }
            else if (_enemyLaserPrefab != null)
            {
                bullet = Instantiate(_enemyLaserPrefab, spawnPos, Quaternion.identity);
            }

            if (bullet != null && bullet.TryGetComponent<Laser>(out Laser laserScript))
            {
                laserScript.SetDirection(Vector3.down);
            }

            yield return new WaitForSeconds(_fireRate);
        }
    }

    public override void DestroyMyself()
    {
        StopShooting();
        base.DestroyMyself();
    }

    protected override void DropLoot()
    {
        float roll = Random.Range(0f, 100f);
        float missileThreshold = _noDropChance + _missileAmmoChance;

        if (roll < _noDropChance) return;
        else if (roll < missileThreshold)
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

    protected virtual void SpawnRandomGeneralPowerUp()
    {
        if (_generalPowerUpPrefabs == null || _generalPowerUpPrefabs.Length == 0) return;

        GameObject chosen = _generalPowerUpPrefabs[Random.Range(0, _generalPowerUpPrefabs.Length)];
        if (chosen != null)
        {
            Instantiate(chosen, transform.position, Quaternion.identity);
        }
    }
}
*/
using System.Collections;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Combat & Shooting")]
    [SerializeField] private GameObject _enemyLaserPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 2f;

    [Header("Movement Base")]
    [SerializeField] protected float _moveSpeed = 6f;

    [Header("Loot Drop Settings")]
    [SerializeField] private protected GameObject _missileAmmoPrefab;
    [SerializeField] private GameObject[] _generalPowerUpPrefabs;

    [Header("Loot Chances (0 - 100)")]
    [SerializeField] private float _noDropChance = 50f;
    [SerializeField] private float _missileAmmoChance = 35f;

    [SerializeField] private Animator _animator;

    private Coroutine _shootRoutine;

    public override void OnReturnedToPool()
    {
        StopShooting();
        base.OnReturnedToPool();
    }

    protected override void Movement()
    {
        BehaviorLogic();
    }

    protected override void Update()
    {
        Movement();
        CheckBounds();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopShooting();
    }

    protected virtual void BehaviorLogic()
    {
    }

    public virtual void StartShooting()
    {
        if (ProjectilePool.Instance == null && _enemyLaserPrefab == null) return;
        if (_shootRoutine != null) StopCoroutine(_shootRoutine);
        _shootRoutine = StartCoroutine(ShootRoutine());
    }

    public virtual void StopShooting()
    {
        if (_shootRoutine != null)
        {
            StopCoroutine(_shootRoutine);
            _shootRoutine = null;
        }
    }

    private IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        while (enabled)
        {
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
            GameObject bullet = null;

            if (ProjectilePool.Instance != null)
            {
                bullet = ProjectilePool.Instance.SpawnEnemyLaser(spawnPos, Quaternion.identity);
            }
            else if (_enemyLaserPrefab != null)
            {
                bullet = Instantiate(_enemyLaserPrefab, spawnPos, Quaternion.identity);
            }

            if (bullet != null && bullet.TryGetComponent<Laser>(out Laser laserScript))
            {
                laserScript.SetDirection(Vector3.down);
            }

            yield return new WaitForSeconds(_fireRate);
        }
    }

    public override void DestroyMyself()
    {
        StopShooting();
        base.DestroyMyself();
    }

    protected override void DropLoot()
    {
        float roll = Random.Range(0f, 100f);
        float missileThreshold = _noDropChance + _missileAmmoChance;

        if (roll < _noDropChance) return;
        else if (roll < missileThreshold)
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

    protected virtual void SpawnRandomGeneralPowerUp()
    {
        if (_generalPowerUpPrefabs == null || _generalPowerUpPrefabs.Length == 0) return;

        GameObject chosen = _generalPowerUpPrefabs[Random.Range(0, _generalPowerUpPrefabs.Length)];
        if (chosen != null)
        {
            Instantiate(chosen, transform.position, Quaternion.identity);
        }
    }
}