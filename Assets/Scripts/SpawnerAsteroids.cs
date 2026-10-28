using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerAsteroids : Spawner
{
    private const int MaxAsteroids = 60;

    [SerializeField] private GameObject _type;
    [SerializeField] private List<GameObject> _asteroids = new List<GameObject>();

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

        GameObject asteroid = null;
        int posIni = Random.Range(1, 5);
        float scale = Random.Range(0.5f, 1f);

        switch (posIni)
        {
            case 1:
                asteroid = Instantiate(_type, new Vector3(Random.Range(-7.5f, 7.5f), 6.5f, 0f), Quaternion.identity);
                break;
            case 2:
                asteroid = Instantiate(_type, new Vector3(Random.Range(-7.5f, 7.5f), -6.5f, 0f), Quaternion.identity);
                break;
            case 3:
                asteroid = Instantiate(_type, new Vector3(-10.2f, Random.Range(-3.5f, 3.5f), 0f), Quaternion.identity);
                break;
            case 4:
                asteroid = Instantiate(_type, new Vector3(10.2f, Random.Range(-3.5f, 3.5f), 0f), Quaternion.identity);
                break;
            default:
                asteroid = Instantiate(_type, new Vector3(Random.Range(-7.5f, 7.5f), 7f, 0f), Quaternion.identity);
                break;
        }

        if (asteroid != null)
        {
            asteroid.transform.localScale = new Vector3(scale, scale, scale);

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