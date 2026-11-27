using UnityEngine;

public class WarningPool : MonoBehaviour
{
    public static WarningPool Instance { get; private set; }

    [SerializeField] private GameObject _warningPrefab;
    [SerializeField] private int _initialCount = 5;

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
        _pool.RegisterAndPrewarm(_warningPrefab, _initialCount);
    }

    public GameObject SpawnWarning(Vector3 position, Quaternion rotation) =>
        _pool.Spawn(_warningPrefab, position, rotation);

    public void ReturnToPool(GameObject instance) =>
        _pool.Return(instance);
}