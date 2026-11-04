using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEnemies : MonoBehaviour
{
    private EnemyFormationSpawner _formationSpawner;
    private KamikazeSpawner _kamikazeSpawner;
    private AsteroidSpawner _asteroidSpawner;

    // Listas globales de control para saber qué hay vivo en pantalla
    private List<GameObject> _blockEnemies = new List<GameObject>();
    private List<GameObject> _kamikazeEnemies = new List<GameObject>();
    private List<GameObject> _asteroidEnemies = new List<GameObject>();

    private void Awake()
    {
        // Como están en el mismo GameObject, los cogemos automáticamente al arrancar
        _formationSpawner = GetComponent<EnemyFormationSpawner>();
        _kamikazeSpawner = GetComponent<KamikazeSpawner>();
        _asteroidSpawner = GetComponent<AsteroidSpawner>();

        // Una comprobación por seguridad por si a alguno se le olvidó ponerse el componente
        if (_formationSpawner == null) Debug.LogError("Falta el componente EnemyFormationSpawner en este objeto.", this);
        if (_kamikazeSpawner == null) Debug.LogError("Falta el componente KamikazeSpawner en este objeto.", this);
        if (_asteroidSpawner == null) Debug.LogError("Falta el componente AsteroidSpawner en este objeto.", this);
    }

    /// <summary>
    /// Orquesta la secuencia completa de la oleada delegando en los sub-spawners.
    /// </summary>
    public IEnumerator RunCompleteWaveSequenceRoutine(WaveManager.Wave wave)
    {
        gameObject.SetActive(true);

        // Limpiamos listas de oleadas anteriores
        _blockEnemies.Clear();
        _kamikazeEnemies.Clear();
        _asteroidEnemies.Clear();

        // 1. Delegamos las naves normales y tanques en formación
        if (_formationSpawner != null)
        {
            yield return StartCoroutine(_formationSpawner.SpawnWaveInRowRoutine(wave, _blockEnemies, (controller) => 
            {
                // Aquí puedes hacer algo con el FormationController si lo necesitas en el futuro
            }));
        }

        // 2. Delegamos los kamikazes si la oleada los activa
        if (wave.enableKamikazes && wave.kamikazeCount > 0 && _kamikazeSpawner != null)
        {
            StartCoroutine(_kamikazeSpawner.SpawnKamikazesOverTimeRoutine(wave, _blockEnemies, _kamikazeEnemies));
        }

        // 3. Delegamos los asteroides si la oleada los activa
        if (wave.enableAsteroids && wave.asteroidCount > 0 && _asteroidSpawner != null)
        {
            StartCoroutine(_asteroidSpawner.SpawnAsteroidsRoutine(wave, _asteroidEnemies));
        }

        // 4. Esperamos de forma limpia a que no quede absolutamente ningún enemigo vivo en pantalla
        yield return new WaitUntil(() => GetActiveEnemyCount() == 0);
    }

    /// <summary>
    /// Cuenta cuántos enemigos siguen vivos en total (bloque + kamikazes + asteroides).
    /// </summary>
    public int GetActiveEnemyCount()
    {
        _blockEnemies.RemoveAll(enemy => enemy == null);
        _kamikazeEnemies.RemoveAll(enemy => enemy == null);
        _asteroidEnemies.RemoveAll(enemy => enemy == null);
        
        return _blockEnemies.Count + _kamikazeEnemies.Count + _asteroidEnemies.Count;
    }
}