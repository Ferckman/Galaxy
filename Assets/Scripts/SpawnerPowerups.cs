using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerPowerups : Spawner
{
    [SerializeField] private GameObject[] _type;
    [SerializeField] private List<GameObject> _powerups;

    protected override List<GameObject> list { get => _powerups; set => _powerups = value; }

    public SpawnerPowerups(float cooldown, GameObject[] type, List<GameObject> powerups) : base(cooldown)
    {
        _type = type;
        _powerups = powerups;
    }

    private void Awake()
    {
        FindGameManager();
        StartCoroutine(SpawnRoutinePowerups());
    }

    private IEnumerator SpawnRoutinePowerups()
    {
        while(gameManager.GetGameStart())
        {
            int _random = Random.Range(0,3);
            GameObject powerup = Instantiate(_type[_random], new Vector3(Random.Range(-7.5f, 7.5f), 7, 0), Quaternion.identity);
            _powerups.Add(powerup);
            
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
