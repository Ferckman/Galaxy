using UnityEngine;

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
}