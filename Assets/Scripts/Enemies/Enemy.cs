using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Combat & Shooting")]
    [SerializeField] private GameObject _enemyBulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 2f;

    [Header("Movement & Formation")]
    [SerializeField] private float _formationSpeed = 5f;

    [Header("Loot Drop Settings")]
    [SerializeField] private protected GameObject _missileAmmoPrefab;        // Prefab del power-up de munición
    [SerializeField] private GameObject[] _generalPowerUpPrefabs;   // Prefabs de power-ups (Triple, Speed, Shield)
    
    [SerializeField] private Animator _animator;

    private Vector3 _targetFormationPos;
    private bool _isMovingToFormation = false;
    private bool _isCombatActive = false;

    // Obligatorio por heredar de Entity, aunque el enemigo use su propia lógica de formación
    protected override void Movement()
    {
        // El movimiento del enemigo se gestiona mediante _isMovingToFormation 
        // y el FormationController externo. Lo dejamos vacío o implementamos lo que necesites.
    }

    protected override void Update()
    {
        // FASE 1: Descenso inicial desde arriba hasta la posición de la fila
        if (_isMovingToFormation)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetFormationPos, _formationSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetFormationPos) < 0.01f)
            {
                transform.position = _targetFormationPos;
                _isMovingToFormation = false;
            }
            return; 
        }

        // FASE 2: Espera en formación hasta recibir la señal global
        if (!_isCombatActive)
        {
            return; 
        }

        // FASE 3: Comportamiento activo de combate y control de límites
        BehaviorLogic();
        CheckBounds();
    }

    public void SetupFormation(Vector3 targetPos)
    {
        _targetFormationPos = targetPos;
        _isMovingToFormation = true;
    }

    /// <summary>
    /// Aquí recibimos la señal global: el enemigo ya está en su sitio, 
    /// por lo que activamos el combate y arrancamos su rutina de disparos.
    /// </summary>
    public virtual void ActivateCombatBehavior()
    {
        _isCombatActive = true;
        //Debug.Log($"{gameObject.name}: ¡Señal de combate recibida! Abriendo fuego.");
        
        // ¡Aquí lanzamos los disparos de forma sincronizada!
        StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        // Pequeño retardo aleatorio inicial para que no disparen todos milimétricamente a la vez
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        while (_isCombatActive && enabled)
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

    protected virtual void BehaviorLogic()
    {
        // Si en el futuro quieres añadir movimiento de ataque en zig-zag o persecución, va aquí.
    }

    // Marcamos el método como virtual para permitir polimorfismo
    protected override void DropLoot()
    {
        float roll = Random.Range(0f, 100f);

        if (roll < 50f) return; // 50%: Nada
        else if (roll < 85f)
        {
            // 35%: Munición de misil
            if (_missileAmmoPrefab != null) Instantiate(_missileAmmoPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // 15%: Power-up general
            SpawnRandomGeneralPowerUp();
        }
    }

    // Método auxiliar protegido para que las clases hijas también puedan usarlo
    protected void SpawnRandomGeneralPowerUp()
    {
        if (_generalPowerUpPrefabs != null && _generalPowerUpPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, _generalPowerUpPrefabs.Length);
            GameObject selectedPowerUp = _generalPowerUpPrefabs[randomIndex];

            if (selectedPowerUp != null)
            {
                Instantiate(selectedPowerUp, transform.position, Quaternion.identity);
            }
        }
    }

    public void SetFacingDirection(int direction)
    {
        if (_animator != null)
        {
            _animator.SetInteger("TurnDir", direction);
        }
    }
}
/*
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                if (!player.IsDisabled)
                {
                    player.GetDamage(_damage);
                    if (ShouldDestroyOnPlayerHit)
                    {
                        DestroyMyself();
                        IncreaseScore();
                    }
                }
            }
        }
        else if (other.CompareTag("Shots"))
        {
            if (other.TryGetComponent<Shots>(out var shot))
            {
                _health -= shot.Damage;
                if (_health <= 0)
                {
                    DestroyMyself();
                    IncreaseScore();
                }
            }

            Destroy(other.gameObject);
        }
        else if (other.TryGetComponent<Kamikaze>(out var kamikaze))
        {
            //DestroyMyself();
        }
    }
    */
