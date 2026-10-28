using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Obstacles
{
    public void Limit()
    {
        float positionX = Random.Range(-7.5f, 7.5f);
        transform.position = new Vector3(positionX, 7.0f, transform.position.z);
    }
}