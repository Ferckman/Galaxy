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

    [Header("Prefabs")]
    [SerializeField] private GameObject[] _normalEnemyPrefabs; 
    [SerializeField] private GameObject[] _tankEnemyPrefabs;

    [Header("Formation Settings")]
    [SerializeField] private float _rowSpawnY = 4.5f;              
    [SerializeField] private float _spacingBetweenEnemies = 2.5f; 
    [SerializeField] private float _rowSpacingY = 2.5f;

    /// <summary>
    /// Spawnea la oleada en filas, los baraja, los coloca en grid y crea el FormationController para moverlos.
    /// </summary>
    public IEnumerator SpawnWaveInRowRoutine(WaveManager.Wave wave, List<GameObject> blockEnemiesList, System.Action<FormationController> onFormationCreated)
    {
        if (_normalEnemyPrefabs.Length == 0 && _tankEnemyPrefabs.Length == 0) yield break;

        Camera cam = Camera.main;
        if (cam == null) yield break;

        float camAspect = cam.aspect;
        float screenHeight = cam.orthographicSize;
        float screenWidth = screenHeight * camAspect;

        // Comprobamos la dirección usando el Enum limpio
        bool comeFromBottom = (wave.spawnDirection == EnemySpawnDirection.Bottom);
        bool comeFromLeft = (wave.spawnDirection == EnemySpawnDirection.Left);
        bool comeFromRight = (wave.spawnDirection == EnemySpawnDirection.Right);

        // Posiciones base fuera de la pantalla según la dirección elegida
        float baseOffScreenY = comeFromBottom ? (-screenHeight - 2.0f) : (screenHeight + 2.0f);
        float baseOffScreenX = comeFromLeft ? (-screenWidth - 3.0f) : (screenWidth + 3.0f);

        List<Enemy> currentWaveEnemyScripts = new List<Enemy>();
        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        // 1. Creamos la lista temporal de todos los prefabs a spawnear
        List<GameObject> rowPrefabsToSpawn = new List<GameObject>();

        for (int i = 0; i < wave.normalEnemyCount; i++)
        {
            if (_normalEnemyPrefabs.Length > 0)
            {
                rowPrefabsToSpawn.Add(_normalEnemyPrefabs[Random.Range(0, _normalEnemyPrefabs.Length)]);
            }
        }

        for (int i = 0; i < wave.tankEnemyCount; i++)
        {
            if (_tankEnemyPrefabs.Length > 0)
            {
                rowPrefabsToSpawn.Add(_tankEnemyPrefabs[Random.Range(0, _tankEnemyPrefabs.Length)]);
            }
        }

        // 2. Barajamos la lista completa (Fisher-Yates) para mezclar normales y tanques
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
        int prefabIndex = 0;

        for (int r = 0; r < numRows; r++)
        {
            int enemiesInThisRow = Mathf.Min(enemiesPerBaseRow, totalEnemies - prefabIndex);
            if (enemiesInThisRow <= 0) break;

            float totalWidth = (enemiesInThisRow - 1) * _spacingBetweenEnemies;
            float startX = -totalWidth / 2f;
            
            float currentTargetY = _rowSpawnY - (r * _rowSpacingY);

            for (int i = 0; i < enemiesInThisRow; i++)
            {
                if (blockEnemiesList.Count >= MaxEnemies) break;
                if (prefabIndex >= totalEnemies) break;

                float posX = startX + (i * _spacingBetweenEnemies);
                Vector3 targetPosition = new Vector3(posX, currentTargetY, 0f);
                Vector3 spawnPos;

                // Cálculo de la posición inicial según la dirección de entrada
                if (comeFromLeft || comeFromRight)
                {
                    float offScreenX = comeFromLeft ? (baseOffScreenX - (r * 1.5f)) : (baseOffScreenX + (r * 1.5f));
                    spawnPos = new Vector3(offScreenX, currentTargetY, 0f);
                }
                else if (comeFromBottom)
                {
                    float invR = numRows - 1 - r;
                    float currentOffScreenY = baseOffScreenY - (invR * _rowSpacingY);
                    currentTargetY = _rowSpawnY - (invR * _rowSpacingY); // Reajuste de destino ordenado
                    targetPosition = new Vector3(posX, currentTargetY, 0f);
                    spawnPos = new Vector3(posX, currentOffScreenY, 0f);
                }
                else
                {
                    // Por defecto: Top (Arriba)
                    float currentOffScreenY = baseOffScreenY - (r * _rowSpacingY);
                    spawnPos = new Vector3(posX, currentOffScreenY, 0f);
                }

                GameObject selectedPrefab = rowPrefabsToSpawn[prefabIndex];
                prefabIndex++;

                if (selectedPrefab != null)
                {
                    GameObject enemyObj = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, container);
                    blockEnemiesList.Add(enemyObj);

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

        // 4. Creamos el bloque de formación global utilizando tu FormationController
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