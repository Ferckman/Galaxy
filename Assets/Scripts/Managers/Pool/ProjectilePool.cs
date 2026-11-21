using System.Collections.Generic;
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

    private readonly Queue<GameObject> _playerLaserPool = new Queue<GameObject>();
    private readonly Queue<GameObject> _enemyLaserPool = new Queue<GameObject>();
    private readonly Dictionary<GameObject, Queue<GameObject>> _instanceToPool = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePools();
    }

    private void InitializePools()
    {
        if (_playerLaserPrefab != null)
            PrewarmQueue(_playerLaserPrefab, _playerLaserPool, _initialPlayerLaserCount);

        if (_enemyLaserPrefab != null)
            PrewarmQueue(_enemyLaserPrefab, _enemyLaserPool, _initialEnemyLaserCount);
    }

    private void PrewarmQueue(GameObject prefab, Queue<GameObject> queue, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            _instanceToPool[obj] = queue;
            queue.Enqueue(obj);
        }
    }

    public GameObject SpawnPlayerLaser(Vector3 position, Quaternion rotation)
    {
        if (_playerLaserPrefab == null) return null;
        return GetFromQueue(_playerLaserPrefab, _playerLaserPool, position, rotation);
    }

    public GameObject SpawnEnemyLaser(Vector3 position, Quaternion rotation)
    {
        if (_enemyLaserPrefab == null) return null;
        return GetFromQueue(_enemyLaserPrefab, _enemyLaserPool, position, rotation);
    }

    private GameObject GetFromQueue(GameObject prefab, Queue<GameObject> queue, Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;

        while (queue.Count > 0 && obj == null)
        {
            obj = queue.Dequeue();
        }

        if (obj == null)
        {
            obj = Instantiate(prefab, transform);
            _instanceToPool[obj] = queue;
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject instance)
    {
        if (instance == null || !instance.activeSelf) return;

        instance.transform.SetParent(transform, false);
        instance.SetActive(false);

        if (_instanceToPool.TryGetValue(instance, out Queue<GameObject> queue))
        {
            queue.Enqueue(instance);
            return;
        }

        Destroy(instance);
    }
}