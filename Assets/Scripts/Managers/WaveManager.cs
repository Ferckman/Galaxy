using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        [Header("Configuración de la Oleada")]
        public string waveName = "Oleada 1";
        public int enemyCount = 10;
        // Ya no necesitamos 'spawnInterval' aquí si van a salir en formación de golpe, 
        // pero puedes dejarlo o usarlo si quieres cambiar pausas.
    }

    [Header("Settings")]
    [SerializeField] private List<Wave> _waves;
    [SerializeField] private SpawnerEnemies _enemySpawner;

    private int _currentWaveIndex = 0;
    private bool _isWaveActive = false;

    public void StartWaves()
    {
        if (_isWaveActive) return; 

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

            // 1. Lanzamos la formación en fila y esperamos a que el spawner coloque a todos y active la señal
            if (_enemySpawner != null)
            {
                yield return StartCoroutine(_enemySpawner.SpawnWaveInRowRoutine(currentWave.enemyCount));
            }

            // 2. Esperamos aquí bloqueados hasta que TODOS los enemigos de la oleada mueran
            yield return new WaitUntil(() => _enemySpawner == null || _enemySpawner.GetActiveEnemyCount() == 0);

            _isWaveActive = false;
            
            Debug.Log($"¡Oleada {currentWave.waveName} superada! Dando un respiro...");

            // 3. Pausa de descanso antes de arrancar la siguiente oleada
            yield return new WaitForSeconds(3f);

            _currentWaveIndex++;
        }

        Debug.Log("¡Todas las oleadas completadas! Preparando Boss...");
        // Aquí puedes activar a tu Jefe Final cuando toque
    }
}