using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerPowerups : Spawner
{
    [SerializeField] private GameObject[] _type;
    private List<GameObject> _powerups = new List<GameObject>();

    protected override List<GameObject> list 
    { 
        get => _powerups; 
        set => _powerups = value; 
    }

    private void Awake()
    {
        FindGameManager();
        StartCoroutine(SpawnRoutinePowerups());
    }

    private IEnumerator SpawnRoutinePowerups()
    {
        yield return new WaitForSeconds(1f);

        while (gameManager != null && gameManager._isGameStarted)
        {
            if (_type != null && _type.Length > 0)
            {
                int randomIndex = Random.Range(0, _type.Length);
                
                if (_type[randomIndex] != null)
                {
                    Vector3 spawnPos = new Vector3(Random.Range(-7.5f, 7.5f), 7f, 0f);
                    GameObject powerup = Instantiate(_type[randomIndex], spawnPos, Quaternion.identity);
                    _powerups.Add(powerup);
                }
            }

            yield return new WaitForSeconds(Cooldown);
        }
    }
}