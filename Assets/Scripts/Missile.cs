using UnityEngine;

public class Missile : Shots
{
    [SerializeField] private Transform _limit;
    [SerializeField] private float _rotateSpeed;
    
    private Rigidbody2D _rb;
    private GameManager _gameManager;
    private Transform _target = null;
    private Transform _targetEnemy = null;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        _gameManager = Object.FindFirstObjectByType<GameManager>();
        
        Limit limitComponent = Object.FindFirstObjectByType<Limit>();
        if (limitComponent != null)
        {
            _limit = limitComponent.transform;
        }

        LookForNearestEnemy();
    }

    private void LookForNearestEnemy()
    {
        if (_gameManager == null) return;

        Spawner spawnerEnemies = _gameManager.SpawnerEnemies;
        Spawner spawnerAsteroids = _gameManager.SpawnerAsteroids;

        Transform enemyNearest = spawnerEnemies != null ? spawnerEnemies.LookForNearest(transform) : null;
        Transform asteroidNearest = spawnerAsteroids != null ? spawnerAsteroids.LookForNearest(transform) : null;

        if (enemyNearest != null && asteroidNearest != null)
        {
            float distanceEnemyNearest = Vector3.Distance(transform.position, enemyNearest.position);
            float distanceAsteroidNearest = Vector3.Distance(transform.position, asteroidNearest.position);
            
            _targetEnemy = distanceAsteroidNearest < distanceEnemyNearest ? asteroidNearest : enemyNearest;
        }
        else if (enemyNearest != null) _targetEnemy = enemyNearest;
        else if (asteroidNearest != null) _targetEnemy = asteroidNearest;
        else _targetEnemy = null;
    }

    public override void Movement()
    {
        if (_targetEnemy == null || !_targetEnemy.gameObject.activeInHierarchy) LookForNearestEnemy();

        if (_targetEnemy != null) _target = _targetEnemy;
        else _target = _limit;

        if (_target == null) return;

        Vector2 direction = (Vector2)_target.position - _rb.position;
        direction.Normalize();
        
        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        
        _rb.angularVelocity = -rotateAmount * _rotateSpeed;
        
        _rb.velocity = (Vector2)(transform.up * _speed);
    }
}