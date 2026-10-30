using UnityEngine;
using System.Collections;

public class ShooterEnemy : Enemy
{
    [Header("Combat Settings")]
    [SerializeField] private GameObject _enemyBulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate;

    protected override void Awake()
    {
        base.Awake();
        _health = 80;
        _speed = 3.5f;
        _score = 15;
    }

    private void Start()
    {
        StartCoroutine(ShootRoutine());
    }

    protected override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.down);
    }

    private IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        while (enabled)
        {
            if (_enemyBulletPrefab != null)
            {
                Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
                
                GameObject bullet = Instantiate(_enemyBulletPrefab, spawnPos, Quaternion.identity);
                
                Laser laserScript = bullet.GetComponent<Laser>();
                
                if (laserScript != null) laserScript.SetDirection(Vector3.down);
            }

            yield return new WaitForSeconds(_fireRate);
        }
    }
}