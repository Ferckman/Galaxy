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
        _pool.RegisterAndPrewarm(_missileAmmoPrefab, _initialMissileAmmoCount);

        foreach (GameObject prefab in _generalPowerUpPrefabs)
        {
            _pool.RegisterAndPrewarm(prefab, _initialPerPowerUpCount);
        }
    }

    public GameObject SpawnMissileAmmo(Vector3 position) =>
        _pool.Spawn(_missileAmmoPrefab, position, Quaternion.identity);

    public GameObject SpawnRandomGeneralPowerUp(Vector3 position)
    {
        if (_generalPowerUpPrefabs == null || _generalPowerUpPrefabs.Count == 0) return null;

        GameObject chosenPrefab = _generalPowerUpPrefabs[Random.Range(0, _generalPowerUpPrefabs.Count)];
        return _pool.Spawn(chosenPrefab, position, Quaternion.identity);
    }

    public void ReturnToPool(GameObject instance) =>
        _pool.Return(instance);
}