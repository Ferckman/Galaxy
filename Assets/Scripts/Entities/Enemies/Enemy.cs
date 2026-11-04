using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    [Header("Combat & Shooting")]
    [SerializeField] private GameObject _enemyLaserPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 2f;

    private bool _cameFromBottom = false;
    private bool _cameFromSide = false;
    private Vector3 _startFormationPos;
    private float _initialZRotation = 0f;
    private float _entryDuration = 1.5f; 
    private float _elapsedEntryTime = 0f;

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
        
    }
/*
    protected override void Update()
    {
        // FASE 1: Descenso o ascenso inicial hasta la posición de la fila
        if (_isMovingToFormation)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetFormationPos, _formationSpeed * Time.deltaTime);

            if (_cameFromBottom)
            {
                float totalDist = Vector3.Distance(_startFormationPos, _targetFormationPos);
                float currentDist = Vector3.Distance(transform.position, _targetFormationPos);
                float progress = totalDist > 0 ? 1f - (currentDist / totalDist) : 1f;

                // --- CONFIGURACIÓN DEL RETARDO DE GIRO ---
                // 0.5f significa que espera a recorrer la mitad del camino antes de empezar a girar.
                // Si quieres que espere más, sube el valor (ej. 0.7f). Si quieres que empiece antes, bájalo (ej. 0.3f).
                float rotationDelayThreshold = 0.5f; 

                if (progress < rotationDelayThreshold)
                {
                    // Durante la primera parte del viaje, se mantienen firmes mirando hacia arriba (180°)
                    transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                }
                else
                {
                    // A partir del umbral, mapeamos el progreso restante para hacer un giro suave hasta el final
                    float turnProgress = (progress - rotationDelayThreshold) / (1f - rotationDelayThreshold);
                    transform.rotation = Quaternion.Slerp(Quaternion.Euler(0f, 0f, 180f), Quaternion.identity, turnProgress);
                }
                // ----------------------------------------
            }

            if (Vector3.Distance(transform.position, _targetFormationPos) < 0.01f)
            {
                transform.position = _targetFormationPos;
                _isMovingToFormation = false;
                transform.rotation = Quaternion.identity; 
            }
            return; 
        }

        // FASE 2 y 3 (sin cambios)...
        if (!_isCombatActive) return;

        BehaviorLogic();
        CheckBounds();
    }

/*
    public void SetupFormation(Vector3 targetPos)
    {
        _targetFormationPos = targetPos;
        _startFormationPos = transform.position; // Guardamos dónde empieza el viaje
        _isMovingToFormation = true;

        _cameFromBottom = transform.position.y < targetPos.y;

        if (_cameFromBottom)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
    }
*/
    protected override void Update()
    {
        // FASE 1: Descenso, ascenso o entrada lateral sincronizada hasta la posición de la fila
        if (_isMovingToFormation)
        {
            // Avanzamos el tiempo transcurrido
            _elapsedEntryTime += Time.deltaTime;
            
            // Calculamos el progreso de 0 a 1 de forma idéntica para TODOS los enemigos del bloque
            float progress = Mathf.Clamp01(_elapsedEntryTime / _entryDuration);

            // Usamos Lerp para que todas las naves lleguen exactamente al mismo tiempo (bloque rígido)
            transform.position = Vector3.Lerp(_startFormationPos, _targetFormationPos, progress);

            // Gestión del giro suave durante el trayecto
            if (_cameFromBottom || _cameFromSide)
            {
                float rotationDelayThreshold = 0.5f; // Esperan a la mitad del trayecto para girar

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

            // Cuando el progreso llega al 100%, termina la entrada
            if (progress >= 1f)
            {
                transform.position = _targetFormationPos;
                _isMovingToFormation = false;
                transform.rotation = Quaternion.identity; // Rectas y listas para combatir
            }
            return; 
        }

        // FASE 2 y 3 (sin cambios)...
        if (!_isCombatActive) return;

        BehaviorLogic();
        CheckBounds();
    }

    public void SetupFormation(Vector3 targetPos)
    {
        _targetFormationPos = targetPos;
        _startFormationPos = transform.position;
        _elapsedEntryTime = 0f; // Reiniciamos el crono de entrada
        _isMovingToFormation = true;

        _cameFromBottom = transform.position.y < targetPos.y;
        _cameFromSide = Mathf.Abs(transform.position.x - targetPos.x) > 1f;

        if (_cameFromBottom)
        {
            _initialZRotation = 180f;
            transform.rotation = Quaternion.Euler(0f, 0f, _initialZRotation);
        }
        else if (_cameFromSide)
        {
            // Si vienen de la izquierda miran a la derecha (-90), si vienen de la derecha miran a la izquierda (90)
            _initialZRotation = transform.position.x < targetPos.x ? -90f : 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, _initialZRotation);
        }
        else
        {
            _initialZRotation = 0f;
            transform.rotation = Quaternion.identity;
        }
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
