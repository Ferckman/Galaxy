using UnityEngine;
using System.Collections.Generic;

public class MissileSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _missilePrefab;

    private List<Transform> _leftSpawnPoints = new List<Transform>();
    private List<Transform> _rightSpawnPoints = new List<Transform>();

    public void FireMissileSalvo()
    {
        if (_missilePrefab == null) return;

        List<Transform> activeTargets = FindActiveTargets();
        if (activeTargets.Count == 0) return;

        Transform leftTarget = activeTargets[0];
        Transform rightTarget = activeTargets.Count > 1 ? activeTargets[1] : activeTargets[0];

        SpawnPair(_leftSpawnPoints, leftTarget);
        SpawnPair(_rightSpawnPoints, rightTarget);
    }

    private void SpawnPair(List<Transform> spawnPoints, Transform target)
    {
        if (spawnPoints == null || target == null) return;

        foreach (Transform sp in spawnPoints)
        {
            if (sp == null) continue;

            GameObject missileInstance = Instantiate(_missilePrefab, sp.position, sp.rotation);
            
            if (missileInstance.TryGetComponent<Missile>(out Missile missileScript))
            {
                IMissileGuidance arcStrategy = new SmoothArcGuidance();
                missileScript.Initialize(target, arcStrategy);
            }
        }
    }

    private List<Transform> FindActiveTargets()
    {
        List<Transform> validTargets = new List<Transform>();

        Obstacles[] foundObstacles = FindObjectsByType<Obstacles>(FindObjectsSortMode.None);

        foreach (Obstacles obstacle in foundObstacles)
        {
            if (obstacle != null && obstacle.gameObject.activeInHierarchy)
            {
                validTargets.Add(obstacle.transform);
            }
        }

        return validTargets;
    }

    private void Awake()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child == transform) continue;

            if (child.name.Contains("Spawn") || child.name.Contains("Missile"))
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
}