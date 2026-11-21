using UnityEngine;

public class ExplosionPool : MonoBehaviour
{
    public static ExplosionPool Instance { get; private set; }

    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private int _initialCount = 20;

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
        _pool.RegisterAndPrewarm(_explosionPrefab, _initialCount);
    }

    public GameObject SpawnExplosion(Vector3 position, Quaternion rotation) =>
        _pool.Spawn(_explosionPrefab, position, rotation);

    public void ReturnToPool(GameObject instance) =>
        _pool.Return(instance);
}