using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : Obstacles
{
    
    public Asteroid(int health, int damage, int speed) : base(health, damage, speed)
    {

    }

    public override void DestroyMyself()
    {
        base.DestroyMyself();
    }
}
