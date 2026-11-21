using System.Collections.Generic;
using UnityEngine;

public class PrefabQueuePool
{
    private readonly Transform _parent;
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, Queue<GameObject>> _instanceToQueue = new Dictionary<GameObject, Queue<GameObject>>();

    public PrefabQueuePool(Transform parent)
    {
        _parent = parent;
    }

    public void RegisterAndPrewarm(GameObject prefab, int initialCount)
    {
        if (prefab == null) return;

        Queue<GameObject> queue = GetOrCreateQueue(prefab);

        for (int i = 0; i < initialCount; i++)
        {
            GameObject obj = Object.Instantiate(prefab, _parent);
            obj.SetActive(false);
            _instanceToQueue[obj] = queue;
            queue.Enqueue(obj);
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        Queue<GameObject> queue = GetOrCreateQueue(prefab);
        GameObject obj = null;

        while (queue.Count > 0 && obj == null)
        {
            obj = queue.Dequeue();
        }

        if (obj == null)
        {
            obj = Object.Instantiate(prefab, _parent);
            _instanceToQueue[obj] = queue;
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public bool Return(GameObject instance)
    {
        if (instance == null || !instance.activeSelf) return false;

        instance.transform.SetParent(_parent, false);
        instance.SetActive(false);

        if (_instanceToQueue.TryGetValue(instance, out Queue<GameObject> queue))
        {
            queue.Enqueue(instance);
            return true;
        }

        Object.Destroy(instance);
        return false;
    }

    private Queue<GameObject> GetOrCreateQueue(GameObject prefab)
    {
        if (!_pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pools[prefab] = queue;
        }
        return queue;
    }
}