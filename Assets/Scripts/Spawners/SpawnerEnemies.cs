using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEnemies : Spawner
{
    private const int MaxEnemies = 60;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] _normalEnemyPrefabs; 
    [SerializeField] private GameObject _kamikazePrefab;       
    [SerializeField] private GameObject _warningIndicatorPrefab;

    [Header("Formation Settings")]
    [SerializeField] private float _rowSpawnY = 4f;              
    [SerializeField] private float _spacingBetweenEnemies = 1.8f; 

    private List<GameObject> _blockEnemies = new List<GameObject>();
    private List<GameObject> _kamikazeEnemies = new List<GameObject>();

    private bool _isWaveSpawningInProgress = false;
    private int _kamikazesRemainingToSpawn = 0;

    protected override List<GameObject> list 
    { 
        get => _blockEnemies; 
        set => _blockEnemies = value; 
    }

    private void Awake()
    {
        FindGameManager();
    }

    public IEnumerator RunCompleteWaveSequenceRoutine(int normalCount, bool enableKamikazes, int kamikazeCount, float kamikazeInterval)
    {
        // SEGURIDAD: Forzamos que el objeto esté activo para evitar errores de StartCoroutine
        gameObject.SetActive(true);

        _isWaveSpawningInProgress = true;
        _blockEnemies.Clear();
        _kamikazeEnemies.Clear();

        // 1. Lanzamos las naves normales en fila
        yield return StartCoroutine(SpawnWaveInRowRoutine(normalCount));

        // 2. Programamos los kamikazes si procede
        if (enableKamikazes && kamikazeCount > 0)
        {
            StartCoroutine(SpawnKamikazesOverTimeRoutine(kamikazeCount, kamikazeInterval));
        }
        else
        {
            _isWaveSpawningInProgress = false;
            _kamikazesRemainingToSpawn = 0;
        }

        // 3. Esperamos a que terminen de spawnear todos los kamikazes de la oleada
        yield return new WaitUntil(() => !_isWaveSpawningInProgress && _kamikazesRemainingToSpawn <= 0);

        // 4. Esperamos a que no quede ningún enemigo vivo en pantalla
        yield return new WaitUntil(() => GetActiveEnemyCount() == 0);
    }

    public IEnumerator SpawnWaveInRowRoutine(int enemyCount)
    {
        if (_normalEnemyPrefabs.Length == 0) yield break;

        Camera cam = Camera.main;
        if (cam == null) yield break;

        float offScreenY = cam.orthographicSize + 2.0f;
        List<Enemy> currentWaveEnemyScripts = new List<Enemy>();
        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        float totalWidth = (enemyCount - 1) * _spacingBetweenEnemies;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < enemyCount; i++)
        {
            if (_blockEnemies.Count >= MaxEnemies) break;

            float posX = startX + (i * _spacingBetweenEnemies);
            Vector3 targetPosition = new Vector3(posX, _rowSpawnY, 0f);
            Vector3 spawnPos = new Vector3(posX, offScreenY, 0f);

            int randomIndex = Random.Range(0, _normalEnemyPrefabs.Length);
            GameObject selectedPrefab = _normalEnemyPrefabs[randomIndex];

            if (selectedPrefab == null) continue;

            GameObject enemyObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, container);
            
            if (enemyObj != null) 
            {
                _blockEnemies.Add(enemyObj);

                if (enemyObj.TryGetComponent<Enemy>(out Enemy enemyScript))
                {
                    enemyScript.SetupFormation(targetPosition);
                    currentWaveEnemyScripts.Add(enemyScript);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1.5f); 

        if (currentWaveEnemyScripts.Count > 0)
        {
            GameObject formationObj = new GameObject("WaveFormationBlock");
            formationObj.transform.position = Vector3.zero;
            FormationController controller = formationObj.AddComponent<FormationController>();
            
            List<Enemy> activeEnemiesForBlock = new List<Enemy>();
            foreach (Enemy enemy in currentWaveEnemyScripts)
            {
                if (enemy != null)
                {
                    enemy.transform.SetParent(formationObj.transform, true);
                    activeEnemiesForBlock.Add(enemy);
                    enemy.ActivateCombatBehavior();
                }
            }
            controller.Initialize(activeEnemiesForBlock, 2.5f, 7f);
        }
    }

    private IEnumerator SpawnKamikazesOverTimeRoutine(int count, float interval)
    {
        _isWaveSpawningInProgress = true;
        _kamikazesRemainingToSpawn = count;

        if (_kamikazePrefab == null) 
        {
            _isWaveSpawningInProgress = false;
            yield break;
        }

        Camera cam = Camera.main;
        if (cam == null) 
        {
            _isWaveSpawningInProgress = false;
            yield break;
        }

        float spawnY = cam.orthographicSize + 1.0f;
        Vector3 leftLimit = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
        Vector3 rightLimit = cam.ViewportToWorldPoint(new Vector3(1f, 0f, cam.nearClipPlane));
        float margin = 1.5f;

        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        yield return new WaitForSeconds(2.0f); 

        for (int i = 0; i < count; i++)
        {
            if (_blockEnemies.Count + _kamikazeEnemies.Count >= MaxEnemies) break;

            float randomX = Random.Range(leftLimit.x + margin, rightLimit.x - margin);
            Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

            if (_warningIndicatorPrefab != null)
            {
                Vector3 warningPos = new Vector3(randomX, spawnY - 1.5f, 0f);
                GameObject warning = Instantiate(_warningIndicatorPrefab, warningPos, Quaternion.identity);
                
                WarningIndicator indicatorScript = warning.GetComponent<WarningIndicator>();
                if (indicatorScript != null) indicatorScript.Initialize(1.0f);
            }

            yield return new WaitForSeconds(1.0f); 

            GameObject kamikazeObj = Instantiate(_kamikazePrefab, spawnPos, Quaternion.identity, container);
            
            if (kamikazeObj != null)
            {
                _kamikazeEnemies.Add(kamikazeObj);
            }

            _kamikazesRemainingToSpawn--; 

            if (i < count - 1)
            {
                yield return new WaitForSeconds(interval);
            }
        }

        _isWaveSpawningInProgress = false; 
    }

    public int GetActiveEnemyCount()
    {
        _blockEnemies.RemoveAll(enemy => enemy == null);
        _kamikazeEnemies.RemoveAll(enemy => enemy == null);
        
        return _blockEnemies.Count + _kamikazeEnemies.Count;
    }
}