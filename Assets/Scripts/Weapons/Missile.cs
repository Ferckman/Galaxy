using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Missile : Shots
{
    [Header("Configuration")]
    [SerializeField, Range(50f, 500f)] private float _rotateSpeed = 450f;
    [SerializeField, Min(0.5f)] private float _lifespanTime = 20f;

    private Rigidbody2D _rb;
    private Transform _target;
    private Entity _targetEntity;
    private IMissileGuidance _guidanceStrategy;
    private float _spawnTime;
    private bool _isInitialized;
    private FadeOutTrail _fadeTrail;

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _fadeTrail = GetComponentInChildren<FadeOutTrail>(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_fadeTrail != null) _fadeTrail.ResetTrail();
    }

    private void OnDisable()
    {
        _isInitialized = false;
        _target = null;
        _guidanceStrategy = null;

        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        ClearTargetReticle();
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        UpdateReticle(target);
    }

    public void Initialize(Transform target, IMissileGuidance guidanceStrategy)
    {
        _target = target;
        _guidanceStrategy = guidanceStrategy ?? new SmoothArcGuidance();
        _spawnTime = Time.time;
        _isInitialized = true;

        UpdateReticle(target);
    }

    private void Update()
    {
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            ClearTargetReticle();
        }
    }

    private void FixedUpdate()
    {
        if (!_isInitialized) return;

        if (Time.time - _spawnTime >= _lifespanTime)
        {
            TerminateMissile();
            return;
        }

        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            float newRotation = _guidanceStrategy.CalculateTargetRotation(transform, _target, _rb, _rotateSpeed, Time.fixedDeltaTime);
            _rb.MoveRotation(newRotation);
        }

        _rb.velocity = (Vector2)(transform.up * _speed);
    }

    public void TerminateMissile()
    {
        if (!gameObject.activeInHierarchy) return;

        if (_fadeTrail != null) _fadeTrail.BeginFade();

        ClearTargetReticle();

        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void UpdateReticle(Transform target)
    {
        if (_targetEntity != null && (target == null || _targetEntity.transform != target))
        {
            _targetEntity.SetTargetReticle(false);
            _targetEntity = null;
        }

        if (target != null && target.TryGetComponent<Entity>(out Entity entity))
        {
            _targetEntity = entity;
            _targetEntity.SetTargetReticle(true);
        }
    }

    private void ClearTargetReticle()
    {
        if (_targetEntity != null)
        {
            _targetEntity.SetTargetReticle(false);
            _targetEntity = null;
        }
    }
}