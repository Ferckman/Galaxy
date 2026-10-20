using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Spawner : MonoBehaviour
{
    [SerializeField] private float _cooldown;
    [SerializeField] private protected GameManager gameManager;
    protected abstract List<GameObject> list { get; set; }

    public Spawner(float cooldown)
    {
        _cooldown = cooldown;
    }

    public void SetCooldown(float cooldown)
    {
        _cooldown = cooldown;
    }

    public float GetCooldown()
    {
        return _cooldown;
    }

    private protected void FindGameManager()
    {
        gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
    }

    public virtual void DestroyConcreteObstacle(GameObject obstacle)
    {
        list.Remove(obstacle);
    }

    public virtual void DestroyObstacles()
    {
        for(int i = 0; i < list.Count; i++)
        {
            Destroy(list[i].gameObject);
        }
    }

    public Transform LookForNearest(Transform target)
    {
        Transform trans = null;
        float distNearest = 999999;

        for(int i = 0; i < list.Count; i++)
        {
            float distance = Vector3.Distance(list[i].transform.position, target.transform.position);
            if(distance < distNearest)
            {
                trans = list[i].transform;
                distNearest = distance;
            }
        }
        
        return trans;
    }
}
