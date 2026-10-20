using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool _gameStart;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _spawnerEnemiesPrefab;
    [SerializeField] private GameObject _spawnerPowerupsPrefab;
    [SerializeField] private GameObject _spawnerAsteroidsPrefab;

    private GameObject spawnerEnemies;
    private GameObject spawnerPowerups;
    private GameObject spawnerAsteroids;

    private void StartGame()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _gameStart = true;

            //Create ship and spawners
            Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
            spawnerEnemies = Instantiate(_spawnerEnemiesPrefab, Vector3.zero, Quaternion.identity);
            spawnerPowerups = Instantiate(_spawnerPowerupsPrefab, Vector3.zero, Quaternion.identity);
            spawnerAsteroids = Instantiate(_spawnerAsteroidsPrefab, Vector3.zero, Quaternion.identity);

            UIManager uIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
            uIManager.HideMainMenu();
            uIManager.UpdateLives(3);
            uIManager.ResetScore();

            StartCoroutine(IncreaseDifficulty(spawnerEnemies.GetComponent<Spawner>()));
            StartCoroutine(IncreaseDifficulty(spawnerPowerups.GetComponent<Spawner>()));
            StartCoroutine(IncreaseDifficulty(spawnerAsteroids.GetComponent<Spawner>()));
        }
    }

    public bool GetGameStart()
    {
        return _gameStart;
    }

    public void SetGameStart(bool gameStart)
    {
        _gameStart = gameStart;
    }

    public GameObject GetSpawnerEnemies()
    {
        return spawnerEnemies;
    }

    public GameObject GetSpawnerAsteroids()
    {
        return spawnerAsteroids;
    }

    public void DestroyConcreteElement(GameObject element)
    {
        Spawner spawner;

        if(element.tag == "Enemy")
        {
            spawner = spawnerEnemies.GetComponent<Spawner>();
        }
        else if(element.tag == "Powerup")
        {
            spawner = spawnerPowerups.GetComponent<Spawner>();
        }
        else    //Asteroid
        {
            spawner = spawnerAsteroids.GetComponent<Spawner>();
        }

        spawner.DestroyConcreteObstacle(element);
    }

    public void DestroyElements()
    {
        Spawner spawnerEnemy = spawnerEnemies.GetComponent<Spawner>();
        Spawner spawnerPowerup = spawnerPowerups.GetComponent<Spawner>();
        Spawner spawnerAsteroid = spawnerAsteroids.GetComponent<Spawner>();

        spawnerEnemy.DestroyObstacles();
        spawnerPowerup.DestroyObstacles();
        spawnerAsteroid.DestroyObstacles();

        Destroy(spawnerEnemies);
        Destroy(spawnerPowerups);
        Destroy(spawnerAsteroids);
    }

    private IEnumerator IncreaseDifficulty(Spawner spawner)
    {
        while(spawner.GetCooldown() > 0.75f)
        {
            spawner.SetCooldown(spawner.GetCooldown() - 0.25f);

            yield return new WaitForSeconds(10.0f);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if(!_gameStart)
            StartGame();
        //
    }
}
