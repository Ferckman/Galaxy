using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEnemies : Spawner
{
    private const int MaxEnemies = 60;

    [SerializeField] private GameObject _type;
    private List<GameObject> _enemies = new List<GameObject>();

    protected override List<GameObject> list 
    { 
        get => _enemies; 
        set => _enemies = value; 
    }

    private void Awake()
    {
        FindGameManager();
        StartCoroutine(SpawnRoutineEnemies());
    }

    private IEnumerator SpawnRoutineEnemies()
    {
        yield return new WaitForSeconds(1f);

        while (gameManager != null && gameManager._isGameStarted)
        {
            if (_enemies.Count < MaxEnemies && _type != null)
            {
                Vector3 spawnPos = new Vector3(Random.Range(-7.5f, 7.5f), 7f, 0f);
                GameObject enemy = Instantiate(_type, spawnPos, Quaternion.identity);
                _enemies.Add(enemy);
            }

            yield return new WaitForSeconds(Cooldown);
        }
    }
}