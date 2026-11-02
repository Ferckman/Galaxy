using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int _health;
    [SerializeField] private int _damage;
    [SerializeField] private int _score;
    [SerializeField] private bool _shouldDestroyOnPlayerHit = true;

    [Header("Combat & Shooting")]
    [SerializeField] private GameObject _enemyBulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 2f;

    [Header("Visuals & Effects")]
    [SerializeField] private protected GameObject _explosionPrefab;

    [Header("Movement & Formation")]
    [SerializeField] private float _formationSpeed = 5f;

    [Header("Loot Drop Settings")]
    [SerializeField] private protected GameObject _missileAmmoPrefab;        // Prefab del power-up de munición
    [SerializeField] private GameObject[] _generalPowerUpPrefabs;   // Prefabs de power-ups (Triple, Speed, Shield)
    
    [SerializeField] private Animator _animator;

    private Vector3 _targetFormationPos;
    private bool _isMovingToFormation = false;
    private bool _isCombatActive = false;

    // Referencias a controladores (puedes adaptarlas a tu GameManager/UIManager existente)
    protected GameManager _gameManager;
    protected UIManager _uiManager;

    protected virtual bool ShouldDestroyOnPlayerHit => _shouldDestroyOnPlayerHit;

    private void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _uiManager = FindFirstObjectByType<UIManager>();
        
        // ¡OJO! Ya no ponemos StartCoroutine(ShootRoutine()) aquí, 
        // porque queremos que esperen a estar formados.
    }

    private void Update()
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

    protected virtual void CheckBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float screenHeight = cam.orthographicSize;
        float screenWidth = cam.orthographicSize * cam.aspect;
        float margin = 5.0f; 

        if (transform.position.y < (-screenHeight - margin) || 
            transform.position.y > (screenHeight + margin) || 
            transform.position.x < (-screenWidth - margin) || 
            transform.position.x > (screenWidth + margin))
        {
            if (_gameManager != null) _gameManager.DestroyConcreteElement(gameObject);
            Destroy(gameObject);
        }
    }

    public virtual void IncreaseScore()
    {
        if (_uiManager != null) _uiManager.UpdateScore(_score);
    }

    public virtual void DestroyMyself()
    {
        SpawnExplosion();

        if (_gameManager != null) _gameManager.DestroyConcreteElement(gameObject);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        enabled = false;

        Destroy(gameObject);
    }

    protected virtual void SpawnExplosion()
    {
        if (_explosionPrefab != null) 
        {
            // Añadimos un pequeño desfase aleatorio en el eje Z (-0.1f a -0.5f) 
            // para que una explosión siempre esté matemáticamente "delante" de la otra.
            Vector3 explosionPos = transform.position + new Vector3(0f, 0f, Random.Range(-0.1f, -0.5f));
            
            GameObject explosion = Instantiate(_explosionPrefab, explosionPos, Quaternion.identity);

            // OPCIONAL EXTRA: Si tu explosión usa un SpriteRenderer, puedes forzar 
            // un Order in Layer dinámico para evitar parpadeos de orden.
            if (explosion.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) sr.sortingOrder = 10; // Ponlo por encima de los enemigos normales
            DropLoot();
        }
    }

    // Marcamos el método como virtual para permitir polimorfismo
    protected virtual void DropLoot()
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
}