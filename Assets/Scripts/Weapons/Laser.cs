/*using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Laser : Shots
{
    [SerializeField] private Vector3 _direction = Vector3.up;

    public override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * _direction);
    }

    public void SetDirection(Vector3 newDirection)
    {
        _direction = newDirection;
    }

    // Método para inyectar quién disparó el láser y evitar el fuego amigo
    public void Initialize(Collider2D shooterCollider)
    {

        Collider2D myCollider = GetComponentInChildren<Collider2D>();
        if (myCollider != null && shooterCollider != null)
        {
            // Ignoramos la colisión únicamente con el que disparó (Player o Enemigo concreto)
            Physics2D.IgnoreCollision(myCollider, shooterCollider);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.ReturnToPool(gameObject);
        else
            Destroy(gameObject);
    }
}*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Laser : Shots
{
    [SerializeField] private Vector3 _direction = Vector3.up;

    private Collider2D _lastShooterCollider;
    private Collider2D _myCollider;

    private void Awake()
    {
        _myCollider = GetComponentInChildren<Collider2D>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ClearIgnoredShooter();
    }

    private void OnDisable()
    {
        ClearIgnoredShooter();
    }

    public override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * _direction);
    }

    public void SetDirection(Vector3 newDirection)
    {
        _direction = newDirection;
    }

    // Inyecta quién disparó el láser para evitar fuego amigo y limpia el anterior si venía del pool
    public void SetShooterCollider(Collider2D shooterCollider)
    {
        ClearIgnoredShooter();

        _lastShooterCollider = shooterCollider;
        if (_myCollider != null && _lastShooterCollider != null)
        {
            Physics2D.IgnoreCollision(_myCollider, _lastShooterCollider, true);
        }
    }

    private void ClearIgnoredShooter()
    {
        if (_myCollider != null && _lastShooterCollider != null)
        {
            Physics2D.IgnoreCollision(_myCollider, _lastShooterCollider, false);
        }
        _lastShooterCollider = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ProjectilePool.Instance != null)
            ProjectilePool.Instance.ReturnToPool(gameObject);
        else
            Destroy(gameObject);
    }
}