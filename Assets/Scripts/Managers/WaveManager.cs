using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        [Header("Configuración General")]
        public string waveName = "Oleada 1";
        public int normalEnemyCount = 8; 

        [Header("Configuración de Kamikazes")]
        public bool enableKamikazes = false;
        public int kamikazeCount = 3;             
        public float kamikazeSpawnInterval = 4f;  
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

        // Si es un prefab del proyecto, lo instanciamos en la escena y lo activamos al instante
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

            Debug.Log($"Iniciando: {currentWave.waveName}");

            if (_activeSpawnerInstance != null)
            {
                yield return StartCoroutine(_activeSpawnerInstance.RunCompleteWaveSequenceRoutine(
                    currentWave.normalEnemyCount, 
                    currentWave.enableKamikazes, 
                    currentWave.kamikazeCount, 
                    currentWave.kamikazeSpawnInterval
                ));
            }

            _isWaveActive = false;
            
            Debug.Log($"¡Oleada {currentWave.waveName} superada! Dando un respiro...");

            yield return new WaitForSeconds(3f);

            _currentWaveIndex++;
        }

        Debug.Log("¡Todas las oleadas completadas! Preparando Boss...");
    }
}