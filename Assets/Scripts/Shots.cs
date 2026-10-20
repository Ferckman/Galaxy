using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shots : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private protected int _speed;

    public Shots(int damage, int speed)
    {
        _damage = damage;
        _speed = speed;
    }

    public int GetDamage()
    {
        return _damage;
    }

    public virtual void Movement()
    {

    }

    private void Update()
    {
        Movement();
    }
}
