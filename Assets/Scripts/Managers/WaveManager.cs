using UnityEngine;
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
        [Tooltip("Cantidad de enemigos Tanque (más vida, disparo lento) en el bloque.")]
        public int tankEnemyCount = 2;

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

        [Header("3. Eventos Especiales (Opcionales por Oleada)")]
        [Tooltip("¿Esta oleada incluye un Emisor de Drones pesado que baja del cielo?")]
        public bool spawnDroneCarrier = false;
        [Tooltip("¿Esta oleada es un combate contra el Jefe Final?")]
        public bool isBossWave = false;
    }

    [Header("Settings")]
    [SerializeField] private List<Wave> _waves;
    [SerializeField] private SpawnerEnemies _enemySpawnerPrefab; 

    private SpawnerEnemies _activeSpawnerInstance; 
    private int _currentWaveIndex = 0;
    private bool _isWaveActive = false;

    public void StartWaves()
    {
        if (_isWaveActive) return; 

        if (_activeSpawnerInstance == null && _enemySpawnerPrefab != null)
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

            //Debug.Log($"Iniciando: {currentWave.waveName}");

            if (_activeSpawnerInstance != null)
            {
                // Delegamos toda la ejecución compleja al Spawner pasando la configuración completa de la oleada
                yield return StartCoroutine(_activeSpawnerInstance.RunCompleteWaveSequenceRoutine(currentWave));
            }

            _isWaveActive = false;
            
            Debug.Log($"¡Oleada {currentWave.waveName} superada! Dando un respiro...");

            yield return new WaitForSeconds(3f);

            _currentWaveIndex++;
        }

        Debug.Log("¡Todas las oleadas completadas! ¡Victoria!");
    }
}