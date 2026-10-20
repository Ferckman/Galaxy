using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Shots
{
    public Laser(int damage, int speed): base(damage, speed)
    {

    }

    public override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.up);
    }
}
