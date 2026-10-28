using UnityEngine;

public class Laser : Shots
{
    public override void Movement()
    {
        transform.Translate(_speed * Time.deltaTime * Vector3.up);
    }
}