/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        [Header("Información General")]
        public string waveName = "Oleada 1";

        [Header("1. Composición del Bloque (Filas y Columnas)")]
        [Tooltip("Número de filas en las que se distribuirán los enemigos del bloque.")]
        public int rowCount = 2; // <--- NUEVO: Por defecto 2 filas
        [Tooltip("Cantidad de enemigos comunes en el bloque.")]
        public int normalEnemyCount = 6;
        //[Tooltip("Cantidad de enemigos Tanque (más vida, disparo lento) en el bloque.")]
        //public int tankEnemyCount = 2;
        public bool tanksInFirstRow = false;

        [Header("2. Configuración de Kamikazes")]
        public bool enableKamikazes = false;
        public int kamikazeCount = 3;             
        public float kamikazeSpawnInterval = 4f;  
        public int minKamikazesPerSpawn = 1;
        public int maxKamikazesPerSpawn = 1;

        [Header("Configuración de Asteroides")]
        public bool enableAsteroids = false;
        public int asteroidCount = 4;
        public float asteroidSpawnInterval = 5f;
        [Tooltip("Tamaño mínimo y máximo aleatorio para los asteroides de esta oleada.")]
        public float minAsteroidScale = 0.6f;
        public float maxAsteroidScale = 1.8f;

        [Header("3. Patrón de Movimiento del Bloque")]
        [Tooltip("Si está activo, el bloque se moverá haciendo una onda senoidal vertical mientras va de lado a lado.")]
        public bool useSineWave = false;

        //[Tooltip("Dirección desde donde entrará esta oleada.")]
        //public EnemySpawnDirection spawnDirection = EnemySpawnDirection.Top;

        [Header("3. Eventos Especiales (Opcionales por Oleada)")]
        [Tooltip("¿Esta oleada incluye un Emisor de Drones pesado que baja del cielo?")]
        public bool spawnDroneCarrier = false;
        [Tooltip("¿Esta oleada es un combate contra el Jefe Final?")]
        public bool isBossWave = false;
    }

    [Header("Settings")]
    [SerializeField] private List<Wave> _waves;
    //[SerializeField] private SpawnerEnemies _enemySpawnerPrefab; 

    //private SpawnerEnemies _activeSpawnerInstance; 
    private int _currentWaveIndex = 0;
    private bool _isWaveActive = false;

    public void StartWaves()
    {
        if (_isWaveActive) return; 

        /*if (_activeSpawnerInstance == null && _enemySpawnerPrefab != null)
        {
            GameObject spawnedObj = Instantiate(_enemySpawnerPrefab.gameObject);
            _activeSpawnerInstance = spawnedObj.GetComponent<SpawnerEnemies>();
            _activeSpawnerInstance.gameObject.SetActive(true);
        }

        _currentWaveIndex = 0;
        StartCoroutine(RunWavesSequence());
    }

    private IEnumerator RunWavesSequence()
    {
        while (_currentWaveIndex < _waves.Count)
        {
            Wave currentWave = _waves[_currentWaveIndex];
            _isWaveActive = true;

            /*if (_activeSpawnerInstance != null)
            {
                // AQUÍ ESTABA EL CAMBIO DE NOMBRE: Llamamos a SpawnWaveRoutine (o el nombre que tenga en tu SpawnerEnemies)
                yield return StartCoroutine(_activeSpawnerInstance.SpawnWaveRoutine(currentWave, (formationController) => {
                    // Si pasabas un callback opcional para la formación, lo puedes gestionar aquí
                }));
            }

            _isWaveActive = false;
            
            Debug.Log($"¡Oleada {currentWave.waveName} superada! Dando un respiro...");

            yield return new WaitForSeconds(3f);

            _currentWaveIndex++;
        }

        Debug.Log("¡Todas las oleadas completadas! ¡Victoria!");
    }
}
*/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Serializable]
    public class WaveEntry
    {
        public Entity enemyPrefab;
        public int count = 8;
        public float spawnInterval = 0.2f;
        public float delayBeforeEntry = 0f;
    }

    [Serializable]
    public class WaveDefinition
    {
        public string waveName = "Oleada 1";
        public float spawnY = 23.5f;
        public float delayAfterWave = 2.0f;
        public List<WaveEntry> entries = new List<WaveEntry>();
    }

    [Header("Configuración de Oleadas")]
    [SerializeField] private List<WaveDefinition> _waves = new List<WaveDefinition>();
    [SerializeField] private bool _autoStart = true;

    public int CurrentWaveIndex { get; private set; } = -1;
    public bool IsWaveActive { get; private set; }

    private int _aliveEnemiesCount;

    private void Start()
    {
        if (_autoStart)
        {
            StartWaves();
        }
    }

    public void StartWaves()
    {
        StopAllCoroutines();
        StartCoroutine(RunWavesRoutine());
    }

    private IEnumerator RunWavesRoutine()
    {
        for (int i = 0; i < _waves.Count; i++)
        {
            CurrentWaveIndex = i;
            IsWaveActive = true;
            WaveDefinition wave = _waves[i];

            yield return StartCoroutine(SpawnWaveRoutine(wave));

            while (_aliveEnemiesCount > 0)
            {
                yield return null;
            }

            IsWaveActive = false;

            if (wave.delayAfterWave > 0f)
            {
                yield return new WaitForSeconds(wave.delayAfterWave);
            }
        }
    }

    private IEnumerator SpawnWaveRoutine(WaveDefinition wave)
    {
        foreach (WaveEntry entry in wave.entries)
        {
            if (entry.enemyPrefab == null || EntityPool.Instance == null || EnemySlotManager.Instance == null)
                continue;

            if (entry.delayBeforeEntry > 0f)
            {
                yield return new WaitForSeconds(entry.delayBeforeEntry);
            }

            for (int i = 0; i < entry.count; i++)
            {
                if (!EnemySlotManager.Instance.TryReserveSlot(out Vector3 targetSlot))
                {
                    yield return new WaitForSeconds(0.25f);
                    i--;
                    continue;
                }

                Vector3 spawnPos = new Vector3(targetSlot.x, wave.spawnY, 0f);
                Entity entity = EntityPool.Instance.Get(entry.enemyPrefab, spawnPos, Quaternion.identity);

                if (entity != null)
                {
                    _aliveEnemiesCount++;
                    entity.OnDespawned += HandleEntityDespawned;

                    if (entity.TryGetComponent<GruntEnemy>(out GruntEnemy grunt))
                    {
                        grunt.SetAnchorPoint(targetSlot);
                    }
                }

                if (entry.spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }
        }
    }

    private void HandleEntityDespawned(Entity entity)
    {
        if (entity != null)
        {
            entity.OnDespawned -= HandleEntityDespawned;
        }

        _aliveEnemiesCount = Mathf.Max(0, _aliveEnemiesCount - 1);
    }
}