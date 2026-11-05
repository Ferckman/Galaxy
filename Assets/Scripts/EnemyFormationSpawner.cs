using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemySpawnDirection
{
    Top,
    Bottom,
    Left,
    Right
}

public class EnemyFormationSpawner : MonoBehaviour
{
    private const int MaxEnemies = 60;

    [Header("Prefabs de Enemigos")]
    [SerializeField] private GameObject[] _normalEnemyPrefabs; 
    [SerializeField] private GameObject[] _tankEnemyPrefabs;

    [Header("Configuración de Formación (Top / Bottom)")]
    [SerializeField] private float _rowSpawnY = 4.5f;              
    [SerializeField] private float _spacingBetweenEnemies = 2.5f; 
    [SerializeField] private float _rowSpacingY = 2.5f;

    [Header("Configuración de Formación (Left / Right)")]
    [SerializeField] private float _columnSpacingX = 2.0f; // Profundidad escalonada entre filas laterales

    /// <summary>
    /// Gestiona estrictamente la creación del grid de la oleada y devuelve el controlador de formación.
    /// </summary>
    public IEnumerator SpawnWaveInRowRoutine(WaveManager.Wave wave, List<GameObject> blockEnemiesList, System.Action<FormationController> onFormationCreated)
    {
        if (_normalEnemyPrefabs.Length == 0 && _tankEnemyPrefabs.Length == 0) yield break;

        Camera cam = Camera.main;
        if (cam == null) yield break;

        float camAspect = cam.aspect;
        float screenHeight = cam.orthographicSize;
        float screenWidth = screenHeight * camAspect;

        // 1. Recopilar y barajar los enemigos de la oleada (Normales + Tanques)
        List<GameObject> rowPrefabsToSpawn = new List<GameObject>();

        for (int i = 0; i < wave.normalEnemyCount; i++)
            if (_normalEnemyPrefabs.Length > 0) rowPrefabsToSpawn.Add(_normalEnemyPrefabs[Random.Range(0, _normalEnemyPrefabs.Length)]);

        for (int i = 0; i < wave.tankEnemyCount; i++)
            if (_tankEnemyPrefabs.Length > 0) rowPrefabsToSpawn.Add(_tankEnemyPrefabs[Random.Range(0, _tankEnemyPrefabs.Length)]);

        // Barajado Fisher-Yates
        for (int i = 0; i < rowPrefabsToSpawn.Count; i++)
        {
            GameObject temp = rowPrefabsToSpawn[i];
            int randomIndex = Random.Range(i, rowPrefabsToSpawn.Count);
            rowPrefabsToSpawn[i] = rowPrefabsToSpawn[randomIndex];
            rowPrefabsToSpawn[randomIndex] = temp;
        }

        List<Enemy> currentWaveEnemyScripts = new List<Enemy>();
        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        int numRows = Mathf.Max(1, wave.rowCount);
        int totalEnemies = rowPrefabsToSpawn.Count;
        int enemiesPerBaseRow = Mathf.CeilToInt((float)totalEnemies / numRows);
        int prefabIndex = 0;

        bool isVerticalEntry = (wave.spawnDirection == EnemySpawnDirection.Top || wave.spawnDirection == EnemySpawnDirection.Bottom);

        // ==========================================
        // CASO A: ENTRADA DESDE ARRIBA O ABAJO (Grid Horizontal Tradicional)
        // ==========================================
        if (isVerticalEntry)
        {
            for (int r = 0; r < numRows; r++)
            {
                int enemiesInThisRow = Mathf.Min(enemiesPerBaseRow, totalEnemies - prefabIndex);
                if (enemiesInThisRow <= 0) break;

                float totalWidth = (enemiesInThisRow - 1) * _spacingBetweenEnemies;
                float startX = -totalWidth / 2f;

                for (int i = 0; i < enemiesInThisRow; i++)
                {
                    if (blockEnemiesList.Count >= MaxEnemies) break;
                    if (prefabIndex >= totalEnemies) break;

                    float posX = startX + (i * _spacingBetweenEnemies);
                    float targetY = _rowSpawnY - (r * _rowSpacingY);
                    Vector3 targetPosition = new Vector3(posX, targetY, 0f);

                    Vector3 spawnPos;
                    if (wave.spawnDirection == EnemySpawnDirection.Bottom)
                    {
                        float invR = numRows - 1 - r;
                        float bottomOffScreenY = -screenHeight - 3.0f;
                        targetY = _rowSpawnY - (invR * _rowSpacingY);
                        targetPosition = new Vector3(posX, targetY, 0f);
                        spawnPos = new Vector3(posX, bottomOffScreenY - (invR * _rowSpacingY), 0f);
                    }
                    else // Top
                    {
                        spawnPos = new Vector3(posX, screenHeight + 3.0f - (r * _rowSpacingY), 0f);
                    }

                    GameObject selectedPrefab = rowPrefabsToSpawn[prefabIndex++];
                    if (selectedPrefab != null)
                    {
                        GameObject enemyObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, container);
                        blockEnemiesList.Add(enemyObj);

                        if (enemyObj.TryGetComponent<Enemy>(out Enemy enemyScript))
                        {
                            enemyScript.SetupFormation(targetPosition, wave.spawnDirection);
                            currentWaveEnemyScripts.Add(enemyScript);
                        }
                    }

                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
        // ==========================================
        // CASO B: ENTRADA DESDE LA IZQUIERDA O DERECHA (Grid Lateral Específico)
        // ==========================================
        else
        {
            for (int r = 0; r < numRows; r++)
            {
                int enemiesInThisRow = Mathf.Min(enemiesPerBaseRow, totalEnemies - prefabIndex);
                if (enemiesInThisRow <= 0) break;

                float totalHeight = (numRows - 1) * _rowSpacingY;
                float startY = (totalHeight / 2f) - (r * _rowSpacingY);

                for (int i = 0; i < enemiesInThisRow; i++)
                {
                    if (blockEnemiesList.Count >= MaxEnemies) break;
                    if (prefabIndex >= totalEnemies) break;

                    float targetX = -((enemiesInThisRow * _spacingBetweenEnemies) / 2f) + (i * _spacingBetweenEnemies);
                    float targetY = _rowSpawnY - startY; 
                    Vector3 targetPosition = new Vector3(targetX, targetY, 0f);

                    Vector3 spawnPos;
                    float offScreenBaseX = wave.spawnDirection == EnemySpawnDirection.Left ? -screenWidth - 4.0f : screenWidth + 4.0f;
                    
                    float depthOffset = i * _columnSpacingX;
                    if (wave.spawnDirection == EnemySpawnDirection.Left)
                        spawnPos = new Vector3(offScreenBaseX - depthOffset, targetY, 0f);
                    else
                        spawnPos = new Vector3(offScreenBaseX + depthOffset, targetY, 0f);

                    GameObject selectedPrefab = rowPrefabsToSpawn[prefabIndex++];
                    if (selectedPrefab != null)
                    {
                        GameObject enemyObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, container);
                        blockEnemiesList.Add(enemyObj);

                        if (enemyObj.TryGetComponent<Enemy>(out Enemy enemyScript))
                        {
                            enemyScript.SetupFormation(targetPosition, wave.spawnDirection);
                            currentWaveEnemyScripts.Add(enemyScript);
                        }
                    }

                    yield return new WaitForSeconds(0.02f);
                }
            }
        }

        yield return new WaitForSeconds(1.5f); 

        // 3. Inicializar y devolver el controlador del bloque de formación global
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
            onFormationCreated?.Invoke(controller);
        }
    }
}