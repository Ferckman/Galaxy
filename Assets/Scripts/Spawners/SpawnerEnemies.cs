using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEnemies : Spawner
{
    private const int MaxEnemies = 60;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] _enemyPrefabs;
    
    [Header("Warnings")]
    [SerializeField] private GameObject _warningIndicatorPrefab;

    private List<GameObject> _enemies = new List<GameObject>();

    protected override List<GameObject> list 
    { 
        get => _enemies; 
        set => _enemies = value; 
    }

    private void Awake()
    {
        FindGameManager();
        StartCoroutine(SpawnRoutineEnemies());
    }

    private IEnumerator SpawnRoutineEnemies()
    {
        yield return new WaitForSeconds(1f);

        while (gameManager != null && gameManager._isGameStarted)
        {
            if (_enemies.Count < MaxEnemies && _enemyPrefabs.Length > 0)
            {
                Camera cam = Camera.main;
                if (cam == null) yield break;

                float spawnY = cam.orthographicSize + 1.0f; 

                Vector3 leftLimit = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
                Vector3 rightLimit = cam.ViewportToWorldPoint(new Vector3(1f, 0f, cam.nearClipPlane));

                float margin = 1.5f; 
                float randomX = Random.Range(leftLimit.x + margin, rightLimit.x - margin);
                Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

                int randomIndex = Random.Range(0, _enemyPrefabs.Length);
                GameObject selectedEnemyPrefab = _enemyPrefabs[randomIndex];

                if (selectedEnemyPrefab == null)
                {
                    yield return new WaitForSeconds(Cooldown);
                    continue;
                }

                if (selectedEnemyPrefab.name.Contains("Kamikaze"))
                {
                    if (_warningIndicatorPrefab != null)
                    {
                        Vector3 warningPos = new Vector3(randomX, spawnY - 1.5f, 0f);
                        GameObject warning = Instantiate(_warningIndicatorPrefab, warningPos, Quaternion.identity);
                        
                        WarningIndicator indicatorScript = warning.GetComponent<WarningIndicator>();
                        if (indicatorScript != null) indicatorScript.Initialize(1.0f);
                    }

                    yield return new WaitForSeconds(1.0f); 
                }

                GameObject enemy = Instantiate(selectedEnemyPrefab, spawnPos, Quaternion.identity);
                
                if (enemy != null) _enemies.Add(enemy);
            }

            yield return new WaitForSeconds(Cooldown);
        }
    }
}