using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerAsteroids : Spawner
{
    [SerializeField] private GameObject _type;
    [SerializeField] private List<GameObject> _asteroids;

    protected override List<GameObject> list { get => _asteroids; set => _asteroids = value; }

    public SpawnerAsteroids(float cooldown, GameObject type, List<GameObject> asteroids) : base(cooldown)
    {
        _type = type;
        _asteroids = asteroids;
    }

    private void Awake()
    {
        FindGameManager();
        StartCoroutine(SpawnRoutineAsteroids());
    }

    private void CreateAsteroid()
    {
        GameObject asteroid;
        int _posIni = Random.Range(1, 5);
        float scale = Random.Range(0.5f, 1f);

        switch (_posIni)
        {
            case 1:
                asteroid = Instantiate(_type, new Vector3(Random.Range(-7.5f, 7.5f), 6.5f, 0), Quaternion.identity);
                break;
            case 2:
                asteroid = Instantiate(_type, new Vector3(Random.Range(-7.5f, 7.5f), -6.5f, 0), Quaternion.identity);
                break;
            case 3:
                asteroid = Instantiate(_type, new Vector3(-10.2f, Random.Range(-3.5f, 3.5f), 0), Quaternion.identity);
                break;
            case 4:
                asteroid = Instantiate(_type, new Vector3(10.2f, Random.Range(-3.5f, 3.5f), 0), Quaternion.identity);
                break;
            default:
                asteroid = Instantiate(_type, new Vector3(Random.Range(-7.5f, 7.5f), 7f, 0), Quaternion.identity);
                break;
        }

        asteroid.transform.localScale = new Vector3(scale, scale, scale);
        Asteroid asteroidScript = asteroid.GetComponent<Asteroid>();

        _asteroids.Add(asteroid);
    }

    private IEnumerator SpawnRoutineAsteroids()
    {
        while(gameManager.GetGameStart())
        {
            if(_asteroids.Count < 60)    //Asteroid limit
                CreateAsteroid();

            yield return new WaitForSeconds(GetCooldown());
        }
    }

    public override void DestroyConcreteObstacle(GameObject obstacle)
    {
        base.DestroyConcreteObstacle(obstacle);
    }

    public override void DestroyObstacles()
    {
        base.DestroyObstacles();
    }
}
