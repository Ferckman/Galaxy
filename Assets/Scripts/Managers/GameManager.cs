using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _playerPrefab;
    //[SerializeField] private GameObject _spawnerEnemiesPrefab;
    //[SerializeField] private GameObject _spawnerPowerupsPrefab;
    //[SerializeField] private GameObject _spawnerAsteroidsPrefab;

    [Header("References")]
    [SerializeField] private UIManager _uiManager;

    [Header("Systems")]
    [SerializeField] private WaveManager _waveManager;

    //[Header("Difficulty Settings")]
    //[SerializeField] private float _difficultyInterval;
    //[SerializeField] private float _cooldownDecreaseStep;
    //[SerializeField] private float _minCooldown;

    public bool _isGameStarted { get; private set; }

    private Spawner _spawnerEnemies;
    private Spawner _spawnerPowerups;
    private Spawner _spawnerAsteroids;

    public Spawner SpawnerEnemies => _spawnerEnemies;
    public Spawner SpawnerAsteroids => _spawnerAsteroids;

    public void SetGameStarted(bool status)
    {
        _isGameStarted = status;
    }

    private void Update()
    {
        if (!_isGameStarted && Input.GetKeyDown(KeyCode.Space)) StartGame();
    }

    private void StartGame()
    {
        _isGameStarted = true;

        Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);

        //_spawnerEnemies = Instantiate(_spawnerEnemiesPrefab, Vector3.zero, Quaternion.identity).GetComponent<Spawner>();
        //_spawnerPowerups = Instantiate(_spawnerPowerupsPrefab, Vector3.zero, Quaternion.identity).GetComponent<Spawner>();
        //_spawnerAsteroids = Instantiate(_spawnerAsteroidsPrefab, Vector3.zero, Quaternion.identity).GetComponent<Spawner>();

        if (_uiManager != null)
        {
            _uiManager.HideMainMenu();
            _uiManager.UpdateLives(3);
            _uiManager.ResetScore();
        }

        //StartCoroutine(IncreaseDifficulty(_spawnerEnemies));
        //StartCoroutine(IncreaseDifficulty(_spawnerAsteroids));

        // 3. ¡Disparamos las oleadas!
        if (_waveManager != null)
        {
            _waveManager.StartWaves();
        }
    }

    public void DestroyConcreteElement(GameObject element)
    {
        Spawner targetSpawner = null;

        if (element.CompareTag("Enemy")) targetSpawner = _spawnerEnemies;
        else if (element.CompareTag("Powerup")) targetSpawner = _spawnerPowerups;
        else if (element.CompareTag("Asteroid")) targetSpawner = _spawnerAsteroids;

        if (targetSpawner != null) targetSpawner.DestroyConcreteObstacle(element);
    }

    public void DestroyElements()
    {
        if (_spawnerEnemies != null) _spawnerEnemies.DestroyObstacles();
        if (_spawnerPowerups != null) _spawnerPowerups.DestroyObstacles();
        if (_spawnerAsteroids != null) _spawnerAsteroids.DestroyObstacles();

        if (_spawnerEnemies != null) Destroy(_spawnerEnemies.gameObject);
        if (_spawnerPowerups != null) Destroy(_spawnerPowerups.gameObject);
        if (_spawnerAsteroids != null) Destroy(_spawnerAsteroids.gameObject);
    }

    /*
    private IEnumerator IncreaseDifficulty(Spawner spawner)
    {
        if (spawner == null) yield break;

        WaitForSeconds waitTime = new WaitForSeconds(_difficultyInterval);

        while (spawner.Cooldown > _minCooldown)
        {
            yield return waitTime;
            spawner.Cooldown = Mathf.Max(_minCooldown, spawner.Cooldown - _cooldownDecreaseStep);
        }
    }
    */
}