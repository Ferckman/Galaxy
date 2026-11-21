using System.Collections.Generic;
using UnityEngine;

public class LootPool : MonoBehaviour
{
    public static LootPool Instance { get; private set; }

    [Header("Missile Ammo Pool")]
    [SerializeField] private GameObject _missileAmmoPrefab;
    [SerializeField] private int _initialMissileAmmoCount = 12;

    [Header("General Power-Ups Pool")]
    [SerializeField] private List<GameObject> _generalPowerUpPrefabs = new List<GameObject>();
    [SerializeField] private int _initialPerPowerUpCount = 5;

    private readonly Queue<GameObject> _missileAmmoPool = new Queue<GameObject>();
    private readonly Dictionary<GameObject, Queue<GameObject>> _powerUpPools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new Dictionary<GameObject, GameObject>();

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
        if (_missileAmmoPrefab != null)
        {
            PrewarmQueue(_missileAmmoPrefab, _missileAmmoPool, _initialMissileAmmoCount);
        }

        foreach (var prefab in _generalPowerUpPrefabs)
        {
            if (prefab == null) continue;

            if (!_powerUpPools.ContainsKey(prefab))
            {
                _powerUpPools[prefab] = new Queue<GameObject>();
            }

            PrewarmQueue(prefab, _powerUpPools[prefab], _initialPerPowerUpCount);
        }
    }

    private void PrewarmQueue(GameObject prefab, Queue<GameObject> queue, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            _instanceToPrefab[obj] = prefab;
            queue.Enqueue(obj);
        }
    }

    public GameObject SpawnMissileAmmo(Vector3 position)
    {
        if (_missileAmmoPrefab == null) return null;
        return GetFromQueue(_missileAmmoPrefab, _missileAmmoPool, position);
    }

    public GameObject SpawnRandomGeneralPowerUp(Vector3 position)
    {
        if (_generalPowerUpPrefabs == null || _generalPowerUpPrefabs.Count == 0) return null;

        GameObject chosenPrefab = _generalPowerUpPrefabs[Random.Range(0, _generalPowerUpPrefabs.Count)];
        if (chosenPrefab == null) return null;

        if (!_powerUpPools.ContainsKey(chosenPrefab))
        {
            _powerUpPools[chosenPrefab] = new Queue<GameObject>();
        }

        return GetFromQueue(chosenPrefab, _powerUpPools[chosenPrefab], position);
    }

    private GameObject GetFromQueue(GameObject prefab, Queue<GameObject> queue, Vector3 position)
    {
        GameObject obj = null;

        while (queue.Count > 0 && obj == null)
        {
            obj = queue.Dequeue();
        }

        if (obj == null)
        {
            obj = Instantiate(prefab, transform);
            _instanceToPrefab[obj] = prefab;
        }

        obj.transform.SetPositionAndRotation(position, Quaternion.identity);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject instance)
    {
        if (instance == null) return;

        instance.SetActive(false);
        instance.transform.SetParent(transform);

        if (_instanceToPrefab.TryGetValue(instance, out GameObject prefab))
        {
            if (prefab == _missileAmmoPrefab)
            {
                _missileAmmoPool.Enqueue(instance);
                return;
            }

            if (_powerUpPools.TryGetValue(prefab, out Queue<GameObject> queue))
            {
                queue.Enqueue(instance);
                return;
            }
        }

        Destroy(instance);
    }
}