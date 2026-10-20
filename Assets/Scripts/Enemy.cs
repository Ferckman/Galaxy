using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Obstacles
{
    public Enemy(int health, int damage, int speed) : base(health, damage, speed)
    {

    }

    public override void DestroyMyself()
    {
        base.DestroyMyself();
    }

    public void Limit()
    {
        float _positionX = Random.Range(-7.5f, 7.5f);
        transform.position = new Vector3(_positionX, 7.0f, transform.position.z);
    }
}
