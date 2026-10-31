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
                Camera cam = Camera.main;
                if (cam != null)
                {
                    float screenHeight = cam.orthographicSize;
                    float screenWidth = cam.orthographicSize * cam.aspect;
                    
                    float margin = 1.0f;
                    float randomX = Random.Range(-screenWidth + margin, screenWidth - margin);
                
                    float spawnY = screenHeight + 1.5f;

                    Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

                    int randomIndex = Random.Range(0, _type.Length);
                    if (_type[randomIndex] != null)
                    {
                        GameObject powerup = Instantiate(_type[randomIndex], spawnPos, Quaternion.identity);
                        _powerups.Add(powerup);
                    }
                }
            }
            
            yield return new WaitForSeconds(Cooldown);
        }
    }
    
}