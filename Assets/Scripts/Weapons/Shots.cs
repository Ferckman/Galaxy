using System.Collections;
using UnityEngine;

public abstract class Shots : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private protected int _speed;

    public int Damage => _damage;

    private void Update()
    {
        Movement();
    }

    public virtual void Movement()
    {

    }
}