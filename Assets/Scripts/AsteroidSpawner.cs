using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _asteroidPrefab;

    /// <summary>
    /// Gestiona la aparición gradual de asteroides según los parámetros de la oleada.
    /// </summary>
    public IEnumerator SpawnAsteroidsRoutine(WaveManager.Wave wave, List<GameObject> asteroidEnemiesList)
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
                asteroidEnemiesList.Add(asteroidObj);
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
}