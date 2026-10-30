using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float _lifeTime;

    private void Awake()
    {
        Destroy(gameObject, _lifeTime);
    }
}