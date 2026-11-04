using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEnemies : MonoBehaviour
{
    private const int MaxEnemies = 60;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] _normalEnemyPrefabs; 
    [SerializeField] private GameObject[] _tankEnemyPrefabs;
    [SerializeField] private GameObject _kamikazePrefab;       
    [SerializeField] private GameObject _warningIndicatorPrefab;

    [Header("Asteroid Prefabs")]
    [SerializeField] private GameObject _asteroidPrefab;

    [Header("Formation Settings")]
    [SerializeField] private float _rowSpawnY;              
    [SerializeField] private float _spacingBetweenEnemies; 
    [SerializeField] private float _rowSpacingY;

    private List<GameObject> _blockEnemies = new List<GameObject>();
    private List<GameObject> _kamikazeEnemies = new List<GameObject>();
    private List<GameObject> _asteroidEnemies = new List<GameObject>();

    private bool _isWaveSpawningInProgress = false;
    private int _kamikazesRemainingToSpawn = 0;

    /*private List<GameObject> list 
    { 
        get => _blockEnemies; 
        set => _blockEnemies = value; 
    }*/

    public IEnumerator RunCompleteWaveSequenceRoutine(WaveManager.Wave wave)
    {
        gameObject.SetActive(true);

        _isWaveSpawningInProgress = true;
        _blockEnemies.Clear();
        _kamikazeEnemies.Clear();

        // 1. Lanzamos las naves normales y tanques en fila pasando ambos contadores
        //yield return StartCoroutine(SpawnWaveInRowRoutine(wave.normalEnemyCount, wave.tankEnemyCount));
        yield return StartCoroutine(SpawnWaveInRowRoutine(wave)); // O pásale la wave entera a la rutina de la fila

        // 2. Programamos los kamikazes si la oleada los activa
        if (wave.enableKamikazes && wave.kamikazeCount > 0)
        {
            StartCoroutine(SpawnKamikazesOverTimeRoutine(
                wave.kamikazeCount, 
                wave.kamikazeSpawnInterval, 
                wave.minKamikazesPerSpawn, 
                wave.maxKamikazesPerSpawn
            ));
        }
        else
        {
            _isWaveSpawningInProgress = false;
            _kamikazesRemainingToSpawn = 0;
        }

        if (wave.enableAsteroids && wave.asteroidCount > 0)
        {
            StartCoroutine(SpawnAsteroidsRoutine(wave));
        }

        // 3. Esperamos a que terminen de salir todos los kamikazes programados
        yield return new WaitUntil(() => !_isWaveSpawningInProgress && _kamikazesRemainingToSpawn <= 0);

        // 4. Esperamos a que no quede ningún enemigo vivo en pantalla (bloque + kamikazes)
        yield return new WaitUntil(() => GetActiveEnemyCount() == 0);
    }

    public IEnumerator SpawnWaveInRowRoutine(WaveManager.Wave wave)
    {
        // Si ambos arrays están vacíos, salimos
        if (_normalEnemyPrefabs.Length == 0 && _tankEnemyPrefabs.Length == 0) yield break;

        Camera cam = Camera.main;
        if (cam == null) yield break;

        float offScreenY = cam.orthographicSize + 2.0f;
        List<Enemy> currentWaveEnemyScripts = new List<Enemy>();
        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        // 1. Creamos la lista temporal de todos los prefabs a spawnear
        List<GameObject> rowPrefabsToSpawn = new List<GameObject>();

        for (int i = 0; i < wave.normalEnemyCount; i++)
        {
            if (_normalEnemyPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, _normalEnemyPrefabs.Length);
                rowPrefabsToSpawn.Add(_normalEnemyPrefabs[randomIndex]);
            }
        }

        for (int i = 0; i < wave.tankEnemyCount; i++)
        {
            if (_tankEnemyPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, _tankEnemyPrefabs.Length);
                rowPrefabsToSpawn.Add(_tankEnemyPrefabs[randomIndex]);
            }
        }

        // 2. Barajamos la lista completa (Fisher-Yates) para que los tanques y normales queden mezclados
        for (int i = 0; i < rowPrefabsToSpawn.Count; i++)
        {
            GameObject temp = rowPrefabsToSpawn[i];
            int randomIndex = Random.Range(i, rowPrefabsToSpawn.Count);
            rowPrefabsToSpawn[i] = rowPrefabsToSpawn[randomIndex];
            rowPrefabsToSpawn[randomIndex] = temp;
        }

        // 3. Distribuimos los enemigos en múltiples filas (Grid)
        int numRows = Mathf.Max(1, wave.rowCount);
        int totalEnemies = rowPrefabsToSpawn.Count;
        int enemiesPerBaseRow = Mathf.CeilToInt((float)totalEnemies / numRows);
        //float spacingY = 1.4f; // Distancia vertical entre cada fila
        int prefabIndex = 0;

        for (int r = 0; r < numRows; r++)
        {
            // Calculamos cuántos enemigos van en esta fila concreta
            int enemiesInThisRow = Mathf.Min(enemiesPerBaseRow, totalEnemies - prefabIndex);
            if (enemiesInThisRow <= 0) break;

            float totalWidth = (enemiesInThisRow - 1) * _spacingBetweenEnemies;
            float startX = -totalWidth / 2f;
            
            // Posición Y de destino y de entrada (fuera de pantalla) para esta fila específica
            //float currentTargetY = _rowSpawnY - (r * spacingY);
            //float currentOffScreenY = offScreenY + (r * spacingY);
            float currentTargetY = _rowSpawnY - (r * _rowSpacingY);
            float currentOffScreenY = offScreenY - (r * _rowSpacingY);

            for (int i = 0; i < enemiesInThisRow; i++)
            {
                if (_blockEnemies.Count >= MaxEnemies) break;
                if (prefabIndex >= totalEnemies) break;

                float posX = startX + (i * _spacingBetweenEnemies);
                Vector3 targetPosition = new Vector3(posX, currentTargetY, 0f);
                Vector3 spawnPos = new Vector3(posX, currentOffScreenY, 0f);

                GameObject selectedPrefab = rowPrefabsToSpawn[prefabIndex];
                prefabIndex++;

                if (selectedPrefab != null)
                {
                    GameObject enemyObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, container);
                    _blockEnemies.Add(enemyObj);

                    if (enemyObj.TryGetComponent<Enemy>(out Enemy enemyScript))
                    {
                        enemyScript.SetupFormation(targetPosition);
                        currentWaveEnemyScripts.Add(enemyScript);
                    }
                }

                yield return new WaitForSeconds(0.02f);
            }
        }

        yield return new WaitForSeconds(1.5f); 

        // 4. Creamos el bloque de formación global que moverá a todos los enemigos como una unidad
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
            
            controller.Initialize(activeEnemiesForBlock, 2.5f, 7f, wave.useSineWave);
        }
    }

    private IEnumerator SpawnKamikazesOverTimeRoutine(int totalCount, float interval, int minBatch, int maxBatch)
    {
        _isWaveSpawningInProgress = true;
        _kamikazesRemainingToSpawn = totalCount;

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

        int spawnedCount = 0;

        while (spawnedCount < totalCount)
        {
            if (_blockEnemies.Count + _kamikazeEnemies.Count >= MaxEnemies) break;

            // Decidimos cuántos kamikazes saldrán en este lote (respetando el rango y los que faltan)
            int batchSize = Random.Range(minBatch, maxBatch + 1);
            batchSize = Mathf.Min(batchSize, totalCount - spawnedCount);

            // Lista para almacenar las posiciones X de este lote y asegurar la separación
            List<float> batchXPositions = new List<float>();
            float minSeparation = 4f; // Distancia mínima obligatoria entre kamikazes simultáneos

            for (int b = 0; b < batchSize; b++)
            {
                float randomX = 0f;
                bool validPosition = false;
                int attempts = 0;

                // Buscamos una posición X que no esté demasiado cerca de otro kamikaze de este mismo lote
                while (!validPosition && attempts < 15)
                {
                    randomX = Random.Range(leftLimit.x + margin, rightLimit.x - margin);
                    validPosition = true;

                    foreach (float existingX in batchXPositions)
                    {
                        if (Mathf.Abs(randomX - existingX) < minSeparation)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                    attempts++;
                }

                batchXPositions.Add(randomX);

                // 1. Mostramos la advertencia visual para cada uno en su posición separada
                if (_warningIndicatorPrefab != null)
                {
                    Vector3 warningPos = new Vector3(randomX, spawnY - 1.5f, 0f);
                    GameObject warning = Instantiate(_warningIndicatorPrefab, warningPos, Quaternion.identity);
                    
                    WarningIndicator indicatorScript = warning.GetComponent<WarningIndicator>();
                    if (indicatorScript != null) indicatorScript.Initialize(1.0f);
                }
            }

            // Pausa dramática para que el jugador procese la alerta de todo el lote
            yield return new WaitForSeconds(1.0f); 

            // 2. Instanciamos físicamente todos los kamikazes de este lote
            foreach (float posX in batchXPositions)
            {
                Vector3 spawnPos = new Vector3(posX, spawnY, 0f);
                GameObject kamikazeObj = Instantiate(_kamikazePrefab, spawnPos, Quaternion.identity, container);
                
                if (kamikazeObj != null)
                {
                    _kamikazeEnemies.Add(kamikazeObj);
                }

                _kamikazesRemainingToSpawn--;
                spawnedCount++;
            }

            // Si aún quedan kamikazes por salir en la oleada, esperamos el intervalo configurado
            if (spawnedCount < totalCount)
            {
                yield return new WaitForSeconds(interval);
            }
        }

        _isWaveSpawningInProgress = false; 
    }

    private IEnumerator SpawnAsteroidsRoutine(WaveManager.Wave wave)
    {
        if (_asteroidPrefab == null) yield break;

        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        yield return new WaitForSeconds(1.0f);

        int spawnedAsteroids = 0;
        while (spawnedAsteroids < wave.asteroidCount)
        {
            // Lo instanciamos en Vector3.zero; el propio asteroide se colocará en su borde aleatorio al inicializarse
            GameObject asteroidObj = Instantiate(_asteroidPrefab, Vector3.zero, Quaternion.identity, container);

            if (asteroidObj != null)
            {
                _asteroidEnemies.Add(asteroidObj);
            }

            if (asteroidObj.TryGetComponent<Asteroid>(out Asteroid asteroidScript))
            {
                float randomScale = Random.Range(wave.minAsteroidScale, wave.maxAsteroidScale);
                asteroidScript.Initialize(randomScale);
            }

            spawnedAsteroids++;

            if (spawnedAsteroids < wave.asteroidCount)
            {
                yield return new WaitForSeconds(wave.asteroidSpawnInterval);
            }
        }
    }

    public int GetActiveEnemyCount()
    {
        _blockEnemies.RemoveAll(enemy => enemy == null);
        _kamikazeEnemies.RemoveAll(enemy => enemy == null);
        _asteroidEnemies.RemoveAll(enemy => enemy == null); // <--- NUEVO
        
        return _blockEnemies.Count + _kamikazeEnemies.Count + _asteroidEnemies.Count; // <--- INCLUIDO EN LA SUMA
    }
}