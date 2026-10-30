using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerAsteroids : Spawner
{
    private const int MaxAsteroids = 60;

    [SerializeField] private GameObject _type;
    private List<GameObject> _asteroids = new List<GameObject>();

    protected override List<GameObject> list 
    { 
        get => _asteroids; 
        set => _asteroids = value; 
    }

    private void Awake()
    {
        FindGameManager();
        StartCoroutine(SpawnRoutineAsteroids());
    }

    private void CreateAsteroid()
    {
        if (_type == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        float screenHeight = cam.orthographicSize;
        float screenWidth = cam.orthographicSize * cam.aspect;
        float spawnOffset = 2.5f;

        int posIni = Random.Range(1, 5);
        float scale = Random.Range(0.40f, 1.2f);
        Vector3 spawnPos = Vector3.zero;

        switch (posIni)
        {
            case 1:
                spawnPos = new Vector3(Random.Range(-screenWidth, screenWidth), screenHeight + spawnOffset, 0f);
                break;
            case 2:
                spawnPos = new Vector3(Random.Range(-screenWidth, screenWidth), -screenHeight - spawnOffset, 0f);
                break;
            case 3:
                spawnPos = new Vector3(-screenWidth - spawnOffset, Random.Range(-screenHeight, screenHeight), 0f);
                break;
            case 4:
                spawnPos = new Vector3(screenWidth + spawnOffset, Random.Range(-screenHeight, screenHeight), 0f);
                break;
        }

        GameObject asteroid = Instantiate(_type, spawnPos, Quaternion.identity);

        if (asteroid != null)
        {
            if (asteroid.TryGetComponent<Asteroid>(out var asteroidScript)) 
                asteroidScript.SetAsteroidProperties(scale);

            _asteroids.Add(asteroid);
        }
    }

    private IEnumerator SpawnRoutineAsteroids()
    {
        yield return new WaitForSeconds(1f);

        while (gameManager != null && gameManager._isGameStarted)
        {
            if (_asteroids.Count < MaxAsteroids) CreateAsteroid();

            yield return new WaitForSeconds(Cooldown);
        }
    }
}