using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Combat & Shooting")]
    [SerializeField] private GameObject _enemyLaserPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 2f;

    [Header("Loot Drop Settings")]
    [SerializeField] private protected GameObject _missileAmmoPrefab;        // Prefab del power-up de munición
    [SerializeField] private GameObject[] _generalPowerUpPrefabs;   // Prefabs de power-ups (Triple, Speed, Shield)
    
    [SerializeField] private Animator _animator;

    [Header("Entry Settings")]
    [Tooltip("Tiempo exacto que tarda el bloque en llegar a su posición (en segundos).")]
    [SerializeField] private float _entryDuration = 1.5f; 

    private Vector3 _targetFormationPos;
    private Vector3 _startFormationPos;
    private EnemySpawnDirection _spawnDirection;
    
    private bool _isMovingToFormation = false;
    private bool _isCombatActive = false;
    
    private float _elapsedEntryTime = 0f;
    private float _initialZRotation = 0f;

    // Obligatorio por heredar de Entity, aunque el enemigo use su propia lógica de formación
    protected override void Movement()
    {
        
    }

    protected override void Update()
    {
        // FASE 1: Entrada sincronizada a la formación
        if (_isMovingToFormation)
        {
            _elapsedEntryTime += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsedEntryTime / _entryDuration);

            // Sincronización por tiempo: todas las naves llegan exactamente a la vez (bloque rígido)
            transform.position = Vector3.Lerp(_startFormationPos, _targetFormationPos, progress);

            // Gestión del giro suave si vienen de los flancos o de abajo
            if (_spawnDirection != EnemySpawnDirection.Top)
            {
                // Esperan a recorrer el 50% del trayecto antes de empezar a girar
                float rotationDelayThreshold = 0.5f; 

                if (progress < rotationDelayThreshold)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, _initialZRotation);
                }
                else
                {
                    float turnProgress = (progress - rotationDelayThreshold) / (1f - rotationDelayThreshold);
                    transform.rotation = Quaternion.Slerp(Quaternion.Euler(0f, 0f, _initialZRotation), Quaternion.identity, turnProgress);
                }
            }

            // Al finalizar el trayecto
            if (progress >= 1f)
            {
                transform.position = _targetFormationPos;
                _isMovingToFormation = false;
                transform.rotation = Quaternion.identity; // Alineados y listos para combatir
            }
            return; 
        }

        // FASE 2 y 3: Combate activo
        if (!_isCombatActive) return;

        BehaviorLogic();
        CheckBounds();
    }

    /// <summary>
    /// Configura la posición y la orientación inicial del enemigo según la dirección de la oleada.
    /// </summary>
    public void SetupFormation(Vector3 targetPos, EnemySpawnDirection direction)
    {
        _targetFormationPos = targetPos;
        _startFormationPos = transform.position;
        _spawnDirection = direction;
        _elapsedEntryTime = 0f;
        _isMovingToFormation = true;

        // Determinamos la rotación inicial según de dónde provenga el enemigo
        switch (_spawnDirection)
        {
            case EnemySpawnDirection.Bottom:
                _initialZRotation = 180f; // Miran hacia arriba
                break;
            case EnemySpawnDirection.Left:
                _initialZRotation = -90f; // Miran hacia la derecha (hacia donde viajan)
                break;
            case EnemySpawnDirection.Right:
                _initialZRotation = 90f;  // Miran hacia la izquierda (hacia donde viajan)
                break;
            case EnemySpawnDirection.Top:
            default:
                _initialZRotation = 0f;   // Miran hacia abajo (comportamiento normal)
                break;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, _initialZRotation);
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

        // Buscamos el contenedor de proyectiles (puedes crear un LasersContainer singleton idéntico al de los enemigos)
        Transform lasersContainer = LasersContainer.Instance != null ? LasersContainer.Instance.transform : null;

        while (_isCombatActive && enabled)
        {
            if (_enemyLaserPrefab != null)
            {
                Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
                
                // Instanciamos el láser asignándole directamente el contenedor como padre (cuarto parámetro)
                GameObject bullet = Instantiate(_enemyLaserPrefab, spawnPos, Quaternion.identity, lasersContainer);
                
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
