using System.Collections.Generic;
using UnityEngine;

public abstract class Spawner : MonoBehaviour
{
    [SerializeField] private float _cooldown;
    [SerializeField] private protected GameManager gameManager;
    
    public float Cooldown
    {
        get => _cooldown;
        set => _cooldown = value;
    }

    protected abstract List<GameObject> list { get; set; }

    private protected void FindGameManager()
    {
        if (gameManager == null) gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    public virtual void DestroyConcreteObstacle(GameObject obstacle)
    {
        if (obstacle != null && list.Contains(obstacle)) list.Remove(obstacle);
    }

    public virtual void DestroyObstacles()
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] != null) Destroy(list[i].gameObject);
        }
        list.Clear();
    }

    /*public Transform LookForNearest(Transform target)
    {
        if (target == null || list == null || list.Count == 0) return null;

        Transform trans = null;
        float distNearest = Mathf.Infinity;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;

            float distance = Vector3.Distance(list[i].transform.position, target.position);
            if (distance < distNearest)
            {
                trans = list[i].transform;
                distNearest = distance;
            }
        }
        
        return trans;
    }
    */
}