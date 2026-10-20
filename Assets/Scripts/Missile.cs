using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Missile : Shots
{
    [SerializeField] private Transform limit;
    [SerializeField] private float _rotateSpeed;
    
    private Rigidbody2D _rb;
    private GameManager gameManager;
    private Transform _target;
    private Transform _targetEnemy;

    public Missile(int damage, int speed): base(damage, speed)
    {

    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        limit = GameObject.Find("LimitShots").GetComponent<Transform>();
        LookForNearestEnemy();
        //poner target_enemy empezando null aqui????
    }

    private void LookForNearestEnemy()
    {
        GameObject spawnerEnemies = gameManager.GetSpawnerEnemies();
        GameObject spawnerAsteroids = gameManager.GetSpawnerAsteroids();

        Transform enemyNearest = spawnerEnemies.GetComponent<Spawner>().LookForNearest(this.transform);
        Transform asteroidNearest = spawnerAsteroids.GetComponent<Spawner>().LookForNearest(this.transform);

        float distanceEnemyNearest, distanceAsteroidNearest;


        if(enemyNearest != null && asteroidNearest != null)
        {
            distanceEnemyNearest = Vector3.Distance(transform.position, enemyNearest.position);
            distanceAsteroidNearest = Vector3.Distance(transform.position, asteroidNearest.position);
            _targetEnemy = enemyNearest;
            if(distanceAsteroidNearest < distanceEnemyNearest)
                _targetEnemy = asteroidNearest;
        }
        else if(enemyNearest != null)   //Empty asteroid list
        {
            distanceEnemyNearest = Vector3.Distance(transform.position, enemyNearest.position);
            _targetEnemy = enemyNearest;
        }
        else if(asteroidNearest != null)   //Empty enemy list
        {
            distanceAsteroidNearest = Vector3.Distance(transform.position, asteroidNearest.position);
            _targetEnemy = asteroidNearest;
        }
    }

    public override void Movement()
    {
        if(_targetEnemy != null)
            _target = _targetEnemy;
        else
        {
            _target = limit;
            LookForNearestEnemy();
        }

        Vector2 _direction = (Vector2)_target.position - _rb.position;
        _direction.Normalize();
        float _rotateAmount = Vector3.Cross(_direction, transform.up).z;
        _rb.angularVelocity = -_rotateAmount * _rotateSpeed;
        _rb.velocity = transform.up * _speed;
    }
}
