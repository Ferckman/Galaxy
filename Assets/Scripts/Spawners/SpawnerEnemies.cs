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

    [Header("Formation Settings")]
    [SerializeField] private float _rowSpawnY = 4f;      // Altura fija donde se forma la fila arriba
    [SerializeField] private float _totalRowWidth = 8f; // Ancho total que ocupará la formación en pantalla

    private List<GameObject> _enemies = new List<GameObject>();

    protected override List<GameObject> list 
    { 
        get => _enemies; 
        set => _enemies = value; 
    }

    private void Awake()
    {
        FindGameManager(); // Tu método existente
    }

    // Método individual por si lo llamas de forma suelta en alguna otra parte
    public void SpawnSingleEnemy()
    {
        StartCoroutine(SpawnSingleEnemyRoutine());
    }

    // Corrutina individual (mantiene tu lógica de kamikazes y advertencias intacta)
    public IEnumerator SpawnSingleEnemyRoutine()
    {
        if (_enemyPrefabs.Length == 0) yield break;
        if (_enemies.Count >= MaxEnemies) yield break;

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

        if (selectedEnemyPrefab == null) yield break;

        // Si es Kamikaze, lanzamos la advertencia y ESPERAMOS antes de instanciarlo
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

        // Obtenemos el contenedor de forma ultra rápida y segura mediante el Singleton
        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        // Instanciamos al enemigo dentro de su contenedor
        GameObject enemy = Instantiate(selectedEnemyPrefab, spawnPos, Quaternion.identity, container);
        
        if (enemy != null) 
        {
            _enemies.Add(enemy);
        }
    }

    public IEnumerator SpawnWaveInRowRoutine(int enemyCount)
    {
        if (_enemyPrefabs.Length == 0) yield break;

        Camera cam = Camera.main;
        if (cam == null) yield break;

        // Altura inicial por encima de la pantalla (fuera de vista)
        float offScreenY = cam.orthographicSize + 2.0f;

        List<Enemy> currentWaveEnemyScripts = new List<Enemy>();
        Transform container = EnemiesContainer.Instance != null ? EnemiesContainer.Instance.transform : null;

        // Calculamos el espacio simétrico entre cada enemigo de la fila
        float spacing = enemyCount > 1 ? _totalRowWidth / (enemyCount - 1) : 0f;
        float startX = -(_totalRowWidth / 2f);

        for (int i = 0; i < enemyCount; i++)
        {
            if (_enemies.Count >= MaxEnemies) break;

            // Posición final donde se detendrán en la fila
            float posX = startX + (i * spacing);
            Vector3 targetPosition = new Vector3(posX, _rowSpawnY, 0f);

            // Posición de nacimiento (arriba del todo, fuera de pantalla)
            Vector3 spawnPos = new Vector3(posX, offScreenY, 0f);

            int randomIndex = Random.Range(0, _enemyPrefabs.Length);
            GameObject selectedEnemyPrefab = _enemyPrefabs[randomIndex];

            if (selectedEnemyPrefab == null) continue;

            // Advertencia visual si el prefab es Kamikaze (la ponemos en su destino final de la fila)
            if (selectedEnemyPrefab.name.Contains("Kamikaze") && _warningIndicatorPrefab != null)
            {
                Vector3 warningPos = new Vector3(posX, _rowSpawnY - 1.5f, 0f);
                GameObject warning = Instantiate(_warningIndicatorPrefab, warningPos, Quaternion.identity);
                
                WarningIndicator indicatorScript = warning.GetComponent<WarningIndicator>();
                if (indicatorScript != null) indicatorScript.Initialize(0.8f);
                
                yield return new WaitForSeconds(0.3f);
            }

            // Instanciamos al enemigo arriba fuera de pantalla
            GameObject enemyObj = Instantiate(selectedEnemyPrefab, spawnPos, Quaternion.identity, container);
            
            if (enemyObj != null) 
            {
                _enemies.Add(enemyObj);

                if (enemyObj.TryGetComponent<Enemy>(out Enemy enemyScript))
                {
                    // Le indicamos a dónde tiene que bajar para formar la fila
                    enemyScript.SetupFormation(targetPosition);
                    currentWaveEnemyScripts.Add(enemyScript);
                }
            }

            // Pequeño retardo escalonado para que no aparezcan todos a la vez de golpe
            yield return new WaitForSeconds(0.1f);
        }

        // Esperamos un momento prudencial a que todos terminen de descender y tomar posiciones en la fila
        yield return new WaitUntil(() => {
            // Opcional: podrías verificar si todos llegaron, pero con una pausa de 1.5s suele bastar para este tamaño
            return true; 
        });
        yield return new WaitForSeconds(1.5f); 

        // Damos la orden global de inicio de combate a toda la formación ya colocada
        foreach (Enemy enemy in currentWaveEnemyScripts)
        {
            if (enemy != null)
            {
                enemy.ActivateCombatBehavior();
            }
        }
    }
    
    public int GetActiveEnemyCount()
    {
        // Limpiamos de la lista los elementos que ya se hayan destruido (null)
        _enemies.RemoveAll(enemy => enemy == null);
        
        // Devolvemos cuántos enemigos siguen vivos en la pantalla
        return _enemies.Count;
    }
}