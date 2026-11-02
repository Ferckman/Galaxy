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
}