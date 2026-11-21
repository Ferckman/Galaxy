using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [Header("Player Laser")]
    [SerializeField] private GameObject _playerLaserPrefab;
    [SerializeField] private int _initialPlayerLaserCount = 40;

    [Header("Enemy Laser")]
    [SerializeField] private GameObject _enemyLaserPrefab;
    [SerializeField] private int _initialEnemyLaserCount = 80;

    [Header("Missile")]
    [SerializeField] private GameObject _missilePrefab;
    [SerializeField] private int _initialMissileCount = 20;

    private PrefabQueuePool _pool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _pool = new PrefabQueuePool(transform);
        _pool.RegisterAndPrewarm(_playerLaserPrefab, _initialPlayerLaserCount);
        _pool.RegisterAndPrewarm(_enemyLaserPrefab, _initialEnemyLaserCount);
        _pool.RegisterAndPrewarm(_missilePrefab, _initialMissileCount);
    }

    public GameObject SpawnPlayerLaser(Vector3 position, Quaternion rotation) =>
        _pool.Spawn(_playerLaserPrefab, position, rotation);

    public GameObject SpawnEnemyLaser(Vector3 position, Quaternion rotation) =>
        _pool.Spawn(_enemyLaserPrefab, position, rotation);

    public GameObject SpawnMissile(Vector3 position, Quaternion rotation) =>
        _pool.Spawn(_missilePrefab, position, rotation);

    public void ReturnToPool(GameObject instance) =>
        _pool.Return(instance);
}