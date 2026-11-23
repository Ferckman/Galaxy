using System.Collections.Generic;
using UnityEngine;

public class EntityPool : MonoBehaviour
{
    public static EntityPool Instance { get; private set; }

    [System.Serializable]
    public class PoolConfig
    {
        public Entity prefab;
        public int initialSize = 10;
    }

    [Header("Configuración Inicial del Pool")]
    [SerializeField] private List<PoolConfig> _initialPools = new List<PoolConfig>();

    private readonly Dictionary<Entity, Queue<Entity>> _pools = new Dictionary<Entity, Queue<Entity>>();
    private readonly Dictionary<Entity, Entity> _instanceToPrefab = new Dictionary<Entity, Entity>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PrewarmPools();
    }

    private void PrewarmPools()
    {
        foreach (PoolConfig config in _initialPools)
        {
            if (config.prefab == null) continue;
            EnsurePool(config.prefab);

            for (int i = 0; i < config.initialSize; i++)
            {
                Entity instance = CreateNewInstance(config.prefab);
                instance.gameObject.SetActive(false);
                _pools[config.prefab].Enqueue(instance);
            }
        }
    }

    private void EnsurePool(Entity prefab)
    {
        if (!_pools.ContainsKey(prefab))
        {
            _pools[prefab] = new Queue<Entity>();
        }
    }

    private Entity CreateNewInstance(Entity prefab)
    {
        Entity instance = Instantiate(prefab, transform);
        _instanceToPrefab[instance] = prefab;
        return instance;
    }

    public Entity Get(Entity prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        EnsurePool(prefab);

        Queue<Entity> queue = _pools[prefab];
        Entity instance = null;

        while (queue.Count > 0 && instance == null)
        {
            instance = queue.Dequeue();
        }

        if (instance == null)
        {
            instance = CreateNewInstance(prefab);
        }

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.gameObject.SetActive(true);
        instance.OnSpawnedFromPool();

        return instance;
    }

    public void ReturnToPool(Entity instance)
    {
        if (instance == null) return;

        instance.OnReturnedToPool();
        instance.gameObject.SetActive(false);

        if (_instanceToPrefab.TryGetValue(instance, out Entity prefab) && _pools.ContainsKey(prefab))
        {
            _pools[prefab].Enqueue(instance);
        }
        else
        {
            Destroy(instance.gameObject);
        }
    }
}