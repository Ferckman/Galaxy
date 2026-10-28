using System.Collections;
using UnityEngine;

public abstract class Shots : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private protected int _speed = 10;

    public int Damage => _damage;

    private void Update()
    {
        Movement();
    }

    public virtual void Movement()
    {

    }
}