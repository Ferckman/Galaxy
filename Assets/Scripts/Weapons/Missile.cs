using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Missile : Shots
{
    [Header("Configuration")]
    [SerializeField, Range(50f, 500f)] private float _rotateSpeed = 180f;
    [SerializeField, Min(0.5f)] private float _lifespanTime = 4f;

    private Rigidbody2D _rb;
    private Transform _target;
    private IMissileGuidance _guidanceStrategy;
    private float _spawnTime;
    private bool _isInitialized;

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    // Inyección de dependencias (Principio de Inversión de Dependencias)
    public void Initialize(Transform target, IMissileGuidance guidanceStrategy)
    {
        _target = target;
        _guidanceStrategy = guidanceStrategy ?? new SmoothArcGuidance();
        _spawnTime = Time.time;
        _isInitialized = true;
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
}