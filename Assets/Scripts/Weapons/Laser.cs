using UnityEngine;

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
        Destroy(gameObject);
    }
}