using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEnemies : MonoBehaviour
{
    private EnemyFormationSpawner _formationSpawner;
    private KamikazeSpawner _kamikazeSpawner;
    private AsteroidSpawner _asteroidSpawner;

    // Listas para llevar el control de los elementos de la oleada actual
    private List<GameObject> _currentWaveEnemies = new List<GameObject>();
    private List<GameObject> _currentKamikazes = new List<GameObject>();
    private List<GameObject> _currentAsteroids = new List<GameObject>();

    private bool _isSpawningFinished = false;

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
    /// Gestiona la oleada completa: spawnea la formación, los kamikazes y asteroides, 
    /// y BLOQUEA el avance hasta que todos los enemigos de la oleada mueran.
    /// </summary>
    public IEnumerator SpawnWaveRoutine(WaveManager.Wave wave, System.Action<FormationController> onFormationCreated)
    {
        // 1. Limpiamos las listas de la oleada anterior
        _currentWaveEnemies.Clear();
        _currentKamikazes.Clear();
        _currentAsteroids.Clear();
        _isSpawningFinished = false;

        // 2. Gestionar la formación principal en bloque
        if (_formationSpawner != null && (wave.normalEnemyCount > 0 || wave.tankEnemyCount > 0))
        {
            yield return StartCoroutine(_formationSpawner.SpawnWaveInRowRoutine(wave, _currentWaveEnemies, onFormationCreated));
        }

        // 3. Lanzar Kamikazes (Guardamos la referencia para esperarla)
        Coroutine kamikazeCoroutine = null;
        if (_kamikazeSpawner != null && wave.enableKamikazes && wave.kamikazeCount > 0)
        {
            kamikazeCoroutine = StartCoroutine(_kamikazeSpawner.SpawnKamikazesOverTimeRoutine(wave, _currentWaveEnemies, _currentKamikazes));
        }

        // 4. Lanzar Asteroides (Guardamos la referencia para esperarla)
        Coroutine asteroidCoroutine = null;
        if (_asteroidSpawner != null && wave.enableAsteroids && wave.asteroidCount > 0)
        {
            asteroidCoroutine = StartCoroutine(_asteroidSpawner.SpawnAsteroidsRoutine(wave, _currentAsteroids));
        }

        // Esperamos a que terminen de aparecer todos los elementos programados de esta oleada
        if (kamikazeCoroutine != null) yield return kamikazeCoroutine;
        if (asteroidCoroutine != null) yield return asteroidCoroutine;

        // El despliegue ha concluido por completo
        _isSpawningFinished = true;

        // 5. BUCLE DE ESPERA: El juego no avanza hasta que el jugador elimine a TODO EL MUNDO
        while (!IsWaveComplete())
        {
            yield return null; // Espera al siguiente fotograma comprobando de nuevo
        }
    }

    /// <summary>
    /// Comprueba si ya han terminado de aparecer y si todos los enemigos han sido destruidos.
    /// </summary>
    private bool IsWaveComplete()
    {
        // Limpiamos de las listas las referencias que ya sean nulas (enemigos destruidos al morir)
        _currentWaveEnemies.RemoveAll(enemy => enemy == null);
        _currentKamikazes.RemoveAll(enemy => enemy == null);
        _currentAsteroids.RemoveAll(enemy => enemy == null);

        // La oleada se considera completada solo si el spawn terminó Y las tres listas están vacías
        return _isSpawningFinished &&
               _currentWaveEnemies.Count == 0 &&
               _currentKamikazes.Count == 0 &&
               _currentAsteroids.Count == 0;
    }
}