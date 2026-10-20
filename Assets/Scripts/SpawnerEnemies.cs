using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnerEnemies : Spawner
{
    [SerializeField] private GameObject _type;
    [SerializeField] private List<GameObject> _enemies;

    protected override List<GameObject> list { get => _enemies; set => _enemies = value; }

    public SpawnerEnemies(float cooldown, GameObject type, List<GameObject> enemies) : base(cooldown)
    {
        _type = type;
        _enemies = enemies;
    }

    private void Awake()
    {
        FindGameManager();
        StartCoroutine(SpawnRoutineEnemies());
    }

    private IEnumerator SpawnRoutineEnemies()
    {
        while(gameManager.GetGameStart())
        {
            if(_enemies.Count < 60)    //Enemy limit
            {
                GameObject enemy = Instantiate(_type, new Vector3(Random.Range(-7.5f, 7.5f), 7, 0), Quaternion.identity);
                _enemies.Add(enemy);
            }

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
