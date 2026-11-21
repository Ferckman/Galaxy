using System.Collections.Generic;
using UnityEngine;

public class MissileSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _missilePrefab;

    [Header("Missile Economy & Cooldown")]
    [SerializeField] private int _maxMissileAmmo;
    [SerializeField] private int _currentMissileAmmo;
    [SerializeField] private float _bigFireRate;
    private float _canBigFire = 0f;

    private List<Transform> _leftSpawnPoints = new List<Transform>();
    private List<Transform> _rightSpawnPoints = new List<Transform>();

    private void Awake()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child == transform) continue;

            if (child.name.Contains("Spawn"))
            {
                if (child.localPosition.x < 0f)
                {
                    _leftSpawnPoints.Add(child);
                }
                else
                {
                    _rightSpawnPoints.Add(child);
                }
            }
        }
    }

    public bool TryFireSalvo()
    {
        if (Time.time < _canBigFire) return false;

        if (_currentMissileAmmo <= 0)
        {
            Debug.Log("¡Sin munición de misiles!");
            return false;
        }

        List<Transform> activeTargets = FindActiveTargets();
        if (activeTargets.Count == 0) return false;

        int missilesFired = FireMissileSalvoLogic(activeTargets);

        _currentMissileAmmo -= missilesFired;
        _canBigFire = Time.time + _bigFireRate;

        Debug.Log($"¡Salva lanzada! Misiles gastados: {missilesFired}. Munición restante: {_currentMissileAmmo}");
        return true;
    }

    public void AddAmmo(int amount)
    {
        _currentMissileAmmo += amount;
        _currentMissileAmmo = Mathf.Clamp(_currentMissileAmmo, 0, _maxMissileAmmo);
        Debug.Log("¡Munición recargada! Total: " + _currentMissileAmmo);
    }

    public bool TryAddAmmo(int amount)
    {
        if (_currentMissileAmmo >= _maxMissileAmmo)
        {
            return false;
        }

        _currentMissileAmmo = Mathf.Min(_currentMissileAmmo + amount, _maxMissileAmmo);
        return true;
    }

    private int FireMissileSalvoLogic(List<Transform> activeTargets)
    {
        if (ProjectilePool.Instance == null && _missilePrefab == null) return 0;

        List<Transform> leftSideTargets = activeTargets.FindAll(t => t.position.x < transform.position.x);
        List<Transform> rightSideTargets = activeTargets.FindAll(t => t.position.x >= transform.position.x);

        leftSideTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));
        rightSideTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));

        Transform leftTarget = null;
        Transform rightTarget = null;

        if (leftSideTargets.Count > 0)
        {
            leftTarget = leftSideTargets[0];
        }
        else
        {
            activeTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));
            leftTarget = activeTargets[0];
        }

        if (rightSideTargets.Count > 0)
        {
            rightTarget = rightSideTargets[0];
        }
        else
        {
            activeTargets.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));
            rightTarget = activeTargets[0];
        }

        if (activeTargets.Count == 1)
        {
            leftTarget = activeTargets[0];
            rightTarget = activeTargets[0];
        }

        int leftFired = SpawnPair(_leftSpawnPoints, leftTarget);
        int rightFired = SpawnPair(_rightSpawnPoints, rightTarget);

        return leftFired + rightFired;
    }

    private int SpawnPair(List<Transform> spawnPoints, Transform target)
    {
        if (spawnPoints == null || target == null) return 0;

        int count = 0;

        foreach (Transform sp in spawnPoints)
        {
            if (sp == null) continue;

            GameObject missileInstance = null;

            if (ProjectilePool.Instance != null)
            {
                missileInstance = ProjectilePool.Instance.SpawnMissile(sp.position, sp.rotation);
            }
            else if (_missilePrefab != null)
            {
                missileInstance = Instantiate(_missilePrefab, sp.position, sp.rotation);
            }

            if (missileInstance != null && missileInstance.TryGetComponent<Missile>(out Missile missileScript))
            {
                IMissileGuidance arcStrategy = new SmoothArcGuidance();
                missileScript.Initialize(target, arcStrategy);
            }

            count++;
        }

        return count;
    }

    private List<Transform> FindActiveTargets()
    {
        List<Transform> validTargets = new List<Transform>();
        Entity[] foundEntities = FindObjectsByType<Entity>(FindObjectsSortMode.None);

        foreach (Entity entity in foundEntities)
        {
            if (entity != null && entity.gameObject.activeInHierarchy)
            {
                validTargets.Add(entity.transform);
            }
        }

        return validTargets;
    }
}