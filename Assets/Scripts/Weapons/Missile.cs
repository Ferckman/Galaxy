using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Missile : Shots
{
    [Header("Configuration")]
    [SerializeField, Range(50f, 500f)] private float _rotateSpeed;
    [SerializeField, Min(0.5f)] private float _lifespanTime;

    private Rigidbody2D _rb;
    private Transform _target;
    private IMissileGuidance _guidanceStrategy;
    private float _spawnTime;
    private bool _isInitialized;

    [Header("Target Reticle")]
    [SerializeField] private GameObject _reticlePrefab;
    private GameObject _spawnedReticle;
    
    private Transform _currentTarget;

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    public void SetTarget(Transform target)
    {
        _currentTarget = target;

        // 1. Comprobamos si realmente se está llamando a este método y qué llega
        Debug.Log("SetTarget llamado. Objetivo recibido: " + (target != null ? target.name : "¡ES NULO!"));

        if (_currentTarget != null && _reticlePrefab != null)
        {
            _spawnedReticle = Instantiate(_reticlePrefab, _currentTarget.position, Quaternion.identity, _currentTarget);
            Debug.Log("¡Retícula instanciada con éxito en el enemigo!");
        }
        else
        {
            // 2. Si entra aquí, sabremos por qué no se crea
            Debug.LogWarning("No se pudo crear la retícula. ¿Falta arrastrar el prefab en el Inspector? Target nulo: " + (_currentTarget == null) + " | Prefab nulo: " + (_reticlePrefab == null));
        }
    }

    public void Initialize(Transform target, IMissileGuidance guidanceStrategy)
    {
        _currentTarget = target;
        _target = target;
        _guidanceStrategy = guidanceStrategy ?? new SmoothArcGuidance();
        _spawnTime = Time.time;
        _isInitialized = true;

        if (_currentTarget != null && _reticlePrefab != null)
        {
            // 1. Evitamos duplicados si otro misil ya le puso retícula
            bool alreadyHasReticle = false;
            foreach (Transform child in _currentTarget)
            {
                if (child.name.Contains(_reticlePrefab.name))
                {
                    alreadyHasReticle = true;
                    break;
                }
            }

            if (!alreadyHasReticle)
            {
                // 2. Lo instanciamos COMO HIJO del enemigo
                _spawnedReticle = Instantiate(_reticlePrefab, _currentTarget.position, Quaternion.identity, _currentTarget);
                
                // 3. TRUCO DE ESCALA: Contrarrestamos la escala global del padre (lossyScale)
                // para que la retícula conserve 100% el tamaño original de tu Prefab.
                Vector3 prefabScale = _reticlePrefab.transform.localScale;
                Vector3 parentScale = _currentTarget.lossyScale;

                _spawnedReticle.transform.localScale = new Vector3(
                    parentScale.x != 0 ? prefabScale.x / parentScale.x : prefabScale.x,
                    parentScale.y != 0 ? prefabScale.y / parentScale.y : prefabScale.y,
                    parentScale.z != 0 ? prefabScale.z / parentScale.z : prefabScale.z
                );
            }
        }
}

    private void Update()
    {
        if (_currentTarget == null && _spawnedReticle != null) Destroy(_spawnedReticle);
    }

    private void FixedUpdate()
    {
        if (!_isInitialized) return;

        // Comprobación de tiempo de vida (Fire-and-Forget timeout)
        if (Time.time - _spawnTime >= _lifespanTime)
        {
            TerminateMissile();
            return;
        }

        // Aplicar estrategia de guiado si el objetivo es válido
        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            float newRotation = _guidanceStrategy.CalculateTargetRotation(transform, _target, _rb, _rotateSpeed, Time.fixedDeltaTime);
            _rb.MoveRotation(newRotation);
        }

        // Propulsión constante hacia adelante en su propio eje local
        _rb.velocity = (Vector2)(transform.up * _speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Aquí se delegaría en un sistema de impactos/daño (Eventos o IDamageable)
        TerminateMissile();
    }

    private void TerminateMissile()
    {
        // Punto centralizado de destrucción para futuras implementaciones de Object Pooling
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // MUY IMPORTANTE: Cuando el misil impacta o se destruye, borramos la retícula del enemigo
        if (_spawnedReticle != null) Destroy(_spawnedReticle);
    }
}