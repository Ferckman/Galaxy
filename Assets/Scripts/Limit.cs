using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limit : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    public Limit()
    {

    }

    private void Awake()
    {
        gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Enemy"))
        {
            if(other.TryGetComponent<Enemy>(out var enemy))
                enemy.Limit();
        }
        else if(other.CompareTag("Shots"))
        {
            if(other.TryGetComponent<Laser>(out var laser))
                Destroy(other.gameObject);
        }
        else if(other.CompareTag("Powerup"))
        {
            //Delete from generator list
            gameManager.DestroyConcreteElement(other.gameObject);
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Shots"))
            Destroy(other.gameObject);
    }
}
